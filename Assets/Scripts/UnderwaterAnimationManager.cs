using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private float timeBetweenAnim;

    private float animCount;
    private bool animating;
    private string[] animBools;
    private string whichAnim;


    // Start is called before the first frame update
    void Start()
    {
        animBools = new string[] { "fishSwim", "bigFish", "theEye" };
        animCount = 3;
    }

    void Update()
    {
        if (animCount > 0)
        {
            animCount -= Time.deltaTime;
        }
        else if (!animating)
        {
            animating = true;
            whichAnim = animBools[Random.Range(0, animBools.Length)];
            Debug.Log("Start Animation: " + whichAnim);
            anim.SetBool(whichAnim, true);
        }
    }

    // Call when animations are ended
    public void endAnimation()
    {
        Debug.Log("End Animation: " + whichAnim);
        anim.SetBool(whichAnim, false);
        animCount = timeBetweenAnim;
        animating = false;
    }
}
