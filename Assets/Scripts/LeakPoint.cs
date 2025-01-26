using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeakPoint : MonoBehaviour
{
    [SerializeField] private int leakIndex;
    [SerializeField] private LeakPuzzle leakPuzzle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Metal"))
        {
            Debug.Log("Sheet Entered");
            leakPuzzle.OnSheetEnter(leakIndex, other.gameObject);
        }
    }
}
