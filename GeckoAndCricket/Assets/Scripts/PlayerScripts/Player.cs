using System.Collections;
using BaseScripts;
using DebugUtility;
using UnityEngine;

namespace PlayerScripts
{
    //[RequireComponent(typeof(BaseWorld))]
    public class Player : BaseCharacter
    {
        [Header("Player movement variables:")]
        public float wallJumpForce = 13f;
        public float wallJumpTime = 0.05f;
        public float detachTimer = 1f;
        
        public bool doubleJump;
        
        public Transform attachedTo;
        
        [Header("Player stamina variables:")]
        public float maxStamina = 3f;
        // Jump
        public float staminaToJump = 1f;
        public float staminaJumpCost = 1f;
        // Regeneration:
        public float staminaRegenerationValue = 0.5f;
        public int staminaRegenerationTime = 2;
        // Degradation:
        public float staminaDegradationValue = 0.5f;
        public int staminaDegradationTime = 1;

        [Header("Player input:")] 
        public KeyCode climbKey = KeyCode.X;

        // Private player variables:
        private float _stamina;

        private bool _isWallJumping;
        
        private bool _wasTouchingCeiling;
        private bool _wasTouchingWall;
        private bool _wasClimbKeyPressed;

        private GameObject _disregard;
        private GameObject disregard;

        #region Movement
        private void Move()
        {
            float x = Input.GetAxis("Horizontal") * movementSpeed;
            float y = Rigidbody.velocity.y;

            if (IsTouchingCeiling && !IsGrounded && !Input.GetKeyUp(climbKey) && _stamina > 0f)
            {
                Rigidbody.gravityScale = 0f;
                _wasTouchingCeiling = true;
            }
            else if (_wasTouchingCeiling)
            {
                _wasClimbKeyPressed = false;
                _wasTouchingCeiling = false;
                Rigidbody.gravityScale = BaseWorld.World.GetGravityScale();
            }

            if (IsTouchingWall && !_isWallJumping && _wasClimbKeyPressed)
            {
                if (_stamina > 0f)
                {
                    InteractWithWallType();
                    y = Input.GetAxis("Vertical");
                    if (y > 0)
                        SetRotationZ(90f);
                    else if (y < 0)
                        SetRotationZ(-90f);
                    y *= movementSpeed;
                }
                else
                {
                    Rigidbody.gravityScale = BaseWorld.World.GetGravityScale();
                    SetRotationZ(-90f);
                    y = Mathf.Clamp(Rigidbody.velocity.y, -wallSlidingSpeed, float.MaxValue);
                    _wasClimbKeyPressed = false;
                }
                _wasTouchingWall = true;
            }
            else if (_wasTouchingWall && !_isWallJumping)
            {
                Rigidbody.gravityScale = BaseWorld.World.GetGravityScale();
                SetRotationZ(0f);
                _wasTouchingWall = false;
                _wasClimbKeyPressed = false;
            }
            base.Move(ref x, ref y);
        }

        private void WallJump()
        {
            _isWallJumping = true;
            
            Invoke(nameof(SetWallJumpToFalse), wallJumpTime);
            if (!_isWallJumping) return;

            //Rigidbody.gravityScale = BaseWorld.World.GetGravityScale() / 2;
            
            float direction = isFacingRight ? -1f : 1f;
            if (Input.GetAxis("Horizontal") != 0)
                direction = -Input.GetAxis("Horizontal");

            direction *= Rigidbody.velocity.x;
            Rigidbody.gravityScale = BaseWorld.World.GetGravityScale();
            Rigidbody.AddForce(new Vector2(wallJumpForce * direction , jumpForce), ForceMode2D.Impulse);
        }

        private void SetWallJumpToFalse()
        {
            _isWallJumping = false;
        }
		
        private new void Jump()
        {
            if (!Input.GetKeyDown(KeyCode.Space) || _stamina < staminaToJump) return;

            _stamina -= staminaJumpCost;
            if (IsGrounded || IsAttachedToRope) 
            {
                base.Jump();
                doubleJump = true;
            }
            else if (IsTouchingWall)
            {
                WallJump();
                doubleJump = true;
            }
            else if (doubleJump)
            {
                base.Jump();
                doubleJump = false;
            }
        }
		
        private void Stomp(bool isTouchingStuff) 
        {
            if (Input.GetKeyDown("s") && !isTouchingStuff) 
            {
                Stomp();
            }
        }
		
        private void AttachToRope(Rigidbody2D ropeSeg) 
        {
            Debug.Log(ropeSeg);
            ropeSeg.gameObject.GetComponent<RopeSegment>().isPlayerAttached = true;
            HingeJoint.connectedBody = ropeSeg;
            HingeJoint.enabled = true;
            IsAttachedToRope = true;
            attachedTo = ropeSeg.gameObject.transform.parent;
        }
		
        private void DetachFromRope() 
        {
            HingeJoint.connectedBody.GetComponent<RopeSegment>().isPlayerAttached = false;
            HingeJoint.enabled = false;
            IsAttachedToRope = false;
            HingeJoint.connectedBody = null;
        }
		
        private void SlideOnRope(int direction) {
            RopeSegment actualRopeSegment = HingeJoint.connectedBody.gameObject.GetComponent<RopeSegment>();
            GameObject newRopeSegment = null;
            if (direction > 0)
            {
                if (actualRopeSegment.above != null) {
                    if (actualRopeSegment.above.gameObject.GetComponent<RopeSegment>() != null) {
                        newRopeSegment = actualRopeSegment.above;
                    }
                }
            }
            else 
            {
                if (actualRopeSegment.below != null)
                {
                    newRopeSegment = actualRopeSegment.below;
                }
            }
            if (newRopeSegment != null){
                transform.position = newRopeSegment.transform.position;
                actualRopeSegment.isPlayerAttached = false;
                newRopeSegment.GetComponent<RopeSegment>().isPlayerAttached = true;
                HingeJoint.connectedBody = newRopeSegment.GetComponent<Rigidbody2D>();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsAttachedToRope && collision.gameObject.CompareTag("Rope"))
            {
                if (attachedTo != collision.gameObject.transform.parent)
                {
                    Debug.Log("Pierwszy if");
                    if (_disregard == null || collision.gameObject.transform.parent.gameObject != _disregard)
                    {
                        if (!IsAttachedToRope && collision.gameObject.tag == "Rope" && detachTimer <= 0f)
                        {
                            if (attachedTo != collision.gameObject.transform.parent)
                            {
                                if (disregard == null || collision.gameObject.transform.parent.gameObject != disregard)
                                {
                                    AttachToRope(collision.gameObject.GetComponent<Rigidbody2D>());
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Swing() 
        {
            if (Input.GetKey("a"))
            {
                if (IsAttachedToRope)
                {
                    Rigidbody.AddRelativeForce(new Vector2(-1, 0) * pushForce);

                }
            }
            if (Input.GetKey("d"))
            {
                if (IsAttachedToRope)
                {
                    Rigidbody.AddRelativeForce(new Vector2(1, 0) * pushForce);
                }
            }
            if (Input.GetKeyDown("w") && IsAttachedToRope)
            {
                SlideOnRope(1);
            }
            if (Input.GetKeyDown("s") && IsAttachedToRope)
            {
                SlideOnRope(-1);
            }
            if (Input.GetKeyDown("space") && IsAttachedToRope)
            {
                detachTimer = 1f;
                DetachFromRope();
            }
        }
		
        private void DetachRopeTimer() {
            if (!IsAttachedToRope && detachTimer>0f)
            {
                detachTimer -= Time.deltaTime;
            }
            else if(detachTimer<=0f &&attachedTo!=null){
                attachedTo = null;
            }
        }

        #endregion
        
        #region Stamina

        private bool _isCoroutineRunning;

        private IEnumerator Stamina(bool isUsingStamina)
        {
            _isCoroutineRunning = true;

            /*float staminaUsage = isUsingStamina ? staminaDegradationValue : staminaRegenerationValue;

            do
            {
                yield return new WaitForSeconds(staminaRegenerationTime);
                _stamina += staminaUsage;
                Debug.Log("Stamina = " + _stamina);
            } while (_stamina < maxStamina || _stamina <= 0);*/
                
            while (_stamina < maxStamina && IsGrounded)
            {
                yield return new WaitForSeconds(staminaRegenerationTime);
                _stamina += staminaRegenerationValue;
                //Debug.Log("Stamina + " + _stamina);
            }

            while (_stamina > 0 && isUsingStamina && 
                   (_wasTouchingWall && Input.GetAxis("Vertical") != 0 || 
                    _wasTouchingCeiling && Input.GetAxis("Horizontal") != 0))
            {
                yield return new WaitForSeconds(staminaDegradationTime);
                _stamina -= staminaDegradationValue;
                //Debug.Log("Stamina - " + _stamina);
            }
            _isCoroutineRunning = false;
        }
        
        
        #endregion

        #region Unity
        #if DEBUG
        [Header("Debug:")]
        public bool debugMode;
        public bool printDebugMessages;
        public DebugType debugMessageType;
        public bool enableCheats;

        private DebugUtility.DebugUtility _debug;
        #endif

        private void Start()
        {
            #if DEBUG
            if (debugMode)
            {
                _debug = new DebugUtility.DebugUtility(this, debugMessageType);
                if (enableCheats)
                {
                    _debug.UseCheats();
                }
            }
            #endif
            
            // Ustawia grawitację świata na taką jaką ma gracz.
            BaseWorld.World.GetGravityScale() = Rigidbody.gravityScale;
            _stamina = maxStamina;
        }

        private void Update()
        {
            #if DEBUG
            if (debugMode && printDebugMessages)
                _debug.PrintDebug(true);
            #endif

            if (Input.GetKeyUp(climbKey))
                _wasClimbKeyPressed = !_wasClimbKeyPressed;
            
            InteractWithFloorType();
            Move();
            //Debug.Log(jumpForce);
            bool isTouchingStuff = !IsGrounded && (IsTouchingWall || IsTouchingCeiling || IsAttachedToRope);
            if (!_isCoroutineRunning && ((_stamina  < maxStamina && IsGrounded) || isTouchingStuff))
                StartCoroutine(Stamina(isTouchingStuff));
            Jump();
            Stomp(isTouchingStuff);
            Swing();
            DetachRopeTimer();
        }

        #endregion
    }
}
