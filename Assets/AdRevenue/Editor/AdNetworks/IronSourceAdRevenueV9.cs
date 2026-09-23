using Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks.Utils;

namespace Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks {
    internal class IronSourceAdRevenueV9 : AdNetwork {
        protected override string DefineName => "APPMETRICA_ADREVENUE_ADAPTER_IRONSOURCE_V9";

        internal override bool IsAvailable => AssetsUtils.IsAssetInProject("LevelPlayImpressionData");
    }
}
