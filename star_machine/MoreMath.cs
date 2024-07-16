
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace StarMachine;


public class MoreMath
{
    private const double RadiansPerDegree = Math.PI/180;

    public static double ToRadians(double Degrees)
    {
        return Degrees * RadiansPerDegree;
    }

    public static float ToRadians(float Degrees)
    {
        return Degrees * (float)RadiansPerDegree;
    }

    public static void InfinitePerspective(out Matrix4x4 Result, double FieldOfView, double AspectRatio, double NearPlane = 0.001)
    {
        // equiv to Math.Tan(Math.PI / 180 * FieldOfView / 2) * NearPlane
        double View = Math.Tan(Math.PI / 360 * FieldOfView) * NearPlane;
        Result = Matrix4x4.Identity;
        Result[0, 0] = (float)(NearPlane / View / AspectRatio);
        Result[1, 1] = (float)(NearPlane / View);
        Result[2, 2] = -1.0f;
        Result[3, 3] = 0.0f;
        Result[2, 3] = -1.0f;
        Result[3, 2] = (float)(-2.0f * NearPlane);
    }
}
