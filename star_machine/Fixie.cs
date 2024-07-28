
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Quaternion = System.Numerics.Quaternion;


namespace FixedPoint;


[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public struct Fixie
{
    [System.Runtime.CompilerServices.InlineArray(3)]
    public struct LanesArray
    {
        public long _element0;
    }

    [System.Runtime.InteropServices.FieldOffset(0)]
    public LanesArray Lanes;

    [System.Runtime.InteropServices.FieldOffset(0)]
    public long X;

    [System.Runtime.InteropServices.FieldOffset(8)]
    public long Y;

    [System.Runtime.InteropServices.FieldOffset(16)]
    public long Z;

    public const int Count = 3;

    public const int UnitOffset = 16;

    public const long UnitValue = 1 << UnitOffset;

    public const long DecimalMask = UnitValue - 1;

    public Fixie()
    {
        Unsafe.SkipInit(out Lanes);
        X = 0;
        Y = 0;
        Z = 0;
    }

    public Fixie(long InAll)
    {
        Unsafe.SkipInit(out Lanes);
        X = InAll;
        Y = InAll;
        Z = InAll;
    }

    public Fixie(long InX, long InY, long InZ)
    {
        Unsafe.SkipInit(out Lanes);
        X = InX;
        Y = InY;
        Z = InZ;
    }

    public Fixie(float InAll)
    {
        Unsafe.SkipInit(out Lanes);
        long Fnord = ToFixed(InAll);
        X = Fnord;
        Y = Fnord;
        Z = Fnord;
    }

    public Fixie(float InX, float InY, float InZ)
    {
        Unsafe.SkipInit(out Lanes);
        X = ToFixed(InX);
        Y = ToFixed(InY);
        Z = ToFixed(InZ);
    }

    public Fixie(Vector3 InVector)
    {
        Unsafe.SkipInit(out X);
        Unsafe.SkipInit(out Y);
        Unsafe.SkipInit(out Z);
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Lanes[Lane] = ToFixed(InVector[Lane]);
        }
    }

    public static Fixie Zero
    {
        get => new Fixie(0, 0, 0);
    }

    public static Fixie UnitX
    {
        get => new Fixie(1.0f, 0.0f, 0.0f);
    }

    public static Fixie UnitY
    {
        get => new Fixie(0.0f, 1.0f, 0.0f);
    }

    public static Fixie UnitZ
    {
        get => new Fixie(0.0f, 0.0f, 1.0f);
    }

    public Vector3 ToVector3()
    {
        Vector3 Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result[Lane] = ToFloat(Lanes[Lane]);
        }
        return Result;
    }

    public Vector4 ToVector4()
    {
        Vector4 Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result[Lane] = ToFloat(Lanes[Lane]);
        }
        Result.W = 1.0f;
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long FixedPointMul(long LHS, long RHS)
    {
#if true
        // This is likely slower.
        Int128 LongerLHS = (Int128)LHS;
        Int128 LongerRHS = (Int128)RHS;
        return (long)((LongerLHS * LongerRHS) >> UnitOffset);
#else
        // This can underflow.
        return (LHS * RHS) >> UnitOffset;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long FixedPointDiv(long LHS, long RHS)
    {
#if true
        // This is likely slower.
        Int128 LongerLHS = (Int128)LHS;
        Int128 LongerRHS = (Int128)RHS;
        return (long)((LongerLHS << UnitOffset) / LongerRHS);
#else
        // This can underflow.
        return (LHS << UnitOffset) / RHS;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToFixed(float FloatingPoint)
    {
        float Floor = (float)Math.Floor(FloatingPoint);
        long Whole = ((long)Floor) << UnitOffset;
        long Fract = (long)((FloatingPoint - Floor) * (float)UnitValue);
        return Whole | Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToFixed(double FloatingPoint)
    {
        double Floor = Math.Floor(FloatingPoint);
        long Whole = ((long)Floor) << UnitOffset;
        long Fract = (long)((FloatingPoint - Floor) * (double)UnitValue);
        return Whole | Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToFloat(long FixedPoint)
    {
        float Whole = (float)(FixedPoint >> UnitOffset);
        float Fract = (float)(FixedPoint & DecimalMask) / (float)UnitValue;
        return Whole + Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToDouble(long FixedPoint)
    {
        double Whole = (double)(FixedPoint >> UnitOffset);
        double Fract = (double)(FixedPoint & DecimalMask) / (double)UnitValue;
        return Whole + Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Fixie LHS, Fixie RHS)
    {
        long Result = 0;
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result += FixedPointMul(LHS.Lanes[Lane], RHS.Lanes[Lane]);
        }
        return ToFloat(Result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Fixie Normal, float Mag) Normalize(Fixie Unary)
    {
        (Fixie Normal, float Mag) Result = (Zero, 0);
        float MagSquared = Math.Max(Dot(Unary, Unary), 0);
        if (MagSquared > 0)
        {

            Fixie Mag = new((float)Math.Sqrt(MagSquared));
            Result.Normal = Unary / Mag;
            Result.Mag = ToFloat(Mag.X);
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie Round(Fixie Vec, long PreClampDivisor)
    {
        PreClampDivisor = Math.Min(PreClampDivisor, 1L);
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            long Channel = FixedPointDiv(Vec.Lanes[Lane], PreClampDivisor);
            Channel = Channel & (~DecimalMask);
            Vec.Lanes[Lane] = FixedPointMul(Channel, PreClampDivisor);
        }
        return Vec;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie Round(Fixie Vec, float PreClampDivisor = 1.0f)
    {
        return Round(Vec, ToFixed(PreClampDivisor));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie Round(Fixie Vec, double PreClampDivisor = 1.0)
    {
        return Round(Vec, ToFixed(PreClampDivisor));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie Transform(Fixie FixedPointVector, Quaternion RotateBy)
    {
        return new Fixie(Vector3.Transform(FixedPointVector.ToVector3(), RotateBy));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator +(Fixie Unary) => Unary;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator -(Fixie Unary) => new Fixie(-Unary.X, -Unary.Y, -Unary.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Fixie LHS, Fixie RHS)
    {
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            if (LHS.Lanes[Lane] == RHS.Lanes[Lane])
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Fixie LHS, Fixie RHS)
    {
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            if (LHS.Lanes[Lane] != RHS.Lanes[Lane])
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator +(Fixie LHS, Fixie RHS)
    {
        Fixie Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result.Lanes[Lane] = LHS.Lanes[Lane] + RHS.Lanes[Lane];
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator -(Fixie LHS, Fixie RHS)
    {
        Fixie Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result.Lanes[Lane] = LHS.Lanes[Lane] - RHS.Lanes[Lane];
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator *(Fixie LHS, Fixie RHS)
    {
        Fixie Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result.Lanes[Lane] = FixedPointMul(LHS.Lanes[Lane], RHS.Lanes[Lane]);
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator /(Fixie LHS, Fixie RHS)
    {
        Fixie Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result.Lanes[Lane] = FixedPointDiv(LHS.Lanes[Lane], RHS.Lanes[Lane]);
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator %(Fixie LHS, Fixie RHS)
    {
        Fixie Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result.Lanes[Lane] = LHS.Lanes[Lane] % RHS.Lanes[Lane];
        }
        return Result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator +(Fixie LHS, float RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS + Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator -(Fixie LHS, float RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS - Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator *(Fixie LHS, float RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS * Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator /(Fixie LHS, float RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS / Substitute;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator +(Fixie LHS, Vector3 RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS + Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator -(Fixie LHS, Vector3 RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS - Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator *(Fixie LHS, Vector3 RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS * Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator /(Fixie LHS, Vector3 RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS / Substitute;
    }

    public static void PreflightCheck()
    {
        // Do negative numbers work?
        {
            long FixedPoint = ToFixed(-10.5f);
            float FloatingPoint = ToFloat(FixedPoint);
            Trace.Assert(FloatingPoint == -10.5f);
        }

        // Does subtraction work?
        {
            Fixie A = -Fixie.UnitX;
            Fixie B = new(1.0f);
            Fixie C = A - B;
            Trace.Assert(ToFloat(C.X) == -2.0f);
            Trace.Assert(ToFloat(C.Y) == -1.0f);
            Trace.Assert(ToFloat(C.Z) == -1.0f);
        }

        // Does multiplying negative numbers work?
        {
            Fixie A = -Fixie.UnitX;
            Fixie B = A * -2.0f;
            Trace.Assert(ToFloat(B.X) == 2.0f);
        }

        // Does dividing negative numbers work?
        {
            Fixie A = -Fixie.UnitX;
            Fixie B = A / -2.0f;
            Trace.Assert(ToFloat(B.X) == 0.5f);
        }

        // Does normalize work?
        {
            Fixie A = Fixie.UnitX + Fixie.UnitY;
            (Fixie B, float Mag) = Fixie.Normalize(A);
            Trace.Assert(Mag > 1.0f);

            Vector3 Equiv = new(1.0f, 1.0f, 0.0f);
            Equiv = Vector3.Normalize(Equiv);

            Trace.Assert(Math.Abs(B.ToVector3().X - Equiv.X) < 0.00001f);
        }
    }
}
