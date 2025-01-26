using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource audio;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float timeBetweenAnim;
    [SerializeField] private float timeBetweenAudios;

    private float animCount;
    private float audioCount;
    private bool animating;
    private bool playingAudio;
    private string[] animBools;
    private string whichAnim;
    private AudioClip whichAudio;


    // Start is called before the first frame update
    void Start()
    {
        animBools = new string[] { "fishSwim", "bigFish", "theEye" };
        animCount = 3;
        audioCount = 5;
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

        if (audioCount > 0)
        {
            audioCount -= Time.deltaTime;
        }
        else if (!playingAudio)
        {
            playingAudio = true;
            whichAudio = audioClips[Random.Range(0, audioClips.Length)];
            StartCoroutine("playClip");

        }
    }

    // Play an audio clip
    private IEnumerator playClip()
    {
        Debug.Log("Start Audio: " + whichAudio.name);   

        audio.clip = whichAudio;
        audio.Play();
        yield return new WaitForSeconds(audio.clip.length);
        audio.Stop();

        Debug.Log("End Audio: " + whichAudio.name);
        playingAudio = false;
        audioCount = timeBetweenAudios;
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
