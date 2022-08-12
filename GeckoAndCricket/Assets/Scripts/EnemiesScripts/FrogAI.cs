using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseScripts;
using BaseScripts.SurfaceTypes;
using PlayerScripts;

public class FrogAI : BaseCharacter
{
    private bool mustPatrol;
    public bool mustTurn;
    public bool grapplingPlayer;
    // Start is called before the first frame update
    void Start()
    {
        mustPatrol = true;
        grapplingPlayer = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (IsTouchingWall && IsGrounded || FloorType == BaseWorld.FloorType.Lava)
        {
            Flip();
        }
        if (mustPatrol)
        {
            Patrol();
        }
        else {
            Attack();
        }
    }
    void Patrol() {
        float x = movementSpeed;
        float y = Mathf.Abs(jumpForce/2);
        if (IsGrounded)
        {
            Move(x,y);
        }

    }
    void Attack() {
        float x = movementSpeed*3;
        float y = Mathf.Abs(jumpForce / 1.5f);
        if (IsGrounded && !grapplingPlayer)
        {
            Move(x,y);
        }
    }
    public void Move(float x,float y){
        base.Move(ref x, ref y);
    }
    void Flip()
    {
        mustPatrol = false;
        movementSpeed *= -1;
        mustPatrol = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            mustPatrol = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mustPatrol = true;
            grapplingPlayer = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            grapplingPlayer = true;
            gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            Transform grapple = collision.gameObject.transform.Find("GrapplePosition").transform;
            this.transform.parent = grapple;
            this.transform.position = grapple.position;
            Rigidbody.isKinematic = true;
            Rigidbody.velocity = new Vector2(0f, 0f);
            collision.gameObject.GetComponent<Player>().isGrappled = true;



        }
    }
}
