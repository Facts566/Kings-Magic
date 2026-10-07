using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;
    public Text percentText;
    public AudioSource backgroundMusic;

    private const string VolumeKey = "musicVolume";

    void Start()
    {
        if (backgroundMusic == null)
        {
            GameObject bg = GameObject.Find("BackgroundMusic");
            if (bg != null)
                backgroundMusic = bg.GetComponent<AudioSource>();
        }

        float saved = PlayerPrefs.GetFloat(VolumeKey, 0.5f);
        saved = Mathf.Clamp01(saved);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        ApplyVolume(saved);
    }

    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
    }

    private void ApplyVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (backgroundMusic != null)
            backgroundMusic.volume = value;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }
}
