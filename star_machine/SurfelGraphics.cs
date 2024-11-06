
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Vector3 = System.Numerics.Vector3;


namespace StarMachine;


public class SurfelResource
{
    public string Name = "";
    public Vector3[] Position;
    public Vector3[] Normal;
    public float[] Radius;
    public Int32[] Parent;
    public byte[] Generation;

    private SurfelResource(
        string InName,
        Vector3[] InPosition,
        Vector3[] InNormal,
        float[] InRadius,
        Int32[] InParent,
        byte[] InGeneration)
    {
        Name = InName;
        Position = InPosition;
        Normal = InNormal;
        Radius = InRadius;
        Parent = InParent;
        Generation = InGeneration;

        int SurfelCount = Position.Length;

        Trace.Assert(SurfelCount > 0);
        Trace.Assert(Normal.Length == SurfelCount);
        Trace.Assert(Radius.Length == SurfelCount);
        Trace.Assert(Parent.Length == SurfelCount);
        Trace.Assert(Generation.Length == SurfelCount);

#if False
        for (int i = 0; i < 10; ++i)
        {
            Console.WriteLine($"###({i})----------------");
            Console.WriteLine($"{Position[i]}");
            Console.WriteLine($"{Normal[i]}");
            Console.WriteLine($"{Radius[i]}");
            Console.WriteLine($"{Parent[i]}");
            Console.WriteLine($"{Generation[i]}");
        }
#endif
    }

    private static Vector3[] ReadVectorArray(byte[] ResourceData, int Cursor, int SizeBytes)
    {
        int Count = SizeBytes / 12;
        Vector3[] Data = new Vector3[Count];
        for (int Index = 0; Index < Count; ++Index)
        {
            Data[Index].X = BitConverter.ToSingle(ResourceData, Cursor + Index * 12);
            Data[Index].Y = BitConverter.ToSingle(ResourceData, Cursor + Index * 12 + 4);
            Data[Index].Z = BitConverter.ToSingle(ResourceData, Cursor + Index * 12 + 8);
        }
        return Data;
    }

    public static SurfelResource LoadSMBH(string Name)
    {
        byte[] ResourceData = Resources.Read(Name);
        unsafe
        {
            string Header = System.Text.Encoding.UTF8.GetString(ResourceData, 0, 8);
            Trace.Assert(Header == "starmach");
            Int64 Reserved = BitConverter.ToInt64(ResourceData, 8);
            Int64 SurfelCount = BitConverter.ToInt64(ResourceData, 16);
            int Cursor = 24;

            int PositionBytes = (int)BitConverter.ToInt64(ResourceData, Cursor);
            Cursor += 8;
            Vector3[] Position = ReadVectorArray(ResourceData, Cursor, PositionBytes);
            Cursor += PositionBytes;

            int NormalBytes = (int)BitConverter.ToInt64(ResourceData, Cursor);
            Cursor += 8;
            Vector3[] Normal = ReadVectorArray(ResourceData, Cursor, NormalBytes);
            Cursor += NormalBytes;

            int RadiusBytes = (int)BitConverter.ToInt64(ResourceData, Cursor);
            Cursor += 8;
            float[] Radius = new float[SurfelCount];
            Buffer.BlockCopy(ResourceData, Cursor, Radius, 0, RadiusBytes);
            Cursor += RadiusBytes;

            int ParentBytes = (int)BitConverter.ToInt64(ResourceData, Cursor);
            Cursor += 8;
            Int32[] Parent = new Int32[SurfelCount];
            Buffer.BlockCopy(ResourceData, Cursor, Parent, 0, ParentBytes);
            Cursor += ParentBytes;

            int GenerationBytes = (int)BitConverter.ToInt64(ResourceData, Cursor);
            Cursor += 8;
            byte[] Generation = new byte[SurfelCount];
            Buffer.BlockCopy(ResourceData, Cursor, Generation, 0, GenerationBytes);
            Cursor += GenerationBytes;

            return new SurfelResource(Name, Position, Normal, Radius, Parent, Generation);
        }
    }
}
