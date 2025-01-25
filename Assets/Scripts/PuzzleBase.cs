// PuzzleBase.cs
using UnityEngine;

public enum PuzzleState
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public abstract class PuzzleBase : MonoBehaviour
{
    [SerializeField] protected PuzzleState currentState = PuzzleState.NotStarted;
    protected AudioSource[] puzzleAudioSources;
    
    protected virtual void Start()
    {
        puzzleAudioSources = GetComponents<AudioSource>();
    }

    public virtual void InitializePuzzle()
    {
        currentState = PuzzleState.InProgress;
    }

    public virtual void CompletePuzzle()
    {
        currentState = PuzzleState.Completed;
        StopAllPuzzleAudio();
    }

    public virtual void FailPuzzle()
    {
        currentState = PuzzleState.Failed;
        StopAllPuzzleAudio();
    }

    protected virtual void StopAllPuzzleAudio()
    {
        if (puzzleAudioSources != null)
        {
            foreach (var audioSource in puzzleAudioSources)
            {
                if (audioSource != null)
                    audioSource.Stop();
            }
        }
    }
}