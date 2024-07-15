
using System.Diagnostics;
using Vector3 = System.Numerics.Vector3;
using static StarMachine.MoreMath;

namespace StarMachine;


public class SplatGenerator
{
    public Vector3[] Vertices;
    public UInt16[] Indices;
    public uint TriangleCount;

    public SplatGenerator(bool Paraboloid, int[] Rings)
    {
        var Offsets = new int[Rings.Length];
        var SplatVertexCount = Rings.Sum();

        var DiscPoint = (float Radius, float Degrees) =>
        {
            float Angle = ToRadians(Degrees);
            float S = (float)System.Math.Sin(Angle);
            float C = (float)System.Math.Cos(Angle);
            var Vertex = new Vector3(S * Radius, C * Radius, 0.0f);
            if (Paraboloid)
            {
                Vertex.Z = -Vector3.Dot(Vertex, Vertex);
            }
            return Vertex;
        };

        {
            Vertices = new Vector3[SplatVertexCount];
            int Offset = 0;
            int Ring = 0;

            foreach (int RingVertexCount in Rings)
            {
                if (RingVertexCount == 1)
                {
                    Vertices[Offset] = new Vector3(0.0f, 0.0f, 0.0f);
                }
                else
                {
                    Debug.Assert(RingVertexCount >= 3);
                    int RingNudge = Rings[0] == 1 ? 0 : 1;
                    float Radius = (float)(Ring + RingNudge) / (float)(Rings.Length - 1  + RingNudge);
                    for (int Index = 0; Index < RingVertexCount; ++Index)
                    {
                        float Angle = (float)(Index) / (float)(RingVertexCount) * 360.0f;
                        Vertices[Offset + Index] = DiscPoint(Radius, Angle);
                    }
                }
                Offsets[Ring] = Offset;
                ++Ring;
                Offset += RingVertexCount;
            }
        }

        int SplatIndexCount = 0;
        {
            var DecodeRingIndex = (int Index, int Ring) =>
            {
                int Offset = Offsets[Ring];
                int Range = Rings[Ring];
                return (((Index - Offset) % Range) + Offset);
            };

            var Loops = new List<List<UInt16>>();
            int RingA;
            for (RingA = 0; RingA < Rings.Length - 1; ++RingA)
            {
                var Strip = new List<UInt16>();
                int RingB = RingA + 1;
                int Dialation = Rings[RingA] * Rings[RingB];
                int StrideA = Dialation / Rings[RingA];
                int StrideB = Dialation / Rings[RingB];
                int OffsetA = Offsets[RingA];
                int OffsetB = Offsets[RingB];
                (int A, int B) Last = (OffsetA, OffsetB);

                var RecordTriangle = ((int A, int B) LHS, (int A, int B) RHS) =>
                {
                    int Wrote = 2;
                    Strip.Add((UInt16)LHS.B);
                    if (LHS.B != RHS.B)
                    {
                        ++Wrote;
                        Strip.Add((UInt16)RHS.B);
                    }

                    Strip.Add((UInt16)RHS.A);
                    if (LHS.A != RHS.A)
                    {
                        ++Wrote;
                        Strip.Add((UInt16)LHS.A);
                    }
                    Debug.Assert(Wrote == 3);
                };

                var RecordLine = ((int A, int B) Next) =>
                {
                    int A1 = DecodeRingIndex(Last.A, RingA);
                    int A2 = DecodeRingIndex(Next.A, RingA);
                    int B1 = DecodeRingIndex(Last.B, RingB);
                    int B2 = DecodeRingIndex(Next.B, RingB);

                    bool MatchA = (A1 == A2);
                    bool MatchB = (B1 == B2);
                    if (MatchA && MatchB)
                    {
                        // Matched a repeat.
                        return;
                    }
                    else if (MatchA == MatchB)
                    {
                        // Matched a quad.
                        RecordTriangle((A1, B1), (A2, B1));
                        RecordTriangle((A2, B1), (A2, B2));
                        Last = Next;
                    }
                    else
                    {
                        // Matched a triangle.
                        RecordTriangle((A1, B1), (A2, B2));
                        Last = Next;
                    }
                };

                for (int Cursor = 1; Cursor < Dialation; ++Cursor)
                {
                    (int A, int B) Next = (Cursor / StrideA + OffsetA, Cursor / StrideB + OffsetB);
                    RecordLine(Next);
                }
                {
                    (int A, int B) Next = (OffsetA + Rings[RingA], OffsetB + Rings[RingB]);
                    RecordLine(Next);
                }

                SplatIndexCount += Strip.Count;
                Loops.Add(Strip);
            }

            Indices = new UInt16[SplatIndexCount];
            {
                int Cursor = 0;
                foreach (var Strip in Loops)
                {
                    foreach (UInt16 Index in Strip)
                    {
                        Indices[Cursor++] = Index;
                    }
                }
            }
        }

        TriangleCount = (uint)SplatIndexCount / 3;
    }
}
