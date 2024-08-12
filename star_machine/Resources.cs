
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Buffer;

namespace StarMachine;


public class Resources
{
    public static string Find(string Partial)
    {
        Regex Pattern = new($"^(.+{Partial})$");

        int MatchCount = 0;
        string MatchedName = "";
        string[] ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        foreach (string ResourceName in ResourceNames)
        {
            MatchCollection Match = Pattern.Matches(ResourceName);
            if (Match.Count == 1)
            {
                Trace.Assert(MatchCount == 0, $"\"{Partial}\" matches multiple embedded resources!");
                ++MatchCount;
                MatchedName = ResourceName;
            }
        }
        Trace.Assert(MatchCount == 1, $"\"{Partial}\" matches no embedded resources!");
        return MatchedName;
    }

    public static byte[] Read(string ResourceName)
    {
        using (Stream? ResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
        {
            Trace.Assert(ResourceStream != null, $"\"{ResourceName}\" matches no embedded resources!");
            if (ResourceStream != null)
            {
                var ResourceBytes = new byte[ResourceStream.Length];
                ResourceStream.Read(ResourceBytes, 0, (int)ResourceStream.Length);
                return ResourceBytes;
            }
            else
            {
                throw new UnreachableException();
            }
        }
    }

    public static byte[] FindAndRead(string Partial)
    {
        return Read(Find(Partial));
    }
}
