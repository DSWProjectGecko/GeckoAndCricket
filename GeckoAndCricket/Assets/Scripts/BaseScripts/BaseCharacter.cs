#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace BaseScripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BaseCharacter : MonoBehaviour
    {
#pragma warning disable 8618
        public Transform body;

        [Header("Character variables:")] 
        public float health = 100f;
        public float movementSpeed = 5f;
        public float wallSlidingSpeed = 4f;
        public float jumpForce = 12f;
        public float stompForce = 2f;
        public float pushForce = 7f;

        // Character private movement fields:
        private float _tempMovementSpeed;

        [Header("Character colliders:")] 
        public Transform groundCollider;
        public float groundCheckSize = 0.2f;
        public Transform wallCollider;
        public float wallCheckSize = 0.6f;
        public Transform ceilingCollider;
        public float ceilingCheckSize = 0.2f;

        // Character protected fields:
        protected Rigidbody2D characterRigidbody;
        protected HingeJoint2D characterHingeJoint;

        // Character protected collision fields
        // TODO: Not sure about using ints instead of enums
        protected int floorType;
        protected int wallType;

        [Header("Character flags:")] 
        public bool isFacingRight = true;
        public bool isFlippedVertically;
        public bool isFlippedHorizontally;

        // Character protected flags:
        protected bool isGrounded = true;
        protected bool isTouchingWall;
        protected bool isTouchingCeiling;
        protected bool isAttachedToRope;
        
        // Character private flags:
        private bool _wasTouchingDifferentFloor;
        private bool _wasTouchingDifferentWall;
        
        // Available collision functions:
        private readonly List<Action> _collisionFunctions = new List<Action>();

        [Header("Debug BaseCharacter:")] 
        public Vector3 resetPosition;
#pragma warning restore 8618

        #region Getters and Setters
        public bool IsGroundedFlag { get => isGrounded; set => isGrounded = value; }
        public bool IsTouchingWallFlag { get => isTouchingWall; set => isTouchingWall = value; }
        public bool IsTouchingCeilingFlag { get => isTouchingCeiling; set => isTouchingCeiling = value; }

        #endregion

        #region Sprite Flipping
        /// <summary>
        /// Flips character sprite horizontally.
        /// </summary>
        /// <returns>Returns true if the sprite is flipped.</returns>
        public bool FlipHorizontally()
        {
            Vector3 flip = body.localScale;
            flip.x *= -1;
            body.localScale = flip;

            return isFlippedHorizontally = !isFlippedHorizontally;
        }
        
        /// <summary>
        /// Flips character sprite vertically.
        /// </summary>
        /// <returns>Returns true if the sprite is flipped.</returns>
        public bool FlipVertically()
        {
            Vector3 flip = body.localScale;
            flip.y *= -1;
            body.localScale = flip;
            
            return isFlippedVertically = !isFlippedVertically;
        }
        #endregion

        #region Sprite Rotation
        /// <summary>
        /// Sets rotation of the sprite in x axis.
        /// </summary>
        /// <param name="angle">angle to set.</param>
        public void SetRotationX(float angle)
        {
            SetRotation(angle, 0f, 0f);
        }
        
        /// <summary>
        /// Sets rotation of the sprite in y axis.
        /// </summary>
        /// <param name="angle">angle to set.</param>
        public void SetRotationY(float angle)
        {
            SetRotation(0f, angle, 0f);
        }
        
        /// <summary>
        /// Sets rotation of the sprite in z axis.
        /// </summary>
        /// <param name="angle">angle to set.</param>
        public void SetRotationZ(float angle)
        {
            SetRotation(0f, 0f, angle);
        }

        private void SetRotation(float xAngle, float yAngle, float zAngle)
        {
            body.localEulerAngles = new Vector3(xAngle, yAngle, zAngle);
        }
        #endregion
        
        #region Collisions
        private void CheckWallCollision()
        {
            foreach (int? type in BaseWorld.wallType.GetSurfaceTypes())
            {
                // TODO: We shouldn't check that everytime.
                if (type == null)
                    continue;

                if (!Physics2D.OverlapCircle(wallCollider.position, wallCheckSize, (int) type)) 
                    continue;
                
                wallType = (int) type;
                isTouchingWall = true;
                return;
            }
            
            isTouchingWall = false;
        }

        private void CheckFloorCollision()
        {
            foreach (int? type in BaseWorld.floorType.GetSurfaceTypes())
            {
                // TODO: We shouldn't check that everytime.
                if (type == null)
                    continue;
                if (!Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, (int) type))
                    continue;
                
                floorType = (int) type;
                isGrounded = true;
                return;
            }

            isGrounded = false;
        }

        private void CheckCeilingCollision()
        {
            isTouchingCeiling = Physics2D.OverlapCircle(ceilingCollider.position, ceilingCheckSize, BaseWorld.world.ceilingLayer);
        }
        
        public int InteractWithWallType()
        {
            if (wallType == BaseWorld.wallType.Lava)
            {
                characterRigidbody.transform.localPosition = resetPosition;
                _wasTouchingDifferentWall = true;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (wallType == BaseWorld.wallType.Honey && movementSpeed != BaseWorld.world.honeySpeed) 
            {
                characterRigidbody.gravityScale = 0f;
                movementSpeed = BaseWorld.world.honeySpeed;
                _wasTouchingDifferentWall = true;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (wallType == BaseWorld.wallType.Ice && characterRigidbody.gravityScale != 0f)
            {
                characterRigidbody.gravityScale = BaseWorld.world.GetGravityScale();
                _wasTouchingDifferentWall = true;
            }
            else if (wallType == BaseWorld.wallType.Normal)
            {
                _wasTouchingDifferentWall = false;
                movementSpeed = _tempMovementSpeed;
                characterRigidbody.gravityScale = 0f;
            }
            
            return wallType;
        }
        
        public int InteractWithFloorType()
        {
            if (floorType == BaseWorld.floorType.Lava)
            {
                characterRigidbody.transform.localPosition = resetPosition;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (floorType == BaseWorld.floorType.Honey && movementSpeed != BaseWorld.world.honeySpeed) 
            {
                _wasTouchingDifferentFloor = true;
                movementSpeed = BaseWorld.world.honeySpeed;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (floorType == BaseWorld.floorType.Ice && movementSpeed != BaseWorld.world.iceSpeed)
            {
                _wasTouchingDifferentFloor = true;
                movementSpeed = BaseWorld.world.iceSpeed;
            }
            else if ((_wasTouchingDifferentFloor || _wasTouchingDifferentWall) && floorType == BaseWorld.floorType.Normal)
            {
                _wasTouchingDifferentFloor = false;
                _wasTouchingDifferentWall = false;
                movementSpeed = _tempMovementSpeed;
            }

            return floorType;
        }
        
        /// <summary>
        /// Checks if object is touching a wall and a floor and a ceiling and sets appropriate flags.
        /// </summary>
        public void CheckCollision()
        {
            foreach (Action? action in _collisionFunctions)
                action();
        }
        #endregion
        
        #region Movement
        /// <summary>
        /// Moves character.
        /// </summary>
        /// <param name="x">Argument takes a reference to float x value.</param>
        /// <param name="y">Argument takes a reference to float y value.</param>
        public void Move(ref float x, ref float y)
        {
            characterRigidbody.velocity = new Vector2(x, y);
            if ((x > 0 && !isFacingRight) || (x < 0 && isFacingRight))
                isFacingRight = !FlipHorizontally();
            if ((isTouchingCeiling && !isGrounded && !isFlippedVertically)||(!isTouchingCeiling && isFlippedVertically))
                FlipVertically();
        }

        /// <summary>
        /// Moves character by adding x as force.
        /// </summary>
        /// <param name="x">Argument takes a reference to float x value.</param>
        /// <param name="y">Argument takes a reference to float y value.</param>
        /// <param name="direction">Argument takes a reference to float direction value</param>
        public void Move(ref float x, ref float y, ref float direction)
        {
            characterRigidbody.AddForce(x * Vector2.right);
            characterRigidbody.velocity = new Vector2(characterRigidbody.velocity.x, y);
            if ((direction > 0.01f && !isFacingRight && isFlippedHorizontally) || (direction < 0f && isFacingRight && !isFlippedHorizontally))
                isFacingRight = !FlipHorizontally();
            if ((isTouchingCeiling && !isGrounded && !isFlippedVertically)||(!isTouchingCeiling && isFlippedVertically))
                FlipVertically();
        }
        
        public void Jump()
        {
            Vector2 jump = new Vector2(characterRigidbody.velocity.x, jumpForce);
            characterRigidbody.velocity = jump;
        }
        
        public void Stomp() 
        {
            Vector2 stomp = new Vector2(0f, stompForce);
            characterRigidbody.velocity -= stomp;
        }

        #endregion

        #region JSON
        private string? _filePath;

        public void SetPath(string? fileName)
        {
            switch (fileName)
            {
                case null:
                    _filePath = null;
                    return;
                case "":
                    throw new ArgumentException();
                default:
                    _filePath = Directory.GetCurrentDirectory() + fileName + @".json";
                    break;
            }
        }

        public void Serialize()
        {
            _filePath ??= Directory.GetCurrentDirectory() + GetType() + @".json";

            var tempObject = new
            {
                health, movementSpeed, jumpForce
            };

            string yourBoiJson = JsonConvert.SerializeObject(tempObject);
            File.WriteAllText(_filePath, yourBoiJson);
            Debug.Log("Object serialized to: " + _filePath);
        }
        
        //TODO: finish deserialization in the future.
        public void Deserialize()
        {
            _filePath ??= Directory.GetCurrentDirectory() + GetType() + @".json";

            if (!File.Exists(_filePath))
            {
                Debug.Log(_filePath + " does not exist, object could not be deserialized.");
                return;
            }
            
            // ReSharper disable once UnusedVariable
            var template = new
            {
                health, movementSpeed, jumpForce
            };
            
            Debug.Log("Object deserialized from: " + _filePath);
        }
        #endregion
        
        #region Debug:
        #if DEBUG
        public unsafe bool* GetIsGroundedPointer()
        {
            fixed (bool* ptr = &isGrounded)
                return ptr;
        }

        public unsafe bool* GetIsTouchingWallPointer()
        {
            fixed (bool* ptr = &isTouchingWall)
                return ptr;
        }

        public unsafe bool* GetIsTouchingCeilingPointer()
        {
            fixed (bool* ptr = &isTouchingCeiling)
                return ptr;
        }

        public unsafe bool* GetIsAttachedToRopePointer()
        {
            fixed (bool* ptr = &isAttachedToRope)
                return ptr;
        }

        public unsafe float* GetMovementSpeedPointer()
        {
           fixed (float* ptr = &movementSpeed)
                return ptr;
        }
        #endif
        #endregion

        #region Unity

        protected void Awake()
        {
            characterRigidbody = GetComponent<Rigidbody2D>();
            characterHingeJoint = GetComponent<HingeJoint2D>();
            _tempMovementSpeed = movementSpeed;
            
            if (groundCollider)
                _collisionFunctions.Add(CheckFloorCollision);
            if (wallCollider)
                _collisionFunctions.Add(CheckWallCollision);
            if (ceilingCollider)
                _collisionFunctions.Add(CheckCeilingCollision);
        }

        protected void FixedUpdate()
        {
            CheckCollision();
        }
        #endregion
    }
}
