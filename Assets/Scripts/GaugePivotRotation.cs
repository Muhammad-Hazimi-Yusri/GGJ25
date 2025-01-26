using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class GaugePivotRotation : MonoBehaviour
{
    [SerializeField] private XRSlider Slider;
    
    // Optional: Set minimum and maximum rotation angles
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 360f;

    [SerializeField] private float randomnessStrength = 5f;  // Controls how much random movement
    [SerializeField] private float randomnessSpeed = 2f;     // How fast the random movement updates
    
    private float randomOffset = 0f;
    private float noiseOffset;

    void Start()
    {
        // Initialize random seed
        noiseOffset = Random.Range(0f, 1000f);
    }

    public void SetRandomness(float strength, float speed)
    {
        randomnessStrength = strength;
        randomnessSpeed = speed;
    }

    void Update()
    {
        // Generate smooth random movement using Perlin noise
        randomOffset = Mathf.PerlinNoise(Time.time * randomnessSpeed + noiseOffset, 0f) * 2f - 1f;
        randomOffset *= randomnessStrength;

        // Get base angle from slider
        float baseAngle = Mathf.Lerp(minAngle, maxAngle, Slider.value);
        
        // Add random movement to the base angle
        float finalAngle = baseAngle + randomOffset;
        
        // Set the rotation
        transform.localRotation = Quaternion.Euler(finalAngle, 0f, 0f);
    }
}