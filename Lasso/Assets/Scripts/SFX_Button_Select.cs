using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX_Button_Select : MonoBehaviour
{
    public GameObject musicPlayer;
    public AudioManager audioManagerScript;

    public void onHover() {
        musicPlayer = GameObject.Find("MusicPlayer");
        audioManagerScript = musicPlayer.GetComponent<AudioManager>();
        audioManagerScript.SFX("sound");
    }

    public void onSelect() {
        musicPlayer = GameObject.Find("MusicPlayer");
        audioManagerScript = musicPlayer.GetComponent<AudioManager>();
        audioManagerScript.SFX("select");
    }
}
