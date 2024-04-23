using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] List<AudioClip> sfxClips = new List<AudioClip>();
    [SerializeField] List<AudioClip> musicClips = new List<AudioClip>();
    public const string MUSIC_KEY = "MusicVolume";
    public const string SFX_KEY = "SFXVolume";
    public static AudioClip song;
    public static AudioClip sfx;

    void Awake() {
        if (instance == null) {
            instance = this;
            startMusic(1);
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
        SceneManager.activeSceneChanged += onSceneChanged;
        loadVolume();
    }

    // Plays the music on scene start and change
    public void startMusic(int songIndex) {
        song = musicClips[songIndex];
        instance.musicPlayer.GetComponent<AudioSource>().clip = song;
        instance.musicPlayer.GetComponent<AudioSource>().loop = true;
        instance.musicPlayer.GetComponent<AudioSource>().Play();
    }

    // Plays the sfx 
    public void startSfx(int sfxIndex) {
        sfx = sfxClips[sfxIndex];
        instance.sfxSource.GetComponent<AudioSource>().clip = sfx;
        instance.sfxSource.GetComponent<AudioSource>().loop = false;
        instance.sfxSource.GetComponent<AudioSource>().Play();
    }

    // when the scene is changed, depending on the name of the scene, it changes the music
    void onSceneChanged(Scene current, Scene next) {
        string currentName = current.name;
        Debug.Log(musicClips.Count);
        if (next.name == "MainMenu") {
            instance.musicPlayer.Stop();
            startMusic(1);
        } else if (next.name == "Tutorial") {
            
        } else if (next.name == "Lone Star"){
            instance.musicPlayer.Stop();
            startMusic(2);
        } else if (next.name == "Canyon") {
            instance.musicPlayer.Stop();
            startMusic(3);
        } else if (next.name == "Credits") {
            instance.musicPlayer.Stop();
            startMusic(4);
        }


        Debug.Log("Scene: " + next.name);
    }

    public void playerDied() {
        instance.musicPlayer.Stop();
        startMusic(0);
    }

    // loads sfx clips
    public void SFX(string sound) {
        // Plays the select button sfx
        if (sound == "select") {
            instance.sfxSource.Stop();
            startSfx(0);
        } else if (sound == "sound") {
            instance.sfxSource.Stop();
            startSfx(1); 
        } else if (sound == "hurt") {
            instance.sfxSource.Stop();
            int rand = UnityEngine.Random.Range(2, 11);
            startSfx(rand);
        } else if (sound == "lasso") {
            instance.sfxSource.Stop();
            int rand = UnityEngine.Random.Range(11, 14);
            startSfx(rand);
        }
    }

    // volume saved in audioslider.cs
    public void loadVolume()  {

        float musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1F);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1F);

        mixer.SetFloat(AudioSlider.MIXER_MUSIC, Mathf.Log10(musicVolume) * 20);
        mixer.SetFloat(AudioSlider.MIXER_SFX, Mathf.Log10(sfxVolume) * 20);

    }
}
