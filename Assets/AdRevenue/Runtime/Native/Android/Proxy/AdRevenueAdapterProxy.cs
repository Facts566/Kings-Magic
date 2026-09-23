#if UNITY_ANDROID
using UnityEngine;

namespace Io.AppMetrica.AdRevenueAdapter.Native.Android.Proxy {
    internal static class AdRevenueAdapterProxy {
        private static readonly AndroidJavaClass NativeClass = new AndroidJavaClass("io.appmetrica.adrevenueadapter.plugin.unity.AdRevenueAdapterProxy");
        
        public static void ReportAdRevenue(string adRevenue) {
            NativeClass.CallStatic("reportAdRevenue", adRevenue);
        }
    }
}
#endif
