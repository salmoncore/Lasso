using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] TextMeshProUGUI valueTextVOL;
    [SerializeField] TextMeshProUGUI valueTextSFX;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    public const string MIXER_MUSIC = "MusicVolume";
    public const string MIXER_SFX = "SFXVolume";

    void Awake() {
        musicSlider.onValueChanged.AddListener(setMusicVolume);
        sfxSlider.onValueChanged.AddListener(setSFXVolume);
    }

    void Start() {
        musicSlider.value = PlayerPrefs.GetFloat(AudioManager.MUSIC_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(AudioManager.SFX_KEY, 1f);
    }

    void Update() {
        PlayerPrefs.SetFloat(AudioManager.MUSIC_KEY, musicSlider.value);
        PlayerPrefs.SetFloat(AudioManager.SFX_KEY, sfxSlider.value);
        PlayerPrefs.Save();
    }
    
    public void setMusicVolume(float value) {
        valueTextVOL.SetText($"{value.ToString("P0")}");
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(value) * 20);
    }

    public void setSFXVolume(float value) {
        valueTextSFX.SetText($"{value.ToString("P0")}");
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(value) * 20);
    }
}
