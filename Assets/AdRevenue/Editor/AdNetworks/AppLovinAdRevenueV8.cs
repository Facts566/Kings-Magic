using Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks.Utils;

namespace Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks {
    internal class AppLovinAdRevenueV8 : AdNetwork {
        protected override string DefineName => "APPMETRICA_ADREVENUE_ADAPTER_APPLOVIN_V8";
        
        internal override bool IsAvailable => AssetsUtils.IsAssetInProject("MaxSdk");
    }
}
