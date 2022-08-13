using BaseScripts;
using PlayerScripts;
using UnityEngine;

namespace EnemiesScripts
{
    public class FrogAI : BaseCharacter
    {
        // public flags:
        public bool mustTurn;
        public bool grapplingPlayer;
        
        // private flags:
        private bool _mustPatrol;

        #region FrogAI
        private void Patrol() 
        {
            float x = movementSpeed;
            float y = Mathf.Abs(jumpForce/2);
            
            if (isGrounded)
                Move(ref x, ref y);
        }

        private void Attack() 
        {
            float x = movementSpeed*3;
            float y = Mathf.Abs(jumpForce / 1.5f);
            
            if (isGrounded && !grapplingPlayer)
                Move(ref x, ref y);
        }
        public void Move(float x,float y)
        {
            base.Move(ref x, ref y);
        }

        private void Flip()
        {
            _mustPatrol = false;
            movementSpeed *= -1;
            _mustPatrol = true;
        }
        #endregion

        #region Unity
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                _mustPatrol = false;
        }
        
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player")) return;
            
            _mustPatrol = true;
            grapplingPlayer = false;
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            
            grapplingPlayer = true;
            gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            Transform grapple = collision.gameObject.transform.Find("GrapplePosition").transform;
            
            Transform frogTransform = transform;
            
            frogTransform.parent = grapple;
            frogTransform.position = grapple.position;
            characterRigidbody.isKinematic = true;
            characterRigidbody.velocity = new Vector2(0f, 0f);
            collision.gameObject.GetComponent<Player>().isGrappled = true;
        }
        
        private void Start()
        {
            _mustPatrol = true;
            grapplingPlayer = false;
        }

        private void Update()
        {
            if (isTouchingWall && isGrounded || floorType == BaseWorld.floorType.Lava)
            {
                Flip();
            }
            if (_mustPatrol)
            {
                Patrol();
            }
            else {
                Attack();
            }
        }
        #endregion
    }
}
