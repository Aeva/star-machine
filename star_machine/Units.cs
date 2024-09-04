
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FixedInt = FixedPoint.FixedInt;


namespace StarMachine;


// Common SI prefixes.
static class Prefix
{
    // These are written as double precision floats because fixed point 48.16
    // and 64.64 can't express the most of the fractional prefixes exactly.
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
    public const double Picosecond  = Prefix.Pico;
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
    public const double LightSecond = 299_792_458.0;
    public const double LightHour = 1_079_252_848_800.0;
    public const double LightYear = 9_460_730_472_580_800.0;

    // Fictional units
    public const double WorldUnit = 0.25;
}


static class UnitConversions
{
    // Always list the ratio with the larger magnitude first.

    public const double DaysToPicoseconds = Time.Day / Time.Picosecond;
    public const double PicosecondsToDays = Time.Picosecond / Time.Day;

    public const double DaysToNanoseconds = Time.Day / Time.Nanosecond;
    public const double NanosecondsToDays = Time.Nanosecond / Time.Day;

    public const double DaysToSeconds = Time.Day / Time.Second;
    public const double SecondsToDays = Time.Second / Time.Day;

    public const double HoursToSeconds = Time.Hour / Time.Second;
    public const double SecondsToHours = Time.Second / Time.Hour;

    public const double MilesToMeters = Space.Mile / Space.Meter;
    public const double MetersToMiles = Space.Meter / Space.Mile;

    public const double MilesToKilometers = Space.Mile / Space.Kilometer;
    public const double KilometersToMiles = Space.Kilometer / Space.Mile;

    public const double MetersToWorldUnits = Space.Meter / Space.WorldUnit;
    public const double WorldUnitsToMeters = Space.WorldUnit / Space.Meter;

    public const double MilesToWorldUnits = Space.Mile / Space.WorldUnit;
    public const double WorldUnitsToMiles = Space.WorldUnit / Space.Mile;

    public const double LightSecondsToWorldUnits = Space.LightSecond / Space.WorldUnit;
    public const double WorldUnitsToLightSeconds = Space.WorldUnit / Space.LightSecond;

    // The velocity conversion constants are generally lossy and should only be used for display.

    public const double MetersPerSecondToMilesPerHour = Time.Hour / Space.Mile;
    public const double MilesPerHourToMetersPerSecond = Space.Mile / Time.Hour;

    public const double MilesPerHourToKilometersPerHour = MilesToKilometers;
    public const double KilometersPerHourToMilesPerHour = KilometersToMiles;

    public const double WorldUnitsPerSecondToMilesPerHour = (Space.WorldUnit / Space.Mile) / (Time.Second / Time.Hour);
    public const double MilesPerHourToWorldUnitsPerSecond = (Space.Mile / Space.WorldUnit) / (Time.Hour / Time.Second);

    public const double MilesPerSecondToWorldUnitsPerSecond = MilesToWorldUnits;
    public const double WorldUnitsPerSecondToMilesPerSecond = WorldUnitsToMeters;

    public const double WorldUnitPerSecondToSpeedOfLight = WorldUnitsToLightSeconds;
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

        var TestSymmetry = (double From, double To) =>
        {
            double Forward = From / To;
            double Backward = To / From;
            double Result = Forward * Backward;
            Trace.Assert(Result == 1.0, $"({From} / {To}) * ({To} / {From}) != 1.0");
        };
        TestSymmetry(Time.Day, Time.Picosecond);

        {
            // Note: FixedInt is not wide enough to express a day's worth of picoseconds.
            long Picoseconds = 86400000000000000L;
            long Got = Picoseconds / (long)UnitConversions.DaysToPicoseconds;
            long Expected = 1L;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }

        {
            FixedInt Nanoseconds = (FixedInt)86400000000000L;
            FixedInt Got = Nanoseconds / (FixedInt)UnitConversions.DaysToNanoseconds;
            FixedInt Expected = (FixedInt)1L;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }

        {
            FixedInt Seconds = (FixedInt)(60 * 60 * 24);
            FixedInt Got = Seconds / (FixedInt)UnitConversions.DaysToSeconds;
            FixedInt Expected = (FixedInt)1L;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }

        {
            FixedInt WorldUnits = (FixedInt)4;
            FixedInt Got = WorldUnits / (FixedInt)UnitConversions.MetersToWorldUnits;
            FixedInt Expected = (FixedInt)1L;
            Trace.Assert(Got == Expected, $"Expected {Expected}, got {Got}");
        }

        {
            Trace.Assert(UnitConversions.MilesPerHourToMetersPerSecond == 0.44704);
        }
    }
}
