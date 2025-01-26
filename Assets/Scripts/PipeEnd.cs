using UnityEngine;

public class PipeEnd : MonoBehaviour
{
    public PipeManager manager; // Reference to the central PipeManager
    public Transform destination; // Where this pipe end teleports objects to
    public Transform spawnPoint; // Where objects should be spawned
    public Vector3 spawnDirecton;

    private void OnTriggerEnter(Collider other)
    {

        // Debug.Log("Pipe Triggered");

        // Inform the manager of the collision
        if (other.CompareTag("Draggable"))
        {
            manager.OnPipeTriggered(this, other.transform);

            //make the colour of the material red
            GetComponent<Renderer>().material.color = Color.red;


        }
    }

    //on trigger exit, set the cooldown to false
    private void OnTriggerExit(Collider other)
    {
        
        if(other.CompareTag("Draggable"))
        {
            manager.OnPipeExit();

            //make the colour of the material white
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void OnTransportedObject()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }

}
