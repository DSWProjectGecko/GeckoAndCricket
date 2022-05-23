using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseScripts;

public class SnakeAI : BaseCharacter
{
    private bool mustPatrol;
    public bool mustTurn;
    private bool mustAttack;
    public bool seePlayer;

    [SerializeField] GameObject distancePoint;
    [SerializeField] Collider2D visionRange;
    //Roboczy timer na czas braku animacji do ataku
    static float Timer=2f;
    float currentTimer=Timer;
    static float recoveryTimer = 1f;
    float currentRecoveryTimer = recoveryTimer;
    void Start()
    {
        mustPatrol = true;
        needGroundCollider = needWallCollider = true;
        needCeilingCollider = false;
        seePlayer = false;
        mustAttack = false;
    }
    private void FixedUpdate()
    {
        if (mustPatrol) {
            mustTurn = !Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, BaseWorld.World.groundLayer);
        }
    }
    void Update()
    {
        if (mustTurn || IsTouchingWall) {
            Flip();
        }
        if (mustPatrol)
        {
            Patrol();
        }
        else if(mustAttack){
            visionRange.enabled = false;
            Attack();
        }
    }
    void Patrol() {
        float x;
        if (seePlayer)
        {
            x = movementSpeed * 2f;

        }
        else {
            x = movementSpeed;
        }
        float y = Rigidbody.velocity.y;
        base.Move(ref x, ref y);
    }
    void Flip() {
        mustPatrol = false;
        movementSpeed *= -1;
        mustPatrol=true;
    }
    void Attack()
    {
        //Roboczy atak do zmiany potem
        if (currentTimer > 0f)
        {
            currentTimer -= Time.deltaTime;
        }
        else {
            currentTimer = Timer;
            visionRange.enabled = true;
            mustAttack = false;
            if (!seePlayer) {
                mustPatrol = true;
            }
        }  
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player") {
            float dist = Vector3.Distance(distancePoint.transform.position, collision.gameObject.transform.position);
            if (dist < 2.3f)
            {
                mustPatrol = false;
                mustAttack = true;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            seePlayer = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            seePlayer = false;
        }
    }
}
