using UnityEngine;

public class PipeEnd : MonoBehaviour
{
    public PipeManager manager; // Reference to the central PipeManager
    public Transform destination; // Where this pipe end teleports objects to

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("Pipe Triggered");

        // Inform the manager of the collision
        if (other.CompareTag("Draggable"))
        {
            manager.OnPipeTriggered(this, other.transform);
        }
    }
}
