#if UNITY_IOS || UNITY_IPHONE
using Io.AppMetrica.AdRevenueAdapter.Native.iOS.Proxy;
using Io.AppMetrica.AdRevenueAdapter.Native.Utils.Serializer;

namespace Io.AppMetrica.AdRevenueAdapter.Native.iOS {
    internal class AdRevenueAdapterIos : IAdRevenueAdapterNative {
        public void ReportAdRevenue(AdRevenue adRevenue) {
            AdRevenueAdapterProxy.amauara_reportAdRevenue(adRevenue.ToJsonString());
        }
    }
}
#endif
