using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class LeakPuzzle : PuzzleBase
{
    [Header("Leak Setup")]
    [SerializeField] private Transform[] leakPoints; 
    [SerializeField] private AudioSource[] leakAudioSources;
    [SerializeField] private GameObject[] waterParticles;
    [SerializeField] private GameObject[] metalSheets;
    private MeshRenderer[] leakPointRenderers;  // Store reference to leak point renderers
    
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
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentMetalSheet;

    private void Awake()
    {
        // Debug check
        Debug.Log($"Found {leakAudioSources.Length} leak audio sources");
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize leak point renderers array
        leakPointRenderers = new MeshRenderer[leakPoints.Length];
        
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

        /*
        // Setup particles
        for(int i = 0; i < leakAudioSources.Length; i++)
        {
            if (leakParticleSystems[i] == null)
            {
                // Create particle system if it doesn't exist
                CreateLeakParticles(i);
            }
            // Stop all particles initially
            leakParticleSystems[i].Stop();
        }
        */
    }

    public override void InitializePuzzle()
    {
        Debug.Log("Initializing Puzzle");
        currentLeakIndex = 0;
        StartCurrentLeak();
    }

    /*
    private void CreateLeakParticles(int index)
    {
        // Create a new GameObject for particles at the leak position
        GameObject particleObj = new GameObject($"LeakParticles_{index}");
        particleObj.transform.position = leakPoints[index].transform.position;
        // Rotate particles to face upwards of the leak point plane
        particleObj.transform.rotation = Quaternion.LookRotation(leakPoints[index].transform.up);
        particleObj.transform.SetParent(leakAudioSources[index].transform);

        // Add and configure particle system
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        leakParticleSystems[index] = ps;

        // Get particle system components
        var main = ps.main;
        var emission = ps.emission;
        var shape = ps.shape;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();

        // Configure main module
        main.startLifetime = 2f;
        main.startSpeed = 1f;
        main.startSize = 0.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Configure emission
        emission.rateOverTime = 50;

        // Configure shape
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.1f;

        // Configure renderer
        renderer.material = bubbleMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
    */

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
                // Start particles
                //leakParticleSystems[currentLeakIndex].Play();
                SpawnNewMetalSheet();
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
                // Snap to leak point position and rotation
                //currentMetalSheet.transform.position = currentLeakPoint.position;
                //currentMetalSheet.transform.rotation = currentLeakPoint.rotation;
                
                // Disable interaction and set kinematic
                //currentMetalSheet.enabled = false;
                //if (currentMetalSheet.GetComponent<Rigidbody>() is Rigidbody rb)
                //{
                //    rb.isKinematic = true;
                //}


                
                // Destroy the interactable
                Destroy(currentMetalSheet.gameObject);
                OnMetalSheetPlaced(true);
            }
        }
    }

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

        base.CompletePuzzle();
        Debug.Log("Leak Puzzle Completed!");
    }
}