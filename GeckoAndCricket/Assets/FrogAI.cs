using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseScripts;
using BaseScripts.SurfaceTypes;

public class FrogAI : BaseCharacter
{
    private bool mustPatrol;
    public bool mustTurn;
    // Start is called before the first frame update
    void Start()
    {
        mustPatrol = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (IsTouchingWall && IsGrounded || InteractWithFloorType() == BaseWorld.FloorType.Lava)
        {
            Flip();
        }
        if (mustPatrol)
        {
            Patrol();
        }
    }
    void Patrol() {
        float x = movementSpeed;
        float y = Mathf.Abs(jumpForce/2);
        if (IsGrounded)
        {
            base.Move(ref x, ref y);
        }

    }
    void Flip()
    {
        mustPatrol = false;
        movementSpeed *= -1;
        mustPatrol = true;
    }
}
