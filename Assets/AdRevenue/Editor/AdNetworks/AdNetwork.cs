namespace Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks {
    internal abstract class AdNetwork {
        protected abstract string DefineName { get; }

        internal virtual void OnChangedAction() {}

        internal string AutoEnabledDefineName => $"{DefineName}_AUTO_ENABLED";

        internal abstract bool IsAvailable { get; }
    }
}
