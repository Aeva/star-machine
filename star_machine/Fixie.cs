
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Quaternion = System.Numerics.Quaternion;


namespace FixedPoint;


public record struct FixedInt
{
    public Int64 Value;

    public const int UnitOffset = 16;

    public const long UnitValue = 1 << UnitOffset;

    public const long DecimalMask = UnitValue - 1;

    public override string ToString()
    {
        return ToDouble().ToString();
    }

    public FixedInt(FixedInt InValue)
    {
        Value = InValue.Value;
    }

    public FixedInt(int InValue)
    {
        Value = (long)(InValue) << UnitOffset;
    }

    public FixedInt(long InValue)
    {
        Value = InValue << UnitOffset;
    }

    public FixedInt(float InValue)
    {
        float Floor = (float)Math.Floor(InValue);
        long Whole = ((long)Floor) << UnitOffset;
        long Fract = (long)((InValue - Floor) * (float)UnitValue);
        Value = Whole | Fract;
    }

    public FixedInt(double InValue)
    {
        double Floor = Math.Floor(InValue);
        long Whole = ((long)Floor) << UnitOffset;
        long Fract = (long)((InValue - Floor) * (double)UnitValue);
        Value = Whole | Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator FixedInt(int Other) => new FixedInt(Other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator FixedInt(long Other) => new FixedInt(Other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator FixedInt(float Other) => new FixedInt(Other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator FixedInt(double Other) => new FixedInt(Other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ToSingle()
    {
        float Whole = (float)(Value >> UnitOffset);
        float Fract = (float)(Value & DecimalMask) / (float)UnitValue;
        return Whole + Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ToDouble()
    {
        double Whole = (double)(Value >> UnitOffset);
        double Fract = (double)(Value & DecimalMask) / (double)UnitValue;
        return Whole + Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator int(FixedInt Other) => (int)(Other.Value >> UnitOffset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator long(FixedInt Other) => (long)(Other.Value >> UnitOffset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator float(FixedInt Other) => Other.ToSingle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator double(FixedInt Other) => Other.ToDouble();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (UInt32 Inner, UInt32 Outer) Split()
    {
        UInt64 UnsignedValue = (UInt64)Value;

        (UInt32 Inner, UInt32 Outer) Result;
        Result.Inner = (UInt32)(UnsignedValue);
        Result.Outer = (UInt32)(UnsignedValue >> 32);
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt Min(FixedInt LHS, FixedInt RHS)
    {
        LHS.Value = Math.Min(LHS.Value, RHS.Value);
        return LHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt Max(FixedInt LHS, FixedInt RHS)
    {
        LHS.Value = Math.Max(LHS.Value, RHS.Value);
        return LHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator +(FixedInt Unary) => Unary;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator -(FixedInt Unary)
    {
        Unary.Value = -(Unary.Value);
        return Unary;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator +(FixedInt LHS, FixedInt RHS)
    {
        FixedInt Result;
        Result.Value = LHS.Value + RHS.Value;
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator -(FixedInt LHS, FixedInt RHS)
    {
        FixedInt Result;
        Result.Value = LHS.Value - RHS.Value;
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator *(FixedInt LHS, FixedInt RHS)
    {
        FixedInt Result = new();
#if true
        // This is likely slower.
        Int128 LongerLHS = (Int128)LHS.Value;
        Int128 LongerRHS = (Int128)RHS.Value;
        Result.Value = (Int64)((LongerLHS * LongerRHS) >> UnitOffset);
#else
        // This can underflow.
        Result.Value = (LHS.Value * RHS.Value) >> UnitOffset;
#endif
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator /(FixedInt LHS, FixedInt RHS)
    {
        FixedInt Result = new();
#if true
        // This is likely slower.
        Int128 LongerLHS = (Int128)LHS.Value;
        Int128 LongerRHS = (Int128)RHS.Value;
        Result.Value = (Int64)((LongerLHS << UnitOffset) / LongerRHS);
#else
        // This can underflow.
        Result.Value = (LHS.Value << UnitOffset) / RHS.Value;
#endif
        return Result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator +(FixedInt LHS, float RHS)
    {
        return LHS + (FixedInt)RHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator -(FixedInt LHS, float RHS)
    {
        return LHS - (FixedInt)RHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator *(FixedInt LHS, float RHS)
    {
        return LHS * (FixedInt)RHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator /(FixedInt LHS, float RHS)
    {
        return LHS / (FixedInt)RHS;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator +(float LHS, FixedInt RHS)
    {
        return (FixedInt)LHS + RHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator -(float LHS, FixedInt RHS)
    {
        return (FixedInt)LHS - RHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator *(float LHS, FixedInt RHS)
    {
        return (FixedInt)LHS * RHS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt operator /(float LHS, FixedInt RHS)
    {
        return (FixedInt)LHS / RHS;
    }


    public static bool operator<(FixedInt LHS, FixedInt RHS)
    {
        return LHS.Value < RHS.Value;
    }

    public static bool operator>(FixedInt LHS, FixedInt RHS)
    {
        return LHS.Value > RHS.Value;
    }

    public static bool operator<=(FixedInt LHS, FixedInt RHS)
    {
        return LHS.Value <= RHS.Value;
    }

    public static bool operator>=(FixedInt LHS, FixedInt RHS)
    {
        return LHS.Value >= RHS.Value;
    }

    public override int GetHashCode()
    {
        return unchecked((int)((long)Value)) ^ (int)(Value >> 32);
    }
}


[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
public record struct Fixie
{
    [System.Runtime.CompilerServices.InlineArray(3)]
    public struct LanesArray
    {
        public FixedInt _element0;
    }

    [System.Runtime.InteropServices.FieldOffset(0)]
    public LanesArray Lanes;

    [System.Runtime.InteropServices.FieldOffset(0)]
    public FixedInt X;

    [System.Runtime.InteropServices.FieldOffset(8)]
    public FixedInt Y;

    [System.Runtime.InteropServices.FieldOffset(16)]
    public FixedInt Z;

    public const int Count = 3;

    public override string ToString()
    {
        return $"<Fixie {X}, {Y}, {Z}>";
    }

    public Fixie()
    {
        Unsafe.SkipInit(out Lanes);
        X = (FixedInt)0;
        Y = (FixedInt)0;
        Z = (FixedInt)0;
    }

    public Fixie(FixedInt InAll)
    {
        Unsafe.SkipInit(out Lanes);
        X = InAll;
        Y = InAll;
        Z = InAll;
    }

    public Fixie(FixedInt InX, FixedInt InY, FixedInt InZ)
    {
        Unsafe.SkipInit(out Lanes);
        X = InX;
        Y = InY;
        Z = InZ;
    }

    public Fixie(int InAll)
    {
        Unsafe.SkipInit(out Lanes);
        FixedInt Fnord = new(InAll);
        X = Fnord;
        Y = Fnord;
        Z = Fnord;
    }

    public Fixie(int InX, int InY, int InZ)
    {
        Unsafe.SkipInit(out Lanes);
        X = new(InX);
        Y = new(InY);
        Z = new(InZ);
    }

    public Fixie(float InAll)
    {
        Unsafe.SkipInit(out Lanes);
        FixedInt Fnord = new(InAll);
        X = Fnord;
        Y = Fnord;
        Z = Fnord;
    }

    public Fixie(float InX, float InY, float InZ)
    {
        Unsafe.SkipInit(out Lanes);
        X = new(InX);
        Y = new(InY);
        Z = new(InZ);
    }

    public Fixie(Vector3 InVector)
    {
        Unsafe.SkipInit(out Lanes);
        X = new(InVector.X);
        Y = new(InVector.Y);
        Z = new(InVector.Z);
    }

    public static Fixie Zero
    {
        get => new Fixie(0, 0, 0);
    }

    public static Fixie UnitX
    {
        get => new Fixie(1, 0, 0);
    }

    public static Fixie UnitY
    {
        get => new Fixie(0, 1, 0);
    }

    public static Fixie UnitZ
    {
        get => new Fixie(0, 0, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToVector3()
    {
        return new Vector3((float)X, (float)Y, (float)Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4 ToVector4(float W = 1.0f)
    {
        return new Vector4((float)X, (float)Y, (float)Z, W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt Dot(Fixie LHS, Fixie RHS)
    {
        FixedInt Result = (FixedInt)0;
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result += LHS.Lanes[Lane] * RHS.Lanes[Lane];
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Fixie Normal, FixedInt Mag) Normalize(Fixie Unary)
    {
        (Fixie Normal, FixedInt Mag) Result = (Zero, (FixedInt)0);
        FixedInt MagSquared = FixedInt.Max(Dot(Unary, Unary), (FixedInt)0);
        if (MagSquared > (FixedInt)0)
        {
            Result.Mag = (FixedInt)Math.Sqrt((double)MagSquared);
            Result.Normal = Unary / Result.Mag;
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie Transform(Fixie FixedPointVector, Quaternion RotateBy)
    {
        // TODO Implement quaternion rotation directly.
        return new Fixie(Vector3.Transform(FixedPointVector.ToVector3(), RotateBy));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator +(Fixie Unary) => Unary;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator -(Fixie Unary)
    {
        Unary.X = -Unary.X;
        Unary.Y = -Unary.Y;
        Unary.Z = -Unary.Z;
        return Unary;
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
            Result.Lanes[Lane] = LHS.Lanes[Lane] * RHS.Lanes[Lane];
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator /(Fixie LHS, Fixie RHS)
    {
        Fixie Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result.Lanes[Lane] = LHS.Lanes[Lane] / RHS.Lanes[Lane];
        }
        return Result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator +(Fixie LHS, FixedInt RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS + Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator -(Fixie LHS, FixedInt RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS - Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator *(Fixie LHS, FixedInt RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS * Substitute;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator /(Fixie LHS, FixedInt RHS)
    {
        Fixie Substitute = new(RHS);
        return LHS / Substitute;
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
}


public struct FixedPointTests
{
    public static void PreflightCheck()
    {
        // Do negative numbers work?
        {
            FixedInt FixedPoint = new(-10.5f);
            float FloatingPoint = FixedPoint.ToSingle();
            Trace.Assert(FloatingPoint == -10.5f);
        }

        // Does subtraction work?
        {
            FixedInt A = new(-1.0f);
            FixedInt B = new(+1.0f);
            FixedInt C = A - B;
            Trace.Assert(C.ToSingle() == -2.0f);
        }

        // Does multiplying negative numbers work?
        {
            FixedInt A = new(-1.0f);
            FixedInt B = A * -2.0f;
            Trace.Assert(B.ToSingle() == 2.0f);
        }

        // Does dividing negative numbers work?
        {
            FixedInt A = new(1.0f);
            FixedInt B = -A / -2.0f;
            Trace.Assert(B.ToSingle() == 0.5f);
        }

        // Does Normalize work?
        {
            (Fixie FixedPointNormal, FixedInt Mag) = Fixie.Normalize(Fixie.UnitX + Fixie.UnitY);
            Trace.Assert(Mag > (FixedInt)0);

            Vector3 Equiv = Vector3.Normalize(Vector3.UnitX + Vector3.UnitY);
            float Delta = Math.Abs(Equiv.X - (float)FixedPointNormal.X);

            Trace.Assert(Delta < 0.00001f);
        }
    }
}
