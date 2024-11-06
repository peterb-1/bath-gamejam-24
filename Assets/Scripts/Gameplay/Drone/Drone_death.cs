using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Drone_death : MonoBehaviour
{

    [SerializeField] 
    private Animator droneAnimator;
    [SerializeField] 
    private Rigidbody2D rigidBody;
    [SerializeField] 
    private SpriteRenderer spriteRenderer;

    private static readonly int Died = Animator.StringToHash("died");
    private float alpha = 1.0f;

    private int count = 0;


    // Update is called once per frame
    void FixedUpdate()
    {
        if(alpha < 1.0f && alpha > 0.0f)
        {
            alpha -= 0.0002f / Time.fixedDeltaTime;
            Debug.Log(alpha);
            var temp = spriteRenderer.color;
            temp.a = alpha;
            spriteRenderer.color = temp;
        }
        count ++;
        Debug.Log(count);
        if(count == 400)
        {
            RunDeathSequenceAsync();
        }
    }

    private async UniTask RunDeathSequenceAsync()
    {
        rigidBody.linearVelocity = new Vector2(0.0f, 0.0f);
        droneAnimator.SetTrigger(Died);
        alpha = 0.99f;

        await UniTask.Delay(TimeSpan.FromSeconds(2.0f));

        Destroy(gameObject);
    }
}
