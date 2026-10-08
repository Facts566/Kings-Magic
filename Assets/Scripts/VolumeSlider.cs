using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{

    public Slider volumeSlider;
    public AudioSource audio;
    public Text percentText;

    void Update()
    {
        volumeSlider.value = audio.volume;
        percentText.text = Mathf.RoundToInt(audio.volume * 100) + "%";
    }

    public void ChangeVolume()
    {
        audio.volume = volumeSlider.value;
        percentText.text = Mathf.RoundToInt(audio.volume * 100) + "%";
    }
}
