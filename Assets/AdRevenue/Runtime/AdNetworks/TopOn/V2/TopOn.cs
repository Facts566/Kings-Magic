#if !APPMETRICA_FEATURES_ADREVENUE_TOPON_V2 && APPMETRICA_ADREVENUE_ADAPTER_TOPON_V2_AUTO_ENABLED && !APPMETRICA_ADAPTER_ADREVENUE_AUTO_COLLECTION_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Io.AppMetrica.AdRevenueAdapter.AdNetworks.Utils;
using UnityEngine;
using UnityEngine.Scripting;

namespace Io.AppMetrica.AdRevenueAdapter.AdNetworks.TopOn.V2 {
    internal static class TopOn {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SubscribeOnAdRevenueEventsSafely() {
            try {
                SubscribeOnAdRevenueEventsUnsafely();
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during subscribe on TopOn ad revenue: " + e.Message);
            }
        }

        private static void SubscribeOnAdRevenueEventsUnsafely() {
            var topOnAssembly = AssemblyUtils.GetAssembly(Constants.TopOnAssemblyName);

            if (topOnAssembly == null) {
                return;
            }

            var eventMappings = new Dictionary<string, string> {
                { "ATRewardedAutoVideo", "onRewardEvent" },
                { "ATInterstitialAutoAd", "onAdVideoEndEvent" },
                { "ATBannerAd", "onAdImpressEvent" },
                { "ATNativeAd", "onAdImpressEvent" },
                { "ATSplashAd", "onAdCloseEvent" }
            };

            foreach (var mapping in eventMappings) {
                SubscribeOnAdRevenueWithTypeSafely(
                    topOnAssembly.GetTypes().FirstOrDefault(t => t.Name == mapping.Key),
                    mapping.Value
                );
            }
        }

        private static void SubscribeOnAdRevenueWithTypeSafely(Type type, string eventName) {
            try {
                SubscribeOnAdRevenueWithTypeUnsafely(type, eventName);
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during subscribe on TopOn ad revenue: " + e.Message);
            }
        }

        private static void SubscribeOnAdRevenueWithTypeUnsafely(Type type, string eventName) {
            if (type == null) return;

            var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var instance = instanceProperty?.GetValue(null);
            var clientField = instance?.GetType().GetField("client", BindingFlags.Public | BindingFlags.Instance);
            var client = clientField?.GetValue(instance);
            var eventInfo = client?.GetType().GetEvent(eventName);
            var method = typeof(TopOn).GetMethod(nameof(ReportTopOnAdRevenueSafely),
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null) {
                var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, null, method);
                eventInfo?.AddEventHandler(client, handler);
            }
        }

        [Preserve]
        private static void ReportTopOnAdRevenueSafely(object sender, object adEvent) {
            try {
                ReportTopOnAdRevenueUnsafely(adEvent);
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during report TopOn ad revenue: " + e.Message);
            }
        }

        private static void ReportTopOnAdRevenueUnsafely(object adEvent) {
            var adEventType = adEvent.GetType();
            var callbackInfo = adEventType.GetField("callbackInfo").GetValue(adEvent);

            var callbackInfoType = callbackInfo.GetType();
            var adUnitFormat = callbackInfoType.GetProperty("adunit_format")?.GetValue(callbackInfo)?.ToString();
            var country = callbackInfoType.GetProperty("country")?.GetValue(callbackInfo)?.ToString();
            var currency = callbackInfoType.GetProperty("currency")?.GetValue(callbackInfo)?.ToString();
            var publisherRevenue =
                (double)(callbackInfoType.GetProperty("publisher_revenue")?.GetValue(callbackInfo) ?? 0d);
            var adUnitId = callbackInfoType.GetProperty("adunit_id")?.GetValue(callbackInfo)?.ToString();
            var networkName = callbackInfoType.GetProperty("network_name")?.GetValue(callbackInfo)?.ToString();
            var networkPlacementId =
                callbackInfoType.GetProperty("network_placement_id")?.GetValue(callbackInfo)?.ToString();
            var precision = callbackInfoType.GetProperty("precision")?.GetValue(callbackInfo)?.ToString();

            var adType = ConvertTopOnAdType(adUnitFormat);
            var payload = new Dictionary<string, string> {
                { "countryCode", country },
                { "original_ad_type", adUnitFormat },
                { "original_source", "unity-ad-revenue-adapter-topon-v2" }
            };

            AppMetricaAdRevenueAdapter.ReportAdRevenue(new AdRevenue(
                publisherRevenue,
                currency ?? "USD"
            ) {
                AdUnitId = adUnitId,
                AdType = adType,
                AdNetwork = networkName,
                AdPlacementName = networkPlacementId,
                Precision = precision,
                Payload = payload
            });
        }

        private static AdType ConvertTopOnAdType(string adUnitFormat) {
            switch (adUnitFormat) {
                case "Native":
                    return AdType.Native;
                case "Banner":
                    return AdType.Banner;
                case "RewardedVideo":
                    return AdType.Rewarded;
                case "Interstitial":
                    return AdType.Interstitial;
                default:
                    return AdType.Other;
            }
        }
    }
}
#endif
