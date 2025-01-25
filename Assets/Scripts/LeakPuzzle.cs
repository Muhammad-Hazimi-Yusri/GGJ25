using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LeakPuzzle : PuzzleBase
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource[] leakAudioSources;
    [SerializeField] private AudioSource effectsAudioSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip waterLeakSound;
    [SerializeField] private AudioClip metalPlacementSound;

    [Header("Leak Effects")]
    [SerializeField] private ParticleSystem[] leakParticleSystems; // Array to match leakAudioSources
    [SerializeField] private Material bubbleMaterial; // Your bubble shader material
    
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
        
        // Start the puzzle
        InitializePuzzle();
    }

    public override void InitializePuzzle()
    {
        base.InitializePuzzle();
        Debug.Log("Initializing Puzzle");
        currentLeakIndex = 0;
        StartCurrentLeak();
    }

    private void CreateLeakParticles(int index)
    {
        // Create a new GameObject for particles at the leak position
        GameObject particleObj = new GameObject($"LeakParticles_{index}");
        particleObj.transform.position = leakAudioSources[index].transform.position;
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

    private void StartCurrentLeak()
    {
        Debug.Log($"Starting leak {currentLeakIndex}");
        if (currentLeakIndex < leakAudioSources.Length)
        {
            if (leakAudioSources[currentLeakIndex] != null)
            {
                Debug.Log($"Playing audio for leak {currentLeakIndex}");
                leakAudioSources[currentLeakIndex].Play();
                // Start particles
                leakParticleSystems[currentLeakIndex].Play();
                SpawnNewMetalSheet();
            }
            else
            {
                Debug.LogError($"Leak audio source {currentLeakIndex} is null!");
            }
        }
        else
        {
            CompletePuzzle();
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
            // Check if near current leak point
            float distanceToLeak = Vector3.Distance(
                currentMetalSheet.transform.position, 
                leakAudioSources[currentLeakIndex].transform.position
            );

            if (distanceToLeak < placementSnapDistance)
            {
                // Correct placement
                currentMetalSheet.transform.position = leakAudioSources[currentLeakIndex].transform.position;
                currentMetalSheet.transform.rotation = leakAudioSources[currentLeakIndex].transform.rotation;
                
                // Disable further interaction with this sheet
                currentMetalSheet.enabled = false;
                if (currentMetalSheet.GetComponent<Rigidbody>() is Rigidbody rb)
                {
                    rb.isKinematic = true;
                }
                
                OnMetalSheetPlaced(true);
            }
        }
    }

    private void OnMetalSheetPlaced(bool correctPlacement)
    {
        if (effectsAudioSource != null && metalPlacementSound != null)
        {
            effectsAudioSource.PlayOneShot(metalPlacementSound);
        }
        
        if (correctPlacement)
        {
            // Stop current leak sound
            leakAudioSources[currentLeakIndex].Stop();
            // Stop particles
            leakParticleSystems[currentLeakIndex].Stop();
            // Move to next leak
            currentLeakIndex++;
            StartCurrentLeak();
        }
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