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

        #region MillipedeAI

        private new void Patrol()
        {
            float xSpeed, ySpeed;
            if (seePlayer)
            {
                ySpeed = isTouchingWall ? verticalMovementSpeed * 2.0f : 0.0f;
                xSpeed = (isGrounded || isTouchingCeiling) && !isTouchingWall ? movementSpeed * 2.0f : 0.0f;
            }
            else
            {
                ySpeed = isTouchingWall ? verticalMovementSpeed : 0.0f;
                xSpeed = (isGrounded || isTouchingCeiling) && !isTouchingWall ? movementSpeed : 0.0f;
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
                || (floorType == BaseWorld.floorType.Lava && mustTurn)
                || verticalMovementSpeed < 0.0f && !isGrounded) 
                return;

            if ((!isTouchingWall && characterRigidbody.velocity.y != 0.0f && !_isGoingDown)
                || (isTouchingCeiling && isTouchingWall)
                || (isGrounded && isTouchingWall && verticalMovementSpeed < 0.0f))
            {
                verticalMovementSpeed = -verticalMovementSpeed;
                _isGoingDown = !_isGoingDown;
                Debug.Log("Going down");
            }
            
            
            if (Math.Abs(transform.position.x) > Math.Abs(_startPosition.x + patrolDistance.x)
                     || floorType == BaseWorld.floorType.Lava
                     || (!Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, groundLayer) && IsGroundedFlag)
                     || Math.Abs(transform.position.y) > Math.Abs(_startPosition.y + patrolDistance.y)
                     || verticalMovementSpeed < 0.0f && !isGrounded)
            {
                mustTurn = true;
            }
            else
            {
                mustTurn = false;
            }


            characterRigidbody.gravityScale = IsGroundedFlag ? BaseWorld.world.GetGravityScale() : 0.0f;
        }
        #endregion
    }
}
