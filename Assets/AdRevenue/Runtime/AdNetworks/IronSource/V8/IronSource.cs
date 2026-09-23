#if !APPMETRICA_FEATURES_ADREVENUE_IRONSOURCE_V8 && APPMETRICA_ADREVENUE_ADAPTER_IRONSOURCE_V8_AUTO_ENABLED && !APPMETRICA_ADAPTER_ADREVENUE_AUTO_COLLECTION_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Io.AppMetrica.AdRevenueAdapter.AdNetworks.Utils;
using UnityEngine;
using UnityEngine.Scripting;

namespace Io.AppMetrica.AdRevenueAdapter.AdNetworks.IronSource.V8 {
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
            var ironSourceAssembly = AssemblyUtils.GetAssembly(Constants.IronSourceV8AssemblyName);

            if (ironSourceAssembly == null) {
                return;
            }

            var eventMappings = new Dictionary<string, (string eventName, string methodName)> {
                { "IronSourceRewardedVideoEvents", ("onAdRewardedEvent", "OnIronSourceRewarded") },
                { "IronSourceInterstitialEvents", ("onAdShowSucceededEvent", "OnIronSourceInterstitial") },
                { "IronSourceBannerEvents", ("onAdScreenPresentedEvent", "OnIronSourceBanner") }
            };

            foreach (var mapping in eventMappings) {
                SubscribeOnAdRevenueWithTypeSafely(
                    ironSourceAssembly.GetTypes().FirstOrDefault(t => t.Name == mapping.Key),
                    mapping.Value.eventName,
                    mapping.Value.methodName
                );
            }
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
        private static void OnIronSourceRewarded(object placement, object adInfo) {
            ReportIronSourceAdRevenueSafely(AdType.Rewarded, adInfo, placement);
        }

        [Preserve]
        private static void OnIronSourceInterstitial(object adInfo) {
            ReportIronSourceAdRevenueSafely(AdType.Interstitial, adInfo);
        }

        [Preserve]
        private static void OnIronSourceBanner(object adInfo) {
            ReportIronSourceAdRevenueSafely(AdType.Banner, adInfo);
        }

        private static void ReportIronSourceAdRevenueSafely(AdType adType, object adInfo, object placement = null) {
            try {
                ReportIronSourceAdRevenueUnsafely(adType, adInfo, placement);
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during report IronSource ad revenue: " + e.Message);
            }
        }

        private static void ReportIronSourceAdRevenueUnsafely(AdType adType, object adInfo, object placement = null) {
            if (adInfo == null) return;

            var adInfoType = adInfo.GetType();
            var country = adInfoType.GetProperty("country")?.GetValue(adInfo)?.ToString();
            var revenue = (double?)adInfoType.GetProperty("revenue")?.GetValue(adInfo) ?? 0d;
            var adUnit = adInfoType.GetProperty("adUnit")?.GetValue(adInfo)?.ToString();
            var adNetwork = adInfoType.GetProperty("adNetwork")?.GetValue(adInfo)?.ToString();
            var precision = adInfoType.GetProperty("precision")?.GetValue(adInfo)?.ToString();

            var placementName = "";
            try {
                placementName = placement?.GetType().GetMethod("getPlacementName")?.Invoke(placement, null).ToString();
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during report IronSource ad revenue: " + e.Message);
            }

            var payload = new Dictionary<string, string> {
                { "countryCode", country },
                { "original_ad_type", adType.ToString() },
                { "original_source", "unity-ad-revenue-adapter-ironsource-v8" }
            };

            AppMetricaAdRevenueAdapter.ReportAdRevenue(new AdRevenue(revenue, "USD") {
                AdUnitId = adUnit,
                AdType = adType,
                AdNetwork = adNetwork,
                AdPlacementName = placementName,
                Precision = precision,
                Payload = payload
            });
        }
    }
}
#endif
