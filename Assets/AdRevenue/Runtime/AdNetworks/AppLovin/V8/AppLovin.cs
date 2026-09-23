#if !APPMETRICA_FEATURES_ADREVENUE_APPLOVIN_V8 && APPMETRICA_ADREVENUE_ADAPTER_APPLOVIN_V8_AUTO_ENABLED && !APPMETRICA_ADAPTER_ADREVENUE_AUTO_COLLECTION_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Io.AppMetrica.AdRevenueAdapter.AdNetworks.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Scripting;

namespace Io.AppMetrica.AdRevenueAdapter.AdNetworks.AppLovin.V8 {
    internal static class AppLovin {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SubscribeOnAdRevenueEventsSafely() {
            try {
                SubscribeOnAdRevenueEventsUnsafely();
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during subscribe on AppLovin ad revenue: " + e.Message);
            }
        }

        private static void SubscribeOnAdRevenueEventsUnsafely() {
            var adTypes = new[] { "AppOpen", "Interstitial", "Rewarded", "Banner", "MRec" };

            var appLovinAssembly = AssemblyUtils.GetAssembly(Constants.AppLovinAssemblyName);

            if (appLovinAssembly == null) {
                return;
            }

            var maxSdkCallbacksType = appLovinAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == "MaxSdkCallbacks");

            if (maxSdkCallbacksType == null) {
                return;
            }

            foreach (var adType in adTypes) {
                SubscribeOnAdRevenueWithTypeSafely(maxSdkCallbacksType.GetNestedType(adType));
            }
        }

        private static void SubscribeOnAdRevenueWithTypeSafely(Type adType) {
            try {
                SubscribeOnAdRevenueWithTypeUnsafely(adType);
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during subscribe on AppLovin ad revenue: " + e.Message);
            }
        }

        private static void SubscribeOnAdRevenueWithTypeUnsafely(Type adType) {
            var eventInfo = adType?.GetEvent("OnAdRevenuePaidEvent");
            var callbackMethod = typeof(AppLovin).GetMethod(nameof(ReportAppLovinAdRevenueSafely),
                BindingFlags.NonPublic | BindingFlags.Static);
            var handler = Delegate.CreateDelegate(eventInfo?.EventHandlerType, callbackMethod);
            eventInfo?.AddEventHandler(null, handler);
        }

        [Preserve]
        private static void ReportAppLovinAdRevenueSafely(string adUnitId, object adInfo) {
            try {
                ReportAppLovinAdRevenueUnsafely(adInfo);
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during report AppLovin ad revenue: " + e.Message);
            }
        }

        private static void ReportAppLovinAdRevenueUnsafely(object adInfo) {
            if (adInfo == null) return;

            var adInfoType = adInfo.GetType();
            var adFormat = adInfoType.GetProperty("AdFormat")?.GetValue(adInfo)?.ToString();
            var revenue = (double)(adInfoType.GetProperty("Revenue")?.GetValue(adInfo) ?? 0);
            var adUnitIdentifier = adInfoType.GetProperty("AdUnitIdentifier")?.GetValue(adInfo)?.ToString();
            var networkName = adInfoType.GetProperty("NetworkName")?.GetValue(adInfo)?.ToString();
            var placement = adInfoType.GetProperty("Placement")?.GetValue(adInfo)?.ToString();
            var networkPlacement = adInfoType.GetProperty("NetworkPlacement")?.GetValue(adInfo)?.ToString();
            var revenuePrecision = adInfoType.GetProperty("RevenuePrecision")?.GetValue(adInfo)?.ToString();

            var maxSdkType = AssemblyUtils.GetAssembly(Constants.AppLovinAssemblyName).GetTypes()
                .FirstOrDefault(t => t.Name == "MaxSdk");

            string countryCode = null;
            try {
                var sdkConfig = maxSdkType?.GetMethod("GetSdkConfiguration")?.Invoke(null, null);
                countryCode = sdkConfig?.GetType().GetProperty("CountryCode")?.GetValue(sdkConfig)?.ToString();
            }
            catch (Exception e) {
                Debug.Log("Something went wrong during AppLovin ad revenue reporting: " + e.Message);
            }

            var adType = ConvertAppLovinAdType(adFormat);
            var payload = new Dictionary<string, string> {
                { "countryCode", countryCode },
                { "original_ad_type", adFormat },
                { "original_source", "unity-ad-revenue-adapter-applovin-v8" }
            };

            AppMetricaAdRevenueAdapter.ReportAdRevenue(new AdRevenue(revenue, "USD") {
                AdUnitId = adUnitIdentifier,
                AdType = adType,
                AdNetwork = networkName,
                AdPlacementName = placement,
                AdPlacementId = networkPlacement,
                Precision = revenuePrecision,
                Payload = payload
            });
        }

        private static AdType ConvertAppLovinAdType([CanBeNull] string adFormat) {
            switch (adFormat) {
                case "NATIVE":
                    return AdType.Native;
                case "BANNER":
                    return AdType.Banner;
                case "REWARDED":
                    return AdType.Rewarded;
                case "INTER":
                    return AdType.Interstitial;
                case "MREC":
                    return AdType.Mrec;
                case "APPOPEN":
                    return AdType.AppOpen;
                default:
                    return AdType.Other;
            }
        }
    }
}
#endif
