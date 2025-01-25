// PuzzleManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    private static PuzzleManager instance;
    public static PuzzleManager Instance => instance;

    [SerializeField] private List<PuzzleBase> allPuzzles = new List<PuzzleBase>();
    private int currentPuzzleIndex = -1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Automatically start first puzzle
        StartNextPuzzle();
    }

    public void StartNextPuzzle()
    {
        currentPuzzleIndex++;
        if (currentPuzzleIndex < allPuzzles.Count)
        {
            Debug.Log($"Starting Puzzle {currentPuzzleIndex}");
            allPuzzles[currentPuzzleIndex].InitializePuzzle();
        }
        else
        {
            Debug.Log("All puzzles completed!");
        }
    }

    public void CompletePuzzle()
    {
        if (currentPuzzleIndex >= 0 && currentPuzzleIndex < allPuzzles.Count)
        {
            allPuzzles[currentPuzzleIndex].CompletePuzzle();
            StartNextPuzzle();
        }
    }
}