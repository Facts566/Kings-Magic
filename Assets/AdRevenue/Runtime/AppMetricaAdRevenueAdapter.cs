using Io.AppMetrica.AdRevenueAdapter.Native;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Scripting;

#if UNITY_ANDROID
using Io.AppMetrica.AdRevenueAdapter.Native.Android;
#elif (UNITY_IPHONE || UNITY_IOS)
using Io.AppMetrica.AdRevenueAdapter.Native.iOS;
#else
using Io.AppMetrica.AdRevenueAdapter.Native.Dummy;
#endif

[assembly: AlwaysLinkAssembly]

namespace Io.AppMetrica.AdRevenueAdapter {
    public static class AppMetricaAdRevenueAdapter {

        [NotNull]
        private static readonly IAdRevenueAdapterNative Native;

        static AppMetricaAdRevenueAdapter() {
#if UNITY_ANDROID
            Native = new AdRevenueAdapterAndroid();
#elif (UNITY_IPHONE || UNITY_IOS)
            Native = new AdRevenueAdapterIos();
#else
            Native = new AdRevenueAdapterDummy();
#endif
        }

        // Dummy method to prevent this module from shrinking
        // Must be called or referenced in project code to ensure the module is preserved
        public static void Activate() {}

        internal static void ReportAdRevenue(AdRevenue adRevenue) {
            Native.ReportAdRevenue(adRevenue);
        }
    }
}
