using UnityEngine;
using Cysharp.Threading.Tasks;
using Gameplay.Player;

public class hit_head : MonoBehaviour
{
    [SerializeField] 
    private Drone_death drone_death;
    [SerializeField]
    private Collider2D collider;

    private PlayerMovementBehaviour playerMovementBehaviour;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Awake()
    {
        await UniTask.WaitUntil(PlayerAccessService.IsReady);

        playerMovementBehaviour = PlayerAccessService.Instance.PlayerMovementBehaviour;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        drone_death.RunDeathSequenceAsync();
        playerMovementBehaviour.HeadJump();
        Destroy(collider);
    }
}