using System;
using BaseScripts;
using UnityEngine;

namespace EnemiesScripts
{
    public class MillipedeAI : SnakeAI
    {
        // Public variables:
        public float verticalMovementSpeed = 5.0f;
        public Vector2 patrolDistance;
        
        // Private variables:
        private Vector2 _startPosition;
        
        // Private flags;
        private bool _isGoingDown;

        #region AI

        private new void Patrol()
        {
            float xSpeed, ySpeed;
            if (seePlayer)
            {
                ySpeed = IsTouchingWall ? verticalMovementSpeed * 2.0f : 0.0f;
                xSpeed = (IsGrounded || IsTouchingCeiling) && !IsTouchingWall ? movementSpeed * 2.0f : 0.0f;
            }
            else
            {
                ySpeed = IsTouchingWall ? verticalMovementSpeed : 0.0f;
                xSpeed = (IsGrounded || IsTouchingCeiling) && !IsTouchingWall ? movementSpeed : 0.0f;
            }
            
            Move(ref xSpeed, ref ySpeed);
        }

        #endregion
        
        #region Unity

        private new void Start()
        {
            base.Start();
            _startPosition = transform.position;
        }
        private new void Update()
        {
            if (mustTurn)
                Flip();

            if (mustPatrol)
            {
                Patrol();
            }
            else if (mustAttack)
            {
                visionRange.enabled = false;
                Attack();
            }
        }

        private new void FixedUpdate()
        {
            CheckCollision();

            if (!mustPatrol 
                || (!IsGroundedFlag && !IsTouchingCeilingFlag)
                || (FloorType == BaseWorld.FloorType.Lava && mustTurn)
                || verticalMovementSpeed < 0.0f && !IsGrounded) 
                return;

            if ((!IsTouchingWall && Rigidbody.velocity.y != 0.0f && !_isGoingDown)
                || (IsTouchingCeiling && IsTouchingWall)
                || (IsGrounded && IsTouchingWall && verticalMovementSpeed < 0.0f))
            {
                verticalMovementSpeed = -verticalMovementSpeed;
                _isGoingDown = !_isGoingDown;
                Debug.Log("Going down");
            }
            
            
            if (Math.Abs(transform.position.x) > Math.Abs(_startPosition.x + patrolDistance.x)
                     || FloorType == BaseWorld.FloorType.Lava
                     || (!Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, groundLayer) && IsGroundedFlag)
                     || Math.Abs(transform.position.y) > Math.Abs(_startPosition.y + patrolDistance.y)
                     || verticalMovementSpeed < 0.0f && !IsGrounded)
            {
                mustTurn = true;
            }
            else
            {
                mustTurn = false;
            }


            Rigidbody.gravityScale = IsGroundedFlag ? BaseWorld.World.GetGravityScale() : 0.0f;
        }
        #endregion
    }
}
