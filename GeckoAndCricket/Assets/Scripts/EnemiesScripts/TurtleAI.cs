using BaseScripts;
using UnityEngine;

namespace EnemiesScripts
{
    public class TurtleAI : BaseCharacter
    {
        [Header("Turtle Properties:")]
        public float playerLaunchForce = 3f;
        public int jumpsToFlip = 2;
        
        [Header("Turtle Colliders:")]
        public Collider2D bottomCollider;
        public Collider2D topCollider;

        // Turtle private variables:
        private Vector3 _localPosition;
        
        private int _jumpsLeft;
        
        // Turtle private flags:
        private bool _canMove = true;
        private bool _isTouchingPlayer;
        private bool _isTurningUpsideDown;

        #region Interactions

        private void Flip()
        {
            if (_jumpsLeft > 0)
            {
                _jumpsLeft--;
                return;
            }
            
            if (!_canMove)
                return;
            
            _canMove = false;
            
            FlipVertically();
            characterRigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
            bottomCollider.enabled = false;
            topCollider.enabled = false;
            
            Transform position = GetComponent<Transform>();
            _localPosition = position.localPosition;
            _localPosition = new Vector3(_localPosition.x, _localPosition.y - 1.4f, _localPosition.z);
            position.localPosition = _localPosition;
            
            _isTurningUpsideDown = true;
        }

        private void LaunchPlayer()
        {
            if (_isTurningUpsideDown)
                return;
            
            BaseWorld.player.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, playerLaunchForce), ForceMode2D.Impulse);
        }

        private void InteractWithColliders()
        {
            if (topCollider.IsTouching(BaseWorld.player.GetComponent<CircleCollider2D>()) && !_isTouchingPlayer)
            {
                _isTouchingPlayer = true;
                BaseWorld.player.GetComponent<BaseCharacter>().IsGroundedFlag = true;
                Flip();
            }
            else if (bottomCollider.IsTouching(BaseWorld.player.GetComponent<CircleCollider2D>()))
            {
                Debug.Log("LaunchPlayer");
                LaunchPlayer();
            }
            else if (!bottomCollider.IsTouching(BaseWorld.player.GetComponent<CircleCollider2D>()) &&
                      !topCollider.IsTouching(BaseWorld.player.GetComponent<CircleCollider2D>()) && _isTouchingPlayer)
                _isTouchingPlayer = false;
        }
        
        #endregion

        #region Unity

        private void Start()
        {
            _jumpsLeft = jumpsToFlip;
        }
        

        private new void FixedUpdate()
        {
            base.FixedUpdate();
            InteractWithColliders();
            
            if (!_isTurningUpsideDown)
                return;
            
            if (Physics2D.OverlapPoint(new Vector2(_localPosition.x, _localPosition.y + 1.0f), LayerMask.NameToLayer("Player"))) 
                return;

            characterRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            bottomCollider.enabled = true;
            topCollider.enabled = true;
            _isTurningUpsideDown = false;
            LaunchPlayer();
        }

        #endregion
    }
}
