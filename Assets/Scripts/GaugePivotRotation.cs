using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class ModifiedGaugeRotation : MonoBehaviour
{
    [SerializeField] private XRSlider slider;
    
    [Header("Rotation Settings")]
    [SerializeField] private float minAngle = 0f;  // The minimum angle in inspector
    [SerializeField] private float maxAngle = 360f;  // The maximum angle in inspector
    [SerializeField] private bool clampRotation = false;  // Option to clamp rotation within min/max range
    
    [Header("Randomness Settings")]
    [SerializeField] private float randomnessStrength = 5f;
    [SerializeField] private float randomnessSpeed = 2f;
    [SerializeField] private bool useRandomness = true;
    
    private float currentAngle;
    private float randomOffset;
    private float noiseOffset;

    private void Start()
    {
        if (slider == null)
        {
            Debug.LogError("Slider reference is missing!", this);
            enabled = false;
            return;
        }

        noiseOffset = Random.Range(0f, 1000f);
        currentAngle = minAngle;
    }

    public void SetRandomness(float strength, float speed)
    {
        randomnessStrength = strength;
        randomnessSpeed = speed;
    }

    public void EnableRandomness(bool enable)
    {
        useRandomness = enable;
    }

    private void Update()
    {
        // Calculate base angle from slider
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, slider.value);
        
        // Calculate random offset if enabled
        if (useRandomness)
        {
            randomOffset = Mathf.PerlinNoise(Time.time * randomnessSpeed + noiseOffset, 0f) * 2f - 1f;
            randomOffset *= randomnessStrength;
        }
        else
        {
            randomOffset = 0f;
        }

        // Calculate final angle
        currentAngle = targetAngle + randomOffset;

        // Clamp if enabled
        if (clampRotation)
        {
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        }

        // Apply rotation
        transform.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }

    public float GetCurrentAngle()
    {
        // Returns the actual angle that matches what you see in the inspector
        return currentAngle;
    }

    public bool IsAngleInRange(Vector2 range)
    {
        float angle = GetCurrentAngle();
        
        if (range.x <= range.y)
        {
            return angle >= range.x && angle <= range.y;
        }
        else
        {
            // Handle wrapping around 360 degrees
            return angle >= range.x || angle <= range.y;
        }
    }
}