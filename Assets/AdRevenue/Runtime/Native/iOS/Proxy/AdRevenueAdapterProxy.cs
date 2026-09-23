#if UNITY_IPHONE || UNITY_IOS
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Io.AppMetrica.AdRevenueAdapter.Native.iOS.Proxy {
    internal static class AdRevenueAdapterProxy {

        [DllImport("__Internal")]
        public static extern void amauara_reportAdRevenue([NotNull] string adRevenue);
    }
}
#endif
