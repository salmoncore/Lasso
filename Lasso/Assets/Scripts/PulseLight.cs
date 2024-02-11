using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PulseLight : MonoBehaviour
{
    private Light2D myLight;
    public float maxIntensity = 0.8f;
    public float minIntensity = 0.7f;
    public float currentPulseSpeed = 4f; //here, a value of 0.5f would take 2 seconds and a value of 2f would take half a second


    public float targetIntensity = 0.7f;
    private float currentIntensity;


    void Start()
    {
        myLight = GetComponent<Light2D>();
        //Debug.Log(myLight.intensity);
    }
    void Update()
    {
        currentIntensity = Mathf.MoveTowards(myLight.falloffIntensity, targetIntensity, Time.deltaTime * currentPulseSpeed);
        if (currentIntensity >= maxIntensity)
        {
            currentIntensity = maxIntensity;
            targetIntensity = minIntensity;
        }
        else if (currentIntensity <= minIntensity)
        {
            currentIntensity = minIntensity;
            targetIntensity = maxIntensity;
        }
        myLight.falloffIntensity = currentIntensity;
    }
}