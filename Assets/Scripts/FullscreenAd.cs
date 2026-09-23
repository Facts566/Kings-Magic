/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Android (C) 2023 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class FullscreenAd : MonoBehaviour
{
    private String message = "";

    private InterstitialAdLoader interstitialAdLoader;
    private Interstitial interstitial;
    private bool isLoading;
    private bool isShowing;
    private bool showWhenLoaded;
    private bool isDestroyed;

    public void Awake()
    {
        this.interstitialAdLoader = new InterstitialAdLoader();
    }

    /// <summary>
    /// Запросить загрузку межстраничной рекламы.
    /// </summary>
    public void RequestInterstitial()
    {
        if (isDestroyed || isLoading || isShowing || this.interstitial != null)
        {
            return;
        }

        //Sets COPPA restriction for user age under 13
        YandexAds.SetAgeRestricted(true);

        // Replace demo Unit ID 'demo-interstitial-yandex' with actual Ad Unit ID
        string adUnitId = "R-M-20035965-1";

        isLoading = true;
        this.interstitialAdLoader.LoadAd(
            this.CreateAdRequest(adUnitId),
            onLoaded: this.HandleAdLoaded,
            onFailed: this.HandleAdFailedToLoad);
        this.DisplayMessage("Interstitial is requested");
    }

    /// <summary>
    /// Загрузить рекламу при необходимости и показать после загрузки.
    /// </summary>
    public void ShowInterstitialWhenReady()
    {
        if (isDestroyed || !isActiveAndEnabled || isShowing) return;

        showWhenLoaded = true;
        if (this.interstitial != null)
        {
            ShowInterstitial();
        }
        else
        {
            RequestInterstitial();
        }
    }

    public void CancelPendingShow()
    {
        showWhenLoaded = false;
    }

    /// <summary>
    /// Показать загруженную рекламу.
    /// </summary>
    public void ShowInterstitial()
    {
        if (isDestroyed || !isActiveAndEnabled || isShowing) return;

        if (this.interstitial == null)
        {
            this.DisplayMessage("Interstitial is not ready yet");
            return;
        }

        this.interstitial.OnAdClicked += this.HandleAdClicked;
        this.interstitial.OnAdShown += this.HandleAdShown;
        this.interstitial.OnAdFailedToShow += this.HandleAdFailedToShow;
        this.interstitial.OnAdImpression += this.HandleImpression;
        this.interstitial.OnAdDismissed += this.HandleAdDismissed;

        showWhenLoaded = false;
        isShowing = true;
        this.interstitial.Show();
    }

    private AdRequest CreateAdRequest(string adUnitId)
    {
        return new AdRequest(adUnitId);
    }

    private void DisplayMessage(String message)
    {
        this.message = message + (this.message.Length == 0 ? "" : "\n--------\n" + this.message);
        MonoBehaviour.print(message);
    }

    #region Interstitial callback handlers

    public void HandleAdLoaded(Interstitial interstitial)
    {
        isLoading = false;
        if (isDestroyed)
        {
            interstitial.Destroy();
            return;
        }

        this.DisplayMessage("HandleAdLoaded event received");
        this.interstitial = interstitial;
        if (showWhenLoaded)
        {
            ShowInterstitial();
        }
    }

    public void HandleAdFailedToLoad(AdFailedToLoadEventArgs args)
    {
        isLoading = false;
        showWhenLoaded = false;
        if (isDestroyed) return;

        this.DisplayMessage($"HandleAdFailedToLoad event received with message: {args.Message}");
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdClicked event received");
    }

    public void HandleAdShown(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdShown event received");
    }

    public void HandleAdDismissed(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdDismissed event received");

        DestroyInterstitial();
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        var data = impressionData == null ? "null" : impressionData.rawData;
        this.DisplayMessage($"HandleImpression event received with data: {data}");
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        this.DisplayMessage($"HandleAdFailedToShow event received with message: {args.Message}");
        DestroyInterstitial();
    }

    #endregion

    private void DestroyInterstitial()
    {
        isShowing = false;
        if (this.interstitial != null)
        {
            this.interstitial.Destroy();
            this.interstitial = null;
        }
    }

    private void OnDisable()
    {
        CancelPendingShow();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        CancelPendingShow();
        if (isLoading && this.interstitialAdLoader != null)
        {
            this.interstitialAdLoader.CancelLoading();
        }
        DestroyInterstitial();
    }
}