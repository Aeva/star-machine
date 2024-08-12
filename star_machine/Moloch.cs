
using System.Runtime.InteropServices;
using StarMachine;

namespace Moloch;


public class BlobHelper
{
    protected IntPtr Ptr = IntPtr.Zero;
    protected int Size = 0;

    protected void Malloc(byte[] Data)
    {
        Size = Data.Length;
        Ptr = Marshal.AllocHGlobal(Size);
        Marshal.Copy(Data, 0, Ptr, Size);
    }

    protected virtual void Free()
    {
    }

    protected static void FreeCallback(IntPtr Ptr)
    {
        Marshal.FreeHGlobal(Ptr);
    }

    ~BlobHelper()
    {
        Free();
    }
}


public class ResourceBlob : BlobHelper
{
    public string ResourceName;

    protected void MallocResource(string ResourceHint)
    {
        ResourceName = Resources.Find(ResourceHint);
        Malloc(Resources.Read(ResourceName));
    }
}
