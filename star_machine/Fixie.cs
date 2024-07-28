
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;


namespace StarMachine;


[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
struct Fixie
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
    private static long ToFixed(float FloatingPoint)
    {
        float Floor = (float)Math.Floor(FloatingPoint);
        long Whole = ((long)Floor) << UnitOffset;
        long Fract = (long)((FloatingPoint - Floor) * (float)UnitValue);
        return Whole | Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long ToFixed(double FloatingPoint)
    {
        double Floor = Math.Floor(FloatingPoint);
        long Whole = ((long)Floor) << UnitOffset;
        long Fract = (long)((FloatingPoint - Floor) * (double)UnitValue);
        return Whole | Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ToFloat(long FixedPoint)
    {
        float Whole = (float)(FixedPoint >> UnitOffset);
        float Fract = (float)(FixedPoint & DecimalMask) / (float)UnitValue;
        return Whole + Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ToDouble(long FixedPoint)
    {
        double Whole = (double)(FixedPoint >> UnitOffset);
        double Fract = (double)(FixedPoint & DecimalMask) / (double)UnitValue;
        return Whole + Fract;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Dot(Fixie LHS, Fixie RHS)
    {
        long Result = 0;
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result += LHS.Lanes[Lane] * RHS.Lanes[Lane];
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Fixie Normal, long Mag) Normalize(Fixie Unary)
    {
        (Fixie Normal, long Mag) Result = (Zero, 0);
        long MagSquared = Math.Max(Dot(Unary, Unary), 0);
        if (MagSquared > 0)
        {

            Fixie Mag = new((long)Math.Sqrt(MagSquared));
            Result.Normal = Unary / Mag;
            Result.Mag = Mag.X;
        }
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator +(Fixie Unary) => Unary;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixie operator -(Fixie Unary) => new Fixie(Unary.X, Unary.Y, Unary.Z);

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
    public static Fixie operator %(Fixie LHS, Fixie RHS)
    {
        Fixie Result = new();
        for (int Lane = 0; Lane < Count; ++Lane)
        {
            Result.Lanes[Lane] = LHS.Lanes[Lane] % RHS.Lanes[Lane];
        }
        return Result;
    }
}
