
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

using static Evaluator.ProgramBuffer;

namespace StarMachine;


class TracingGrist
{
    public readonly Evaluator.ProgramBuffer Model;

    public TracingGrist(Evaluator.ProgramBuffer InModel)
    {
        Model = InModel;
    }

    public float Eval(Vector3 Point)
    {
        // hack to force the scene to repeat
        float Span = 10.0f;
        Point.X -= Span * (float)Math.Round(Point.X / Span);
        Point.Y -= Span * (float)Math.Round(Point.Y / Span);

        return Model.Eval(Point);
    }

    public Vector3 Gradient(Vector3 Point)
    {
        // hack to force the scene to repeat
        float Span = 10.0f;
        Point.X -= Span * (float)Math.Round(Point.X / Span);
        Point.Y -= Span * (float)Math.Round(Point.Y / Span);

        return Model.Gradient(Point);
    }

    public (bool, Vector3) Trace(Vector3 Start, Vector3 Stop)
    {
        Vector3 Point = Start;
        Vector3 Dir = Vector3.Normalize(Stop - Start);
        float Travel = 0.0f;
        for (int Iteration = 0; Iteration < 1000; ++Iteration)
        {
            float Dist = Eval(Point);
            if (Dist <= 0.001f)
            {
                return (true, Point);
            }
            else if (Dist >= 100.0f)
            {
                break;
            }
            else
            {
                Travel += Dist;
                Point = Dir * Travel + Start;
            }
        }
        return (false, Point);
    }

    public (bool, float) TravelTrace(Vector3 Start, Vector3 Dir, float MaxTravel, float Margin)
    {
        MaxTravel += Margin;
        Vector3 Point = Start;
        Dir = Vector3.Normalize(Dir);
        float Travel = 0.0f;
        for (int Iteration = 0; Iteration < 1000 && Travel < MaxTravel; ++Iteration)
        {
            float Dist = Eval(Point) - Margin;
            if (Dist <= 0.001f)
            {
                return (true, Math.Max(Travel - Margin, 0.0f));
            }
            else
            {
                Travel += Dist;
                Point = Dir * Travel + Start;
            }
        }
        return (false, Math.Max(MaxTravel - Margin, 0.0f));
    }

    public (bool, float) TravelTrace(Vector3 Start, Vector2 FlatDir, float MaxTravel, float Margin)
    {
        Vector3 Dir;
        Dir.X = FlatDir.X;
        Dir.Y = FlatDir.Y;
        Dir.Z = 0.0f;
        return TravelTrace(Start, Dir, MaxTravel, Margin);
    }

    public float LightTrace(Vector3 Start, Vector3 Stop, float LightSize)
    {
        Vector3 Dir = Stop - Start;
        float Travel = 0.0f;
        float MaxTravel = Dir.Length();
        Dir /= MaxTravel;

        float Res = 1.0f;

        for (int Iteration = 0; Iteration < 100; ++Iteration)
        {
            float Dist = Eval(Dir * Travel + Start);
            Res = Math.Min(Res, Dist / (LightSize * Travel));
            Travel += Math.Max(Dist, 0.005f);

            if (Res < -1.0 || Travel >= MaxTravel)
            {
                break;
            }
        }

        Res = Math.Max(Res, -1.0f);
        return 0.25f * (1.0f + Res) * (1.0f + Res) * (2.0f - Res);
    }
}
