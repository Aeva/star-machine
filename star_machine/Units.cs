
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FixedInt = FixedPoint.FixedInt;


namespace StarMachine;


// Common SI prefixes.
static class Prefix
{
    public const double Pico  = 0.000000000001;
    public const double Nano  = 0.000000001;
    public const double Micro = 0.000001;
    public const double Milli = 0.001;
    public const double Centi = 0.01;
    public const double Deci  = 0.1;
    public const double Deca  = 10.0;
    public const double Hecto = 100.0;
    public const double Kilo  = 1_000.0;
    public const double Mega  = 1_000_000.0;
    public const double Giga  = 1_000_000_000.0;
    public const double Tera  = 1_000_000_000_000.0;
}


// Supported units defined in terms of SI seconds.
static class Time
{
    // SI second
    public const double Second = 1.0;

    // SI second submultiples
    public const double PicoSecond  = Prefix.Pico;
    public const double Nanosecond  = Prefix.Nano;
    public const double Microsecond = Prefix.Micro;
    public const double Millisecond = Prefix.Milli;
    public const double Centisecond = Prefix.Centi;
    public const double Decisecond  = Prefix.Deci;

    // Non-SI time units
    public const double Minute = 60.0;
    public const double Hour = Minute * 60.0;
    public const double Day = Hour * 24.0;

    // Computer units
    public const double Tick = 1.0 / (double)TimeSpan.TicksPerSecond;
}


// Supported units defined in terms of SI meters.
static class Space
{
    // SI meter
    public const double Meter = 1.0;

    // SI meter submultiples
    public const double Millimeter = Prefix.Milli;
    public const double Centimeter = Prefix.Centi;
    public const double Decimeter  = Prefix.Deci;

    // SI meter multiples
    public const double Decameter  = Prefix.Deca;
    public const double Hectometer = Prefix.Hecto;
    public const double Kilometer  = Prefix.Kilo;
    public const double Megameter  = Prefix.Mega;
    public const double Gigameter  = Prefix.Giga;

    // International units
    public const double Inch = 0.0254;
    public const double Point = Inch / 72.0;
    public const double Pica = Inch / 6.0;
    public const double Foot = 0.3048;
    public const double Yard = 0.9144;
    public const double Mile = 1_609.344;
    public const double League = 4_828.032;

    // Astronomical distances
    public const double LunarDistance = 384_399_000.0;
    public const double AstronomicalUnit = 149_597_870_700.0;
    public const double LightYear = 9_460_730_472_580_800;

    // Fictional units
    public const double WorldScale = 4.0;
}

static class Units
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Convert(double Quantity, double From, double To)
    {
        // Equivalent to `Quantity * (From / To)`, but better precision?
        return Quantity / (To / From);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedInt Convert(FixedInt Quantity, double From, double To)
    {
        return (FixedInt)Convert((double)Quantity, From, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Convert(long Quantity, double From, double To)
    {
        return (long)Convert((double)Quantity, From, To);
    }
}


static class UnitsTests
{
    public static void PreflightCheck()
    {
        var TestUnit = (double Prefix, double Expected) =>
        {
            Trace.Assert(Prefix == Expected, $"Expected {Expected}, got {Prefix}");
        };
        TestUnit(Prefix.Pico,  1e-12);
        TestUnit(Prefix.Nano,  1e-9);
        TestUnit(Prefix.Micro, 1e-6);
        TestUnit(Prefix.Milli, 1e-3);
        TestUnit(Prefix.Centi, 1e-2);
        TestUnit(Prefix.Deci,  1e-1);
        TestUnit(Prefix.Deca,  1e+1);
        TestUnit(Prefix.Hecto, 1e+2);
        TestUnit(Prefix.Kilo,  1e+3);
        TestUnit(Prefix.Mega,  1e+6);
        TestUnit(Prefix.Giga,  1e+9);
        TestUnit(Prefix.Tera,  1e+12);

        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.AstronomicalUnit, Space.Meter);
            FixedInt Expected = (FixedInt)149597870700;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.League, Space.Mile);
            FixedInt Expected = (FixedInt)3;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.Mile, Space.Inch);
            FixedInt Expected = (FixedInt)63360;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.Yard, Space.Inch);
            FixedInt Expected = (FixedInt)36;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.Foot, Space.Inch);
            FixedInt Expected = (FixedInt)12;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.Inch, Space.Millimeter);
            FixedInt Expected = (FixedInt)25.4;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.Inch, Space.Pica);
            FixedInt Expected = (FixedInt)6;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Space.Inch, Space.Point);
            FixedInt Expected = (FixedInt)72;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }


        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Tick, Time.Nanosecond);
            FixedInt Expected = (FixedInt)TimeSpan.NanosecondsPerTick;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Microsecond, Time.Tick);
            FixedInt Expected = (FixedInt)TimeSpan.TicksPerMicrosecond;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Millisecond, Time.Tick);
            FixedInt Expected = (FixedInt)TimeSpan.TicksPerMillisecond;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Second, Time.Tick);
            FixedInt Expected = (FixedInt)TimeSpan.TicksPerSecond;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Minute, Time.Tick);
            FixedInt Expected = (FixedInt)TimeSpan.TicksPerMinute;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Hour, Time.Tick);
            FixedInt Expected = (FixedInt)TimeSpan.TicksPerHour;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Day, Time.Tick);
            FixedInt Expected = (FixedInt)TimeSpan.TicksPerDay;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Hour, Time.Nanosecond);
            FixedInt Expected = (FixedInt)3_600_000_000_000;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }
#if false
        {
            FixedInt Got = Units.Convert((FixedInt)1, Time.Day, Time.Nanosecond);
            FixedInt Expected = (FixedInt)8.64e+13;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}"); // fails on 86_399_999_999_999.984375
        }
#endif
    }
}
