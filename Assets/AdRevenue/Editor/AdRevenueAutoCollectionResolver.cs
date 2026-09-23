using System.Collections.Generic;
using System.Linq;
using Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Io.AppMetrica.AdRevenueAdapter.Editor {
    
    internal static class AdRevenueAutoCollectionResolver {
        
#if UNITY_2021_3_OR_NEWER
        private static readonly NamedBuildTarget[] SupportedBuildTargets = {
            NamedBuildTarget.iOS,
            NamedBuildTarget.Android,
        };
#else
        private static readonly BuildTargetGroup[] SupportedBuildTargets = {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS
        };
        private static readonly char[] DefineSplits = { ';', ',', ' ' };
#endif

        private static readonly Dictionary<string, AdNetwork> SupportedAdNetworks = new Dictionary<string, AdNetwork>
        {
            [SupportedAdNetworksNames.AppLovinAdRevenueV8] = new AppLovinAdRevenueV8(),
            [SupportedAdNetworksNames.IronSourceAdRevenueV8] = new IronSourceAdRevenueV8(),
            [SupportedAdNetworksNames.IronSourceAdRevenueV9] = new IronSourceAdRevenueV9(),
            [SupportedAdNetworksNames.TopOnAdRevenueV2] = new TopOnAdRevenueV2(),
        };

        [InitializeOnLoadMethod]
        internal static void Init() {
#if !APPMETRICA_ADAPTER_ADREVENUE_AUTOCOLLECTION_DISABLED
            foreach (var adNetwork in SupportedAdNetworks.Values) {
                adNetwork.OnChangedAction();
            }
            
            ApplyDefines();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        private static void ApplyDefines() {
            var autoEnabledDefines = SupportedAdNetworks.Values
                .Where(adNetwork => adNetwork.IsAvailable)
                .Select(adNetwork => adNetwork.AutoEnabledDefineName)
                .ToArray();

            var autoDisabledDefines = SupportedAdNetworks.Values
                .Where(adNetwork => !adNetwork.IsAvailable)
                .Select(adNetwork => adNetwork.AutoEnabledDefineName)
                .ToArray();

            foreach (var supportedTarget in SupportedBuildTargets) {
#if UNITY_2021_3_OR_NEWER
                PlayerSettings.GetScriptingDefineSymbols(supportedTarget, out var currentDefines);
#else
                var currentDefines = PlayerSettings
                    .GetScriptingDefineSymbolsForGroup(supportedTarget)
                    .Split(DefineSplits, System.StringSplitOptions.RemoveEmptyEntries);
#endif
                var newDefines = currentDefines
                    .Union(autoEnabledDefines)
                    .Except(autoDisabledDefines)
                    .ToArray();
#if UNITY_2021_3_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(supportedTarget, newDefines);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(supportedTarget, string.Join(";", newDefines));
#endif
            }
        }
    }

    internal static class SupportedAdNetworksNames {
        internal const string AppLovinAdRevenueV8 = nameof(AppLovinAdRevenueV8);
        internal const string IronSourceAdRevenueV8 = nameof(IronSourceAdRevenueV8);
        internal const string IronSourceAdRevenueV9 = nameof(IronSourceAdRevenueV9);
        internal const string TopOnAdRevenueV2 = nameof(TopOnAdRevenueV2);
    }
}
