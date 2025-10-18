using Warudo.Core;

namespace Warudo.Plugins.Scene.Assets;

public static class ML_DebugUtil
{
    private static bool EnableToast = true;

    public static void ToastDebug(string msg)
    {
        if (EnableToast)
        {
            Context.Service.Toast(Warudo.Core.Server.ToastSeverity.Info, "Debug", msg);
        }
    }
}
