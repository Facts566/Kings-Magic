using UnityEngine;

public class AdsTimer : MonoBehaviour
{
    [Header("Настройки таймера")]
    [Tooltip("Интервал в секундах. 300 = 5 минут")]
    public float interval = 300f;

    [Tooltip("Запускать таймер автоматически при старте сцены")]
    public bool autoStart = true;

    [Header("Реклама")]
    [Tooltip("Перетащи сюда объект с компонентом FullscreenAd")]
    public FullscreenAd fullscreenAd;

    private float timeLeft;
    private bool isRunning = false;

    void Start()
    {
        if (autoStart)
        {
            StartTimer();
        }
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = interval; // сброс на следующий цикл
            ShowAd(); // вызываем рекламу
            Debug.Log($"Таймер сработал! Прошло {interval} секунд.");
        }
    }

    /// <summary>
    /// Запустить таймер.
    /// </summary>
    public void StartTimer()
    {
        CancelPendingAd();
        timeLeft = interval;
        isRunning = true;
        Debug.Log($"Таймер запущен на {interval} секунд.");
    }

    /// <summary>
    /// Остановить таймер.
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        CancelPendingAd();
        Debug.Log("Таймер остановлен.");
    }

    /// <summary>
    /// Сбросить таймер и запустить заново.
    /// </summary>
    public void ResetTimer()
    {
        CancelPendingAd();
        timeLeft = interval;
        isRunning = true;
        Debug.Log("Таймер сброшен и запущен заново.");
    }

    /// <summary>
    /// Оставшееся время в секундах.
    /// </summary>
    public float GetTimeLeft()
    {
        return timeLeft;
    }

    // ================= РЕКЛАМА =================

    /// <summary>
    /// Показывает рекламу, когда SDK подтвердит завершение загрузки.
    /// </summary>
    private void ShowAd()
    {
        if (fullscreenAd == null)
        {
            Debug.LogWarning("FullscreenAd не назначен в инспекторе!");
            return;
        }

        fullscreenAd.ShowInterstitialWhenReady();
        Debug.Log("Запрос на показ рекламы отправлен.");
    }

    private void OnDisable()
    {
        CancelPendingAd();
    }

    private void CancelPendingAd()
    {
        if (fullscreenAd != null)
        {
            fullscreenAd.CancelPendingShow();
        }
    }
}