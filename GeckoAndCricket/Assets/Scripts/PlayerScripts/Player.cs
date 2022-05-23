using System.Collections;
using BaseScripts;
using UnityEngine;
using UnityEngine.UI;
#if DEBUG 
using DebugUtility; 
#endif

namespace PlayerScripts
{
    //[RequireComponent(typeof(BaseWorld))]
    public class Player : BaseCharacter
    {
        [Header("Player movement variables:")]
        public float wallJumpForce = 13f;
        public float wallJumpTime = 0.05f;
        public float detachTimer = 1f;

        public float accelerationValue = 1.5f;

        public float decelerationValue = 0.5f;
        
        public float stoppingForce = 1f;
        public float turnForce = 1f;
        
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
        // Roll
        public bool isRolling;

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

        #region Getters

        public bool GetIsTouchingCeiling => IsTouchingCeiling;
        public bool GetIsTouchingWall => IsTouchingWall;
        public bool GetIsGrounded => IsGrounded;

        #endregion

        #region Movement
        private void Move()
        {
            Vector2 velocity = Rigidbody.velocity;
            float xInput = Input.GetAxis("Horizontal");
            float x = xInput * movementSpeed; // 5f
            float y = velocity.y;

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
            
            //if (Input.GetAxis("Horizontal") != 0f && velocity.x < )
            float xDiff = x - velocity.x; // min 5f - 0f, max 5f - 20f or -5f - 0f, max -5f + 20f
            float xAcceleration = (Mathf.Abs(x) > 0.01f) ? accelerationValue : decelerationValue;

            if ((velocity.x > x && x > 0.01f) || (velocity.x < x && x < -0.01f))
                xAcceleration = 0f;

            float velocityForce;
            if (Mathf.Abs(x) < 0.01f)
                velocityForce = stoppingForce;
            else if (Mathf.Abs(velocity.x) > 0f && Mathf.Sign(x) != Mathf.Sign(velocity.x))
                velocityForce = turnForce;
            else
                velocityForce = xAcceleration;

            x = Mathf.Pow(Mathf.Abs(xDiff) * xAcceleration, velocityForce) * Mathf.Sign(x);
            x = Mathf.Lerp(velocity.x, x, 1);
            
            base.Move(ref x, ref y, xInput);
            
            #if DEBUG
            if (_debug == null && debugMessageType == DebugType.Movement)
                    InitializeDebug();

            _accelerationValue = xAcceleration;
            _currentSpeedValue= velocity.x;
            _velocityForceValue = velocityForce;
            _xValue = x;
            _speedDifferenceValue = xDiff;
            #endif
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

        private void Roll() {
            if ((Rigidbody.velocity.x > 0 || Rigidbody.velocity.x < 0) && IsGrounded && Input.GetKeyDown("s"))
            {
                isRolling = true;
                this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
            else if(!isRolling && (this.gameObject.GetComponent<BoxCollider2D>().enabled == false))
            {
                    this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
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
		
        private void SlideOnRope(int direction) 
        {
            RopeSegment actualRopeSegment = HingeJoint.connectedBody.gameObject.GetComponent<RopeSegment>();
            GameObject newRopeSegment = null;
            if (direction > 0)
            {
                if (actualRopeSegment.above != null) 
                {
                    if (actualRopeSegment.above.gameObject.GetComponent<RopeSegment>() != null) 
                    {
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
            if (newRopeSegment != null)
            {
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
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (isRolling && collision.gameObject.CompareTag("Roll"))
            {
                isRolling = false;
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

        #region Debug
        #if DEBUG
        [Header("Debug:")]
        public bool debugMode;
        public bool writeToFile;
        public int frequencyOfWriting = 1;
        public bool printDebugMessages;
        public DebugType debugMessageType;
        public bool enableCheats;
        public Text fpsCounter;
        public bool showFPS = true;

        private void InitializeDebug()
        {
            _debug = new DebugUtility.DebugUtility(this, debugMessageType, writeToFile);
            if (enableCheats)
                _debug.UseCheats();

            if (showFPS)
                _debug.ShowFPS(ref fpsCounter);
        }

        
        // Movement:
        private float _accelerationValue;
        public unsafe float* AccelerationValue
        {
            get
            {
                fixed (float* ptr = &_accelerationValue)
                    return ptr;
            }
        }

        private float _speedDifferenceValue;
        public unsafe float* SpeedDifferenceValue
        {
            get
            {
                fixed (float* ptr = &_speedDifferenceValue)
                    return ptr;
            }
        }

        private float _currentSpeedValue;
        public unsafe float* CurrentSpeedValue
        { 
            get
            {
                fixed (float* ptr = &_currentSpeedValue)
                    return ptr;
            }
        }

        private float _velocityForceValue;
        public unsafe float* VelocityForceValue
        {
            get
            {
                fixed (float* ptr = &_velocityForceValue)
                    return ptr;
            }
        }

        private float _xValue;
        public unsafe float* XValue
        {
            get
            {
                fixed (float* ptr = &_xValue)
                    return ptr;
            }
        }

        private DebugUtility.DebugUtility _debug;
        #endif
        #endregion

        #region Unity
        private new void Awake()
        {
            base.Awake();
            BaseWorld.Player = gameObject;
        }
        private void Start()
        {
            #if DEBUG
            if (debugMode && debugMessageType != DebugType.Movement)
                InitializeDebug();
            #endif
            
            // Ustawia grawitację świata na taką jaką ma gracz.
            BaseWorld.World.GetGravityScale() = Rigidbody.gravityScale;
            _stamina = maxStamina;
            needGroundCollider = needCeilingCollider = needWallCollider = true;
        }

        private void Update()
        {
            if (Input.GetKeyUp(climbKey))
                _wasClimbKeyPressed = !_wasClimbKeyPressed;
            
            InteractWithFloorType();
            
            //Debug.Log(jumpForce);
            bool isTouchingStuff = !IsGrounded && (IsTouchingWall || IsTouchingCeiling || IsAttachedToRope);
            if (!_isCoroutineRunning && ((_stamina  < maxStamina && IsGrounded) || isTouchingStuff))
                StartCoroutine(Stamina(isTouchingStuff));
            Jump();
            Stomp(isTouchingStuff);
            Swing();
            Roll();
            DetachRopeTimer();
            
            #if DEBUG
            if (showFPS)
                _debug.PrintFPS();
            if (debugMode && printDebugMessages)
                _debug.PrintDebug(true);
            if (writeToFile)
                _debug.WriteToFile(frequencyOfWriting);
            if (Input.GetKeyDown(KeyCode.F1))
                _debug.SaveFile();
            #endif
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
            Move();
        }
        #endregion
    }
}
