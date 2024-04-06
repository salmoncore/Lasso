using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] List<AudioClip> sfxClips = new List<AudioClip>();
    public const string MUSIC_KEY = "MusicVolume";
    public const string SFX_KEY = "SFXVolume";

    void Awake() {
        if (instance == null) {
            instance = this;

            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        loadVolume();
    }

    public void SFX() {
        AudioClip clip = sfxClips[0];
        sfxSource.PlayOneShot(clip);
    }

    // volume saved in audioslider.cs
    void loadVolume()  {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1F);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1F);

        mixer.SetFloat(AudioSlider.MIXER_MUSIC, Mathf.Log10(musicVolume) * 20);
        mixer.SetFloat(AudioSlider.MIXER_SFX, Mathf.Log10(sfxVolume) * 20);
    }
}
