using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathManager : MonoBehaviour
{
    [SerializeField] private Sprite[] deathImages;
    [SerializeField] private Image image;

    private void Awake()
    {
        image.sprite = deathImages[Random.Range(0, deathImages.Length)];
    }
}
