using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Colour;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

public class DroneColourScript : MonoBehaviour
{
    [SerializeField]
    private bool isColoured;

    [SerializeField] 
    private ColourId colourId;

    [SerializeField] 
    private Collider2D headCollider;
    
    [SerializeField] 
    private Collider2D deathCollider;

    [SerializeField] 
    private SpriteRenderer spriteRenderer;

    [SerializeField] 
    private ColourDatabase colourDatabase;

    private bool isActive = true;
    private float hiddenAlpha = 0.3f;

     private void Awake()
    {
        if(isColoured)
        {
            ColourManager.OnColourChangeStarted += HandleColourChangeStarted;
            ColourManager.OnColourChangeInstant += HandleColourChangeInstant;
        }
    }

    private void HandleColourChangeStarted(ColourId colour, float duration)
    {

        var shouldActivate = colourId == colour;

        if (shouldActivate != isActive)
        {
            isActive = shouldActivate;
            ToggleDroneAsync(duration).Forget();
        }
    }

    private void HandleColourChangeInstant(ColourId colour)
    {
        isActive = colourId == colour;
        ToggleDroneAsync(0f).Forget();
    }

    private async UniTask ToggleDroneAsync(float duration)
    {
        ToggleCollidersAsync().Forget();

        float changeBy = -0.01f;
        float delay = 0.01f;
        if(isActive) { changeBy *= -1; }

        var spriteColor = spriteRenderer.color;
        float currentAlpha = spriteColor.a;

        while((currentAlpha < 1.0f && isActive) || (currentAlpha > hiddenAlpha && !isActive))
        {
            currentAlpha += changeBy;
            spriteColor = spriteRenderer.color;
            spriteColor.a = currentAlpha;
            spriteRenderer.color = spriteColor;
            await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true);

        }
    }

    private async UniTask ToggleCollidersAsync()
    {
        deathCollider.enabled = isActive;
        headCollider.enabled = isActive;
    }


    #if UNITY_EDITOR
    [Button("Colour Drone")]
    private void ColourDrone()
    {
        if(isColoured)
        {
            // write code
        }
    }
    #endif
}
