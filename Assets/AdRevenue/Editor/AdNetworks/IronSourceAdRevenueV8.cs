using Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks.Utils;

namespace Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks {
    internal class IronSourceAdRevenueV8 : AdNetwork {
        protected override string DefineName => "APPMETRICA_ADREVENUE_ADAPTER_IRONSOURCE_V8";
        
        internal override bool IsAvailable => AssetsUtils.IsAssetInProject("IronSourceAdInfo");
    }
}
