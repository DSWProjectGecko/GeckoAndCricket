using BaseScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnemiesScripts
{
    public class SnakeAI : BaseCharacter
    {
        // Public variables:
        [SerializeField] 
        public Collider2D visionRange;
        
        public LayerMask groundLayer;
        
        // Private variables:
        [FormerlySerializedAs("_distancePoint")] [SerializeField] 
        private GameObject distancePoint;
        
        //Roboczy timer na czas braku animacji do ataku
        private const float Timer = 2f;
        private const float RecoveryTimer = 1f;
        
        private float _currentTimer = Timer;
        
#pragma warning disable CS0414
        private float _currentRecoveryTimer = RecoveryTimer;
#pragma warning restore CS0414
        
        // Public flags:
        public bool seePlayer;
        public bool mustTurn;
        
        // Protected flags:
        protected bool mustPatrol;
        protected bool mustAttack;

        #region SnakeAI
        protected void Patrol() 
        {
            float x;
            if (seePlayer)
                x = movementSpeed * 2f;
            else
                x = movementSpeed;
            
            float y = characterRigidbody.velocity.y;
            Move(ref x, ref y);
        }
        protected void Flip() 
        {
            mustPatrol = false;
            movementSpeed *= -1;
            mustPatrol=true;
        }
        protected void Attack()
        {
            //Roboczy atak do zmiany potem
            if (_currentTimer > 0f)
            {
                _currentTimer -= Time.deltaTime;
            }
            else 
            {
                _currentTimer = Timer;
                visionRange.enabled = true;
                mustAttack = false;
                if (!seePlayer)
                    mustPatrol = true;
            }  
        }
        #endregion
    
        #region Collision
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            float dist = Vector3.Distance(distancePoint.transform.position, collision.gameObject.transform.position);
            
            if (!(dist < 2.3f)) return;
            
            mustPatrol = false;
            mustAttack = true;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
                seePlayer = true;
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
                seePlayer = false;
        }
        #endregion
    
        #region Unity
        protected void Start()
        {
            mustPatrol = true;
            seePlayer = false;
            mustAttack = false;
        }
    
        protected void Update()
        {
            if (mustTurn || isTouchingWall) 
            {
                Flip();
            }
            if (mustPatrol)
            {
                Patrol();
            }
            else if(mustAttack)
            {
                visionRange.enabled = false;
                Attack();
            }
        }
        protected new void FixedUpdate()
        {
            if (mustPatrol)
                mustTurn = !Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, groundLayer);
        }
        #endregion
    }
}
