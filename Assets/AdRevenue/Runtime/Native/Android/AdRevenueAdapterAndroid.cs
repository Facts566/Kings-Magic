#if UNITY_ANDROID

using Io.AppMetrica.AdRevenueAdapter.Native.Android.Proxy;
using Io.AppMetrica.AdRevenueAdapter.Native.Utils.Serializer;

namespace Io.AppMetrica.AdRevenueAdapter.Native.Android {
    internal class AdRevenueAdapterAndroid : IAdRevenueAdapterNative {
        public void ReportAdRevenue(AdRevenue adRevenue) {
            AdRevenueAdapterProxy.ReportAdRevenue(adRevenue.ToJsonString());
        }
    }
}
#endif
