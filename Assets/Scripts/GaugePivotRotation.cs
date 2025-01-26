using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class GaugePivotRotation : MonoBehaviour
{
    [SerializeField] private XRSlider Slider;
    
    // Base rotation range from slider (0-360)
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
        // Calculate random rotation - allow it to freely affect the gauge
        randomOffset = Mathf.PerlinNoise(Time.time * randomnessSpeed + noiseOffset, 0f) * 2f - 1f;
        randomOffset *= randomnessStrength;

        // Map slider value to full rotation range
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, Slider.value);
        
        // Add random offset without clamping - let it go outside the range
        float finalAngle = targetAngle + randomOffset;
        
        // Apply rotation
        transform.localRotation = Quaternion.Euler(finalAngle, 0f, 0f);
    }

    public float GetCurrentAngle()
    {
        return transform.localRotation.eulerAngles.x;
    }
}