using System.Runtime.InteropServices;

namespace LiteBrokerNet;
public class LiteBrokerNative
{
    [DllImport("liteBroker.dll")]
    public static extern IntPtr broker_version();
}

public class LiteBroker
{
    public static string GetVersion()
    {
        var ptr = LiteBrokerNative.broker_version();
        var versionString =  Marshal.PtrToStringAnsi(ptr);

        if (versionString == null)
        {
            throw new Exception("Cannot get version string from LiteBroker Native");
        }

        return versionString;
    }
}