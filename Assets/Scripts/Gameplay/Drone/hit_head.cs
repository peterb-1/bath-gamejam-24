using UnityEngine;

public class hit_head : MonoBehaviour
{
    [SerializeField] 
    private Drone_death drone_death;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("hit!");
        drone_death.RunDeathSequenceAsync();
    }
}