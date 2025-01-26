using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class GaugePivotRotation : MonoBehaviour
{
    [SerializeField] private XRSlider Slider;
    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 360f;
    
    [SerializeField] private float randomnessStrength = 5f;
    [SerializeField] private float randomnessSpeed = 2f;
    
    private float randomOffset = 0f;
    private float noiseOffset;

    void Start()
    {
        noiseOffset = Random.Range(0f, 1000f);
    }

    public void SetRandomness(float strength, float speed)
    {
        randomnessStrength = strength;
        randomnessSpeed = speed;
    }

    void Update()
    {
        // Calculate random rotation
        randomOffset = Mathf.PerlinNoise(Time.time * randomnessSpeed + noiseOffset, 0f) * 2f - 1f;
        randomOffset *= randomnessStrength;

        // Map slider value to angle range
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, Slider.value);
        
        // Apply both target angle and random offset
        float finalAngle = targetAngle + randomOffset;
        
        // Set rotation directly on X axis
        transform.localRotation = Quaternion.Euler(finalAngle, 0f, 0f);
    }

    public float GetCurrentAngle()
    {
        return transform.localRotation.eulerAngles.x;
    }
}