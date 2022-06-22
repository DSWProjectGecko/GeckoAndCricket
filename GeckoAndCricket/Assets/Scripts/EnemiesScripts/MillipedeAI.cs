using System;
using BaseScripts;
using UnityEngine;

namespace EnemiesScripts
{
    public class MillipedeAI : SnakeAI
    {
        // Public variables:
        public Vector2 patrolDistance;
        
        // Private variables:
        private Vector2 _startPosition;

        #region AI

        private new void Patrol()
        {
            float xSpeed, ySpeed;
            if (seePlayer)
            {
                ySpeed = !IsGroundedFlag && IsTouchingWall ? movementSpeed * 2.0f : 0.0f;

                xSpeed = movementSpeed * 2.0f;
            }
            else
            {
                ySpeed = !IsGroundedFlag && IsTouchingWall ? movementSpeed : 0.0f;
                xSpeed = movementSpeed;
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
            if (!mustPatrol || (!IsGroundedFlag && !IsTouchingCeilingFlag)) return;
            
            if (Math.Abs(transform.position.x) > Math.Abs(_startPosition.x + patrolDistance.x) 
                || (!Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, groundLayer) && IsGroundedFlag)
                || Math.Abs(transform.position.y) > Math.Abs(_startPosition.y + patrolDistance.y))
                mustTurn = true;
            else
                mustTurn = false;
                    
            Rigidbody.gravityScale = IsGroundedFlag ? BaseWorld.World.GetGravityScale() : 0.0f;
        }
        #endregion
    }
}
