using System;
using Gameplay.Drone;
using UnityEngine;

public class drone_patrol : MonoBehaviour
{
    [SerializeField]
    private Transform patrolPoint1;

    [SerializeField]
    private Transform patrolPoint2;

    [SerializeField] 
    private Rigidbody2D rigidBody;

    [SerializeField] 
    private DroneHitboxBehaviour droneHitboxBehaviour;

    private bool toSecondPoint = true;
    private bool checkYs = false;
    private float speed = 0.01f;
    private Vector2 direction;

    private void Awake()
    {
        droneHitboxBehaviour.OnDroneKilled += HandleDroneKilled;
    }

    private void HandleDroneKilled()
    {
        enabled = false;
    }

    private void Start()
    {
        direction = new Vector2( patrolPoint2.position.x - patrolPoint1.position.x , patrolPoint2.position.y - patrolPoint1.position.y);
        direction.Normalize();
        direction = direction * speed / Time.fixedDeltaTime;
        rigidBody.linearVelocity = direction;
        
        if (patrolPoint2.position.x == patrolPoint1.position.x)
        {
            checkYs = true;
        }

        transform.position = patrolPoint1.position;
    }

    private void FixedUpdate()
    {
        if (patrolPoint1 == patrolPoint2) return;

        if (checkYs)
        {
            var rby = rigidBody.position.y;
            if (rby < patrolPoint1.position.y ^ rby < patrolPoint2.position.y) return;

            if (toSecondPoint)
            {
                toSecondPoint = false; 
                rigidBody.linearVelocity = -direction;
            }
            else
            {
                toSecondPoint = true; 
                rigidBody.linearVelocity = direction;
            }
        }
        else
        {
            var rbx = rigidBody.position.x;
            if (rbx < patrolPoint1.position.x ^ rbx < patrolPoint2.position.x) return;

            if (toSecondPoint)
            {
                toSecondPoint = false; 
                rigidBody.linearVelocity = -direction;
            }
            else
            {
                toSecondPoint = true; 
                rigidBody.linearVelocity = direction;
            }
        }
    }
    
    private void OnDestroy()
    {
        droneHitboxBehaviour.OnDroneKilled -= HandleDroneKilled;
    }
}
