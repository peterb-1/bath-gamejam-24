using UnityEngine;

public class drone_patrol : MonoBehaviour
{
    // [SerializeField]
    // private Animator droneAnimator;

    [SerializeField]
    private Transform patrolPoint1;

    [SerializeField]
    private Transform patrolPoint2;

    [SerializeField] 
    private Rigidbody2D rigidBody;

    private bool toSecondPoint = true;
    private bool checkYs = false;
    private float speed = 0.01f;
    private Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = new Vector2( patrolPoint2.position.x - patrolPoint1.position.x , patrolPoint2.position.y - patrolPoint1.position.y);
        direction.Normalize();
        direction = direction * speed / Time.fixedDeltaTime;
        rigidBody.linearVelocity = direction;
        if(patrolPoint2.position.x == patrolPoint1.position.x) { checkYs = true; }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(patrolPoint1 == patrolPoint2) { return; }


        if(checkYs)
        {
            float rby = rigidBody.position.y;
            if( !(rby < patrolPoint1.position.y ^ rby < patrolPoint2.position.y) )
            {
                if(toSecondPoint) { toSecondPoint = false; rigidBody.linearVelocity = -direction; }
                else { toSecondPoint = true; rigidBody.linearVelocity = direction; }
            }
        }
        else
        {
            float rbx = rigidBody.position.x;
            if( !(rbx < patrolPoint1.position.x ^ rbx < patrolPoint2.position.x) )
            {
                if(toSecondPoint) { toSecondPoint = false; rigidBody.linearVelocity = -direction; }
                else { toSecondPoint = true; rigidBody.linearVelocity = direction; }
            }
        }
    }
}
