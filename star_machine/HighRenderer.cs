
using Vector3 = System.Numerics.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;
using static StarMachine.MoreMath;

namespace StarMachine;


class HighLevelRenderer
{
    public float AspectRatio = 1.0f;

    public HighLevelRenderer()
    {
        // TODO wire in the ring buffer arrays from the low renderer
    }

    public void Boot()
    {
        // stuff for setting up threadpools
    }

    public void Teardown()
    {
         // stuff for tearing down threadpools
    }

    public void Advance(FrameInfo Frame)
    {
        // stuff for copying data from threadpools and wiring it over to the low renderer
    }
}
