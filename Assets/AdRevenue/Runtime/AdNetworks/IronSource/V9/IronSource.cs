#if !APPMETRICA_FEATURES_ADREVENUE_IRONSOURCE_V9 && APPMETRICA_ADREVENUE_ADAPTER_IRONSOURCE_V9_AUTO_ENABLED && !APPMETRICA_ADAPTER_ADREVENUE_AUTO_COLLECTION_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Io.AppMetrica.AdRevenueAdapter.AdNetworks.Utils;
using UnityEngine;
using UnityEngine.Scripting;

namespace Io.AppMetrica.AdRevenueAdapter.AdNetworks.IronSource.V9 {
    internal static class IronSource {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SubscribeOnAdRevenueEventsSafely() {
            try {
                SubscribeOnAdRevenueEventsUnsafely();
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during subscribe on IronSource ad revenue: " + e.Message);
            }
        }

        private static void SubscribeOnAdRevenueEventsUnsafely() {
            var ironSourceAssembly = AssemblyUtils.GetAssembly(Constants.IronSourceV9AssemblyName);

            if (ironSourceAssembly == null) {
                return;
            }

            var levelPlayType = ironSourceAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == "LevelPlay");

            if (levelPlayType == null) {
                return;
            }

            SubscribeOnAdRevenueWithTypeSafely(levelPlayType, "OnImpressionDataReady", "OnImpressionDataReady");
        }

        private static void SubscribeOnAdRevenueWithTypeSafely(Type type, string eventName, string methodName) {
            try {
                SubscribeOnAdRevenueWithTypeUnsafely(type, eventName, methodName);
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during subscribe on IronSource ad revenue: " + e.Message);
            }
        }

        private static void SubscribeOnAdRevenueWithTypeUnsafely(Type type, string eventName, string methodName) {
            var eventInfo = type?.GetEvent(eventName);
            var method = typeof(IronSource).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, method);
            eventInfo.AddEventHandler(null, handler);
        }

        [Preserve]
        private static void OnImpressionDataReady(object impressionData) {
            ReportIronSourceAdRevenueSafely(impressionData);
        }

        private static void ReportIronSourceAdRevenueSafely(object impressionData) {
            try {
                ReportIronSourceAdRevenueUnsafely(impressionData);
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during report IronSource ad revenue: " + e.Message);
            }
        }

        private static void ReportIronSourceAdRevenueUnsafely(object impressionData) {
            if (impressionData == null) return;

            var impressionDataType = impressionData.GetType();
            var adFormat = impressionDataType.GetProperty("AdFormat")?.GetValue(impressionData)?.ToString();
            var country = impressionDataType.GetProperty("Country")?.GetValue(impressionData)?.ToString();
            var revenue = (double?)impressionDataType.GetProperty("Revenue")?.GetValue(impressionData) ?? 0d;
            var adUnitId = impressionDataType.GetProperty("MediationAdUnitId")?.GetValue(impressionData)?.ToString();
            var adUnitName = impressionDataType.GetProperty("MediationAdUnitName")?.GetValue(impressionData)?.ToString();
            var adNetwork = impressionDataType.GetProperty("AdNetwork")?.GetValue(impressionData)?.ToString();
            var placement = impressionDataType.GetProperty("Placement")?.GetValue(impressionData)?.ToString();
            var precision = impressionDataType.GetProperty("Precision")?.GetValue(impressionData)?.ToString();

            var adType = ParseAdType(adFormat);

            var payload = new Dictionary<string, string> {
                { "countryCode", country },
                { "original_ad_type", adType.ToString() },
                { "original_source", "unity-ad-revenue-adapter-ironsource-v9" }
            };

            AppMetricaAdRevenueAdapter.ReportAdRevenue(new AdRevenue(revenue, "USD") {
                AdUnitId = adUnitId,
                AdUnitName = adUnitName,
                AdType = adType,
                AdNetwork = adNetwork,
                AdPlacementName = placement,
                Precision = precision,
                Payload = payload
            });
        }

        private static AdType ParseAdType(string adFormat) {
            if (string.IsNullOrEmpty(adFormat)) {
                return AdType.Other;
            }

            switch (adFormat.ToLower()) {
                case "rewarded_video":
                    return AdType.Rewarded;
                case "interstitial":
                    return AdType.Interstitial;
                case "banner":
                    return AdType.Banner;
                default:
                    return AdType.Other;
            }
        }
    }
}
#endif
