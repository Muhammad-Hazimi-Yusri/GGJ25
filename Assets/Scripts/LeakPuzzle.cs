using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LeakPuzzle : PuzzleBase
{
    [Header("Leak Setup")]
    [SerializeField] private Transform[] leakPoints; 
    [SerializeField] private AudioSource[] leakAudioSources;
    [SerializeField] private GameObject[] waterParticles;
    [SerializeField] private GameObject[] metalSheets;
    [SerializeField] private GameObject waterPlane;
    [SerializeField] private Volume postProcessing;
    [SerializeField] private Camera playerCam;
    private MeshRenderer[] leakPointRenderers;  // Store reference to leak point renderers

    [Header("Player Settings")]
    [SerializeField, Range(5,30)] private float timeToLive; // Length of time the player can be underwater before they die
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip waterLeakSound;
    [SerializeField] private AudioClip metalPlacementSound;

    [Header("Leak Effects")]
    [SerializeField] private ParticleSystem[] leakParticleSystems; 
    [SerializeField] private Material bubbleMaterial; 
    
    [Header("Metal Sheet Settings")]
    [SerializeField] private GameObject metalSheetPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float placementSnapDistance = 0.2f;
    
    private int currentLeakIndex = 0;
    private float breathCounter; // Counter to track time left being able to breathe
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentMetalSheet;

    // Handle post processing
    private ChannelMixer channelMixer;
    private ColorAdjustments colourAdjustments;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private bool canDrain;

    
    private  void Awake()
    {
        base.Start();
       

        // Initialize leak point renderers array
        leakPointRenderers = new MeshRenderer[leakPoints.Length];

        canDrain = false; // Can't drain the water
        
        // Get and disable all leak point renderers initially
        for (int i = 0; i < leakPoints.Length; i++)
        {
            leakPointRenderers[i] = leakPoints[i].GetComponent<MeshRenderer>();
            if (leakPointRenderers[i] != null)
            {
                leakPointRenderers[i].enabled = false;
            }
        }

        // Get and disable all water effets initially
        for (int i = 0; i < waterParticles.Length; i++)
        {
            waterParticles[i].SetActive(false);
        }

        // Setup each audio source
        foreach (var leakSource in leakAudioSources)
        {
            if (leakSource != null)
            {
                Debug.Log($"Setting up leak source at {leakSource.transform.position}");
                leakSource.clip = waterLeakSound;
                leakSource.loop = true;
                leakSource.spatialBlend = 1f;
                leakSource.Stop(); // Make sure it starts stopped
            }
            else
            {
                Debug.LogError("Null leak audio source found!");
            }
        }

        // Get post processing effects
        if (postProcessing.profile.TryGet<ChannelMixer>(out channelMixer)) Debug.Log("Channel Mixer Found!");
        if (postProcessing.profile.TryGet<ColorAdjustments>(out colourAdjustments)) Debug.Log("Colour Adjustments Found!");
        if (postProcessing.profile.TryGet<ChromaticAberration>(out chromaticAberration)) Debug.Log("Chromatic Aberration Found!");
        if (postProcessing.profile.TryGet<Vignette>(out vignette)) Debug.Log("Vignette Found!");
    
    }

    private void Update()
    {
        // Handle player being underwater
        if (playerCam.transform.position.y <= waterPlane.transform.position.y)
        {
            HandleBreath(); // Handle player breathing and death
            ToggleWaterEffect(true);
        }
        else
        {
            vignette.intensity.value = 0f;
            vignette.active = false;
            breathCounter = timeToLive; // Reset breathe counter
            ToggleWaterEffect(false);
        }
    }

    public override void InitializePuzzle()
    {
        Debug.Log("Initializing Puzzle");
        currentLeakIndex = 0;
        StartCurrentLeak();
        StartCoroutine("RaiseWater");
    }

    private void StartCurrentLeak()
    {
        Debug.Log($"Starting leak {currentLeakIndex}");
        if (currentLeakIndex < leakAudioSources.Length)
        {
            // Enable current leak point renderer
            if (leakPointRenderers[currentLeakIndex] != null)
            {
                leakPointRenderers[currentLeakIndex].enabled = true;
            }

            if (waterParticles[currentLeakIndex] != null)
            {
                Debug.Log("Starting leak water effect!");
                waterParticles[currentLeakIndex].SetActive(true);
            }

            if (leakAudioSources[currentLeakIndex] != null)
            {
                Debug.Log($"Playing audio for leak {currentLeakIndex}");
                leakAudioSources[currentLeakIndex].Play();
                //SpawnNewMetalSheet();
            }
            else
            {
                Debug.LogError($"Leak audio source {currentLeakIndex} is null!");
            }
        }
        else
        {
            // Wait for placement sound to finish before completing puzzle
            StartCoroutine(CompleteWithDelay());
        }
    }

    // Toggle underwater effect on and off
    private void ToggleWaterEffect(bool enable)
    {
        channelMixer.active = enable;
        colourAdjustments.active = enable;
        chromaticAberration.active = enable;
    }

    // Ensure breath is handled
    private void HandleBreath()
    {
        breathCounter -= Time.deltaTime; // Decrease counter
        //Debug.Log("Breath Counter: " + breathCounter);

        if (!vignette.active) vignette.active = true;

        float percentDead = (timeToLive - breathCounter) / timeToLive;
        vignette.intensity.value = percentDead * 0.75f;

        if (breathCounter <= 0)
        {
            Debug.Log("Player has died");
        }
    }

    // Start Raising Water
    private IEnumerator RaiseWater()
    {
        //Define constraints
        int time = 60;
        float startY = waterPlane.transform.position.y;
        float endY = 4.2f;

        float distPerSec = (endY - startY) / time;
        float xPos = waterPlane.transform.position.x;
        float zPos = waterPlane.transform.position.z;
        
        while (waterPlane.transform.position.y < endY)
        {
            waterPlane.transform.position = new Vector3(xPos, waterPlane.transform.position.y + distPerSec / 250, zPos);
            yield return new WaitForSeconds(0.01f); 
        }

        Debug.Log("Finished Water Raise!");
        yield return null;
    }

    // Quicky lower the water
    private IEnumerator LowerWater()
    {
        //Define constraints
        int time = 5;
        float startY = waterPlane.transform.position.y;
        float endY = -0.2f;

        float distPerSec = (endY - startY) / time;
        float xPos = waterPlane.transform.position.x;
        float zPos = waterPlane.transform.position.z;

        while (waterPlane.transform.position.y > endY)
        {
            waterPlane.transform.position = new Vector3(xPos, waterPlane.transform.position.y + distPerSec / 10, zPos);
            yield return new WaitForSeconds(0.01f);
        }

        Debug.Log("Finished Water Draining!");
        yield return null;
    }

    // Drain the water if leak stopped
    public void DrainWater()
    {
        if (canDrain)
        {
            StartCoroutine("LowerWater");
        }
    }

    /*
    private void SpawnNewMetalSheet()
    {
        GameObject sheetObj = Instantiate(metalSheetPrefab, spawnPoint.position, spawnPoint.rotation);
        currentMetalSheet = sheetObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (currentMetalSheet != null)
        {
            // Subscribe to select/deselect events
            currentMetalSheet.selectExited.AddListener(OnSheetReleased);
        }
    }
    */

    // New sheet entered
    public void OnSheetEnter(int index, GameObject sheet)
    {
        if (currentLeakIndex == index)
        {
            Destroy(sheet);
            OnMetalSheetPlaced(true);
        }
    }

    /*
    private void OnSheetReleased(SelectExitEventArgs args)
    {
        if (currentMetalSheet != null)
        {
            Transform currentLeakPoint = leakPoints[currentLeakIndex];
            
            // Check distance to current leak point
            float distanceToLeak = Vector3.Distance(
                currentMetalSheet.transform.position, 
                currentLeakPoint.position
            );

            if (distanceToLeak < placementSnapDistance)
            {
                Destroy(currentMetalSheet.gameObject);
                OnMetalSheetPlaced(true);
            }
        }
    }
    */

    private void OnMetalSheetPlaced(bool correctPlacement)
    {
        AudioSource currentLeakSource = leakAudioSources[currentLeakIndex];
        
        if (correctPlacement)
        {
            // Stop current leak sound
            if (currentLeakSource != null)
            {
                currentLeakSource.Stop();
            }
            
            // Play placement sound
            if (currentLeakSource != null && metalPlacementSound != null)
            {
                currentLeakSource.PlayOneShot(metalPlacementSound);
            }

            // Stop particles
            if (leakParticleSystems[currentLeakIndex] != null)
            {
                leakParticleSystems[currentLeakIndex].Stop();
            }

            // Disable leak point renderer
            if (leakPointRenderers[currentLeakIndex] != null)
            {
                leakPointRenderers[currentLeakIndex].enabled = false;
            }

            // Disable water particle effect
            if (waterParticles[currentLeakIndex] != null)
            {
                waterParticles[currentLeakIndex].SetActive(false);
            }

            // Show metal sheet fixed
            if (metalSheets[currentLeakIndex] != null)
            {
                metalSheets[currentLeakIndex].SetActive(true);
            }
            
            // Move to next leak
            currentLeakIndex++;
            StartCurrentLeak();
        }
    }

    private IEnumerator CompleteWithDelay()
    {
        // Wait for placement sound to finish (adjust time as needed based on your sound clip length)
        yield return new WaitForSeconds(1.5f);
        CompletePuzzle();
    }

    public override void CompletePuzzle()
    {
        // Stop all audio and particles
        for(int i = 0; i < leakAudioSources.Length; i++)
        {
            if (leakAudioSources[i] != null)
            {
                leakAudioSources[i].Stop();
            }
            if (leakParticleSystems[i] != null)
            {
                leakParticleSystems[i].Stop();
            }
        }

        // Stop water filling
        StopCoroutine("RaiseWater");

        // Lower water
        StartCoroutine("LowerWater");

        canDrain = true; // Determines if the button can drain the water

        base.CompletePuzzle();
        Debug.Log("Leak Puzzle Completed!");
    }
}