using System.Collections;
using BaseScripts;
using EnemiesScripts;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG 
using DebugUtility; 
#endif

namespace PlayerScripts
{
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
        
        [Header("Roll variables:")]
        public bool isRolling;
        
        [Header("Grapple variables:")]
        public bool isGrappled;
        
        public Canvas grappleCanvas;
        public Slider grapple;
        
        // private grapple variables:
        private const float GrappleBarTimer=0.01f;
        private float _currentGrappleBarTimer=GrappleBarTimer;
        
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

        
#pragma warning disable CS0649
        // These aren't initialized
        private GameObject _disregard;
        // ReSharper disable InconsistentNaming
        private GameObject disregard;
        // ReSharper restore InconsistentNaming
#pragma warning restore CS0649

        #region Getters

        #endregion

        #region Movement
        private void Move()
        {
            Vector2 velocity = characterRigidbody.velocity;
            float xInput = Input.GetAxis("Horizontal");
            float x = xInput * movementSpeed; // 5f
            float y = velocity.y;

            if (isTouchingCeiling && !isGrounded && !Input.GetKeyUp(climbKey) && _stamina > 0f)
            {
                characterRigidbody.gravityScale = 0f;
                _wasTouchingCeiling = true;
            }
            else if (_wasTouchingCeiling)
            {
                _wasClimbKeyPressed = false;
                _wasTouchingCeiling = false;
                characterRigidbody.gravityScale = BaseWorld.world.GetGravityScale();
            }

            if (isTouchingWall && !_isWallJumping && _wasClimbKeyPressed)
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
                    characterRigidbody.gravityScale = BaseWorld.world.GetGravityScale();
                    SetRotationZ(-90f);
                    y = Mathf.Clamp(characterRigidbody.velocity.y, -wallSlidingSpeed, float.MaxValue);
                    _wasClimbKeyPressed = false;
                }
                _wasTouchingWall = true;
            }
            else if (_wasTouchingWall && !_isWallJumping)
            {
                characterRigidbody.gravityScale = BaseWorld.world.GetGravityScale();
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
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (Mathf.Abs(velocity.x) > 0f && Mathf.Sign(x) != Mathf.Sign(velocity.x))
                velocityForce = turnForce;
            else
                velocityForce = xAcceleration;

            x = Mathf.Pow(Mathf.Abs(xDiff) * xAcceleration, velocityForce) * Mathf.Sign(x);
            x = Mathf.Lerp(velocity.x, x, 1);
            
            base.Move(ref x, ref y, ref xInput);
            
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

            direction *= characterRigidbody.velocity.x;
            characterRigidbody.gravityScale = BaseWorld.world.GetGravityScale();
            characterRigidbody.AddForce(new Vector2(wallJumpForce * direction , jumpForce), ForceMode2D.Impulse);
        }

        private void SetWallJumpToFalse()
        {
            _isWallJumping = false;
        }
		
        private new void Jump()
        {
            if (!Input.GetKeyDown(KeyCode.Space) || _stamina < staminaToJump) return;

            _stamina -= staminaJumpCost;
            if (isGrounded || isAttachedToRope) 
            {
                base.Jump();
                doubleJump = true;
            }
            else if (isTouchingWall)
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

        private void Roll() 
        {
            if ((characterRigidbody.velocity.x > 0 || characterRigidbody.velocity.x < 0) && isGrounded && Input.GetKeyDown("s"))
            {
                isRolling = true;
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
            else if(!isRolling && gameObject.GetComponent<BoxCollider2D>().enabled == false)
            {
                    gameObject.GetComponent<BoxCollider2D>().enabled = true;
            }
        }
		
        private void Stomp(bool isTouchingStuff) 
        {
            if (!Input.GetKeyDown("s") && isTouchingStuff)
                Stomp();
        }
		
        private void AttachToRope(Rigidbody2D ropeSeg) 
        {
            Debug.Log(ropeSeg);
            GameObject ropeGameObject;
            (ropeGameObject = ropeSeg.gameObject).GetComponent<RopeSegment>().isPlayerAttached = true;
            characterHingeJoint.connectedBody = ropeSeg;
            characterHingeJoint.enabled = true;
            isAttachedToRope = true;
            attachedTo = ropeGameObject.transform.parent;
        }
		
        private void DetachFromRope() 
        {
            characterHingeJoint.connectedBody.GetComponent<RopeSegment>().isPlayerAttached = false;
            characterHingeJoint.enabled = false;
            isAttachedToRope = false;
            characterHingeJoint.connectedBody = null;
        }
		
        private void SlideOnRope(int direction) 
        {
            RopeSegment actualRopeSegment = characterHingeJoint.connectedBody.gameObject.GetComponent<RopeSegment>();
            GameObject newRopeSegment = null;
            if (direction > 0)
            {
                if (actualRopeSegment.above != null && actualRopeSegment.above.gameObject.GetComponent<RopeSegment>() != null) 
                {
                    newRopeSegment = actualRopeSegment.above;
                }
            }
            else if (actualRopeSegment.below != null)
            {
                newRopeSegment = actualRopeSegment.below;
            }

            if (newRopeSegment == null) return;
            
            transform.position = newRopeSegment.transform.position;
            actualRopeSegment.isPlayerAttached = false;
            newRopeSegment.GetComponent<RopeSegment>().isPlayerAttached = true;
            characterHingeJoint.connectedBody = newRopeSegment.GetComponent<Rigidbody2D>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isAttachedToRope || !collision.gameObject.CompareTag("Rope") ||
                attachedTo == collision.gameObject.transform.parent ||
                (_disregard != null && collision.gameObject.transform.parent.gameObject == _disregard) ||
                isAttachedToRope || !collision.gameObject.CompareTag("Rope") || !(detachTimer <= 0f) ||
                attachedTo == collision.gameObject.transform.parent) return;
            
            if (disregard == null || collision.gameObject.transform.parent.gameObject != disregard)
            {
                AttachToRope(collision.gameObject.GetComponent<Rigidbody2D>());
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (isRolling && collision.gameObject.CompareTag("Roll"))
                isRolling = false;
        }

        private void Swing() 
        {
            if (Input.GetKey("a") && isAttachedToRope)
                characterRigidbody.AddRelativeForce(new Vector2(-1, 0) * pushForce);

            if (Input.GetKey("d") && isAttachedToRope)
                characterRigidbody.AddRelativeForce(new Vector2(1, 0) * pushForce);

            if (Input.GetKeyDown("w") && isAttachedToRope)
                SlideOnRope(1);
            
            if (Input.GetKeyDown("s") && isAttachedToRope)
                SlideOnRope(-1);

            if (!Input.GetKeyDown("space") || !isAttachedToRope) return;
            
            detachTimer = 1f;
            DetachFromRope();
        }
		
        private void DetachRopeTimer() 
        {
            if (!isAttachedToRope && detachTimer>0f)
                detachTimer -= Time.deltaTime;
            else if(detachTimer <= 0f && attachedTo != null)
                attachedTo = null;
            
        }

        #endregion
        
        #region Stamina

        private bool _isCoroutineRunning;

        private IEnumerator Stamina(bool isUsingStamina)
        {
            _isCoroutineRunning = true;

            while (_stamina < maxStamina && isGrounded)
            {
                yield return new WaitForSeconds(staminaRegenerationTime);
                _stamina += staminaRegenerationValue;
            }

            while (_stamina > 0 && isUsingStamina && 
                   (_wasTouchingWall && Input.GetAxis("Vertical") != 0 || 
                    _wasTouchingCeiling && Input.GetAxis("Horizontal") != 0))
            {
                yield return new WaitForSeconds(staminaDegradationTime);
                _stamina -= staminaDegradationValue;
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

        #region Actions

        private void GrappleEscape() 
        {
            if (grappleCanvas.enabled == false) {
                grappleCanvas.enabled = true;
            }
            if (Input.GetKeyDown(KeyCode.Z)) {
                grapple.value += 2f;
                
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (grapple.value == grapple.maxValue)
                {
                    // TODO: I think this should be partially moved to the frog script GetComponent is expensive and then we can remove Move method in the frog script. - Hubert
                    Transform enemy = gameObject.transform.Find("GrapplePosition").transform.Find("Frog");
                    enemy.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                    enemy.gameObject.GetComponent<FrogAI>().movementSpeed *=-1;
                    enemy.gameObject.GetComponent<FrogAI>().Move(enemy.gameObject.GetComponent<FrogAI>().movementSpeed * 1.5f, enemy.gameObject.GetComponent<Rigidbody2D>().velocity.y);
                    enemy.gameObject.GetComponent<FrogAI>().grapplingPlayer = false;
                    enemy.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
                    enemy.parent=null;
                    grapple.value = 0f;
                    isGrappled = false;
                    grappleCanvas.enabled = false;
                }
            }

            if (!(grapple.value > 0f)) return;
            
            _currentGrappleBarTimer -= Time.deltaTime;
            
            if (!(_currentGrappleBarTimer <= 0f)) return;
                
            _currentGrappleBarTimer = GrappleBarTimer;
            grapple.value -= 0.2f;

        }
        #endregion

        #region Unity
        private new void Awake()
        {
            base.Awake();
            BaseWorld.player = gameObject;
            isGrappled = false;
        }
        private void Start()
        {
            #if DEBUG
            if (debugMode && debugMessageType != DebugType.Movement)
                InitializeDebug();
            #endif
            
            // Ustawia grawitację świata na taką jaką ma gracz.
            BaseWorld.world.GetGravityScale() = characterRigidbody.gravityScale;
            _stamina = maxStamina;
            grapple.maxValue = 10f;
            grapple.value = 0f;
            grappleCanvas.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyUp(climbKey))
                _wasClimbKeyPressed = !_wasClimbKeyPressed;
            
            InteractWithFloorType();
            
            //Debug.Log(jumpForce);
            bool isTouchingStuff = !isGrounded && (isTouchingWall || isTouchingCeiling || isAttachedToRope);
            if (!_isCoroutineRunning && ((_stamina  < maxStamina && isGrounded) || isTouchingStuff))
                StartCoroutine(Stamina(isTouchingStuff));
            
            if (!isGrappled)
            {
                Jump();
                Stomp(isTouchingStuff);
                Swing();
                Roll();
                DetachRopeTimer();
            }
            else
            {
                GrappleEscape();
            }
            
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
            if (!isGrappled)
                Move();
        }
        #endregion
    }
}
