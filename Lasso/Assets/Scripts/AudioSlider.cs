using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private AudioMixer Mixer;
    [SerializeField] private TextMeshProUGUI ValueText;
    
    public void onChangeSlider(float value) {
        ValueText.SetText($"{value.ToString("N0")}");

        Mixer.SetFloat("Volume", Mathf.Log10(value) * 20);
    }
}
