#nullable enable
using System;
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
        public int maxJumps = 1;
        
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
        protected Rigidbody2D Rigidbody;
        protected HingeJoint2D HingeJoint;

        // Character private collision fields
        private int _floorType;
        private int _wallType;

        [Header("Character flags:")] 
        public bool isFacingRight = true;
        public bool isFlippedVertically;
        public bool isFlippedHorizontally;

        // Character protected flags:
        protected bool IsGrounded = true;
        protected bool IsTouchingWall;
        protected bool IsTouchingCeiling;
        protected bool IsAttachedToRope;
        
        // Character private flags:
        private bool _wasTouchingDifferentFloor;
        private bool _wasTouchingDifferentWall;

        [Header("Debug BaseCharacter:")] 
        public Vector3 resetPosition;
#pragma warning restore 8618

        #region Getters and Setters
        public bool IsGroundedFlag { get => IsGrounded; set => IsGrounded = value; }
        public bool IsTouchingWallFlag { get => IsTouchingWall; set => IsTouchingWall = value; }
        public bool IsTouchingCeilingFlag { get => IsTouchingCeiling; set => IsTouchingCeiling = value; }

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
            //isFacingRight = !isFacingRight;
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
        
        /// <summary>
        /// Rotates character sprite in x axis.
        /// </summary>
        /// <param name="angle">angle has to be in range from -360 to 360</param>
        /// <returns>Returns new x angle.</returns>
        public float RotateX(float angle)
        {
            if (angle < -360f || angle > 360f)
                throw new ArgumentOutOfRangeException();
            
            return Rotate(angle, 0f, 0f).x;
        }
        
        /// <summary>
        /// Rotates character sprite in y axis.
        /// </summary>
        /// <param name="angle">angle has to be in range from -360 to 360</param>
        /// <returns>Returns new y angle.</returns>
        public float RotateY(float angle)
        {
            if (angle < -360f || angle > 360f)
                throw new ArgumentOutOfRangeException();
            
            return Rotate(0f, angle, 0f).y;
        }
        
        /// <summary>
        /// Rotates character sprite in z axis.
        /// </summary>
        /// <param name="angle">angle has to be in range from -360 to 360</param>
        /// <returns>Returns new z angle.</returns>
        public float RotateZ(float angle)
        {
            if (angle < -360f || angle > 360f)
                throw new ArgumentOutOfRangeException();
            
            return Rotate(0f, 0f, angle).z;
        }

        private Vector3 Rotate(float xAngle, float yAngle, float zAngle)
        {
            Vector3 currentRotation = body.localEulerAngles;

            if (currentRotation.x > 359f || currentRotation.x < -359f)
            {
                currentRotation.x = 0f;
            }
            
            if (currentRotation.y > 359f || currentRotation.y < -359f)
            {
                currentRotation.y = 0f;
            }
            
            if (currentRotation.z > 359f || currentRotation.z < -359f)
            {
                currentRotation.z = 0f;
            }

            Vector3 newRotation = new Vector3(currentRotation.x + xAngle, currentRotation.y + yAngle,
                                              currentRotation.z + zAngle);

            body.localEulerAngles = newRotation;
            
            return newRotation;
        }
        #endregion
        
        #region Collisions
        private bool CheckWallCollision()
        {
            foreach (int? type in BaseWorld.WallType.GetSurfaceTypes())
            {
                if (type == null)
                    continue;

                if (!Physics2D.OverlapCircle(wallCollider.position, wallCheckSize, (int) type)) 
                    continue;
                
                _wallType = (int) type;
                return true;
            }
            
            return false;
        }

        private bool CheckFloorCollision()
        {
            foreach (int? type in BaseWorld.FloorType.GetSurfaceTypes())
            {
                if (type == null)
                    continue;
                if (!Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, (int) type))
                    continue;
                
                _floorType = (int) type;
                return true;
            }

            return false;
        }
        
        protected int InteractWithWallType()
        {
            if (_wallType == BaseWorld.WallType.Lava)
            {
                Rigidbody.transform.localPosition = resetPosition;
                _wasTouchingDifferentWall = true;
                //Debug.Log("Lava");
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (_wallType == BaseWorld.WallType.Honey && movementSpeed != BaseWorld.World.honeySpeed) 
            {
                Rigidbody.gravityScale = 0f;
                movementSpeed = BaseWorld.World.honeySpeed;
                _wasTouchingDifferentWall = true;
                //Debug.Log("Honey");
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (_wallType == BaseWorld.WallType.Ice && Rigidbody.gravityScale != 0f)
            {
                Rigidbody.gravityScale = BaseWorld.World.GetGravityScale();
                _wasTouchingDifferentWall = true;
                //Debug.Log("Ice");
            }
            else if (_wallType == BaseWorld.WallType.Normal)
            {
                _wasTouchingDifferentWall = false;
                movementSpeed = _tempMovementSpeed;
                Rigidbody.gravityScale = 0f;
                //Debug.Log("Normal");
            }
            
            return _wallType;
        }
        
        protected int InteractWithFloorType()
        {
            if (_floorType == BaseWorld.FloorType.Lava)
            {
                Rigidbody.transform.localPosition = resetPosition;
                //Debug.Log("Lava");
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (_floorType == BaseWorld.FloorType.Honey && movementSpeed != BaseWorld.World.honeySpeed) 
            {
                _wasTouchingDifferentFloor = true;
                movementSpeed = BaseWorld.World.honeySpeed;
                //Debug.Log("Honey");
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (_floorType == BaseWorld.FloorType.Ice && movementSpeed != BaseWorld.World.iceSpeed)
            {
                _wasTouchingDifferentFloor = true;
                movementSpeed = BaseWorld.World.iceSpeed;
                //Debug.Log("Ice");
            }
            else if ((_wasTouchingDifferentFloor || _wasTouchingDifferentWall) && _floorType == BaseWorld.FloorType.Normal)
            {
                _wasTouchingDifferentFloor = false;
                _wasTouchingDifferentWall = false;
                movementSpeed = _tempMovementSpeed;
                //Debug.Log("Normal");
            }

            return _floorType;
        }
        public int GetFloorType() {
            return _floorType;
        }
        #endregion
        
        #region Movement
        /// <summary>
        /// Moves character
        /// </summary>
        /// <param name="direction">Argument takes a reference to Vector2 object.</param>
        protected void Move(ref Vector2 direction)
        {
            if (!IsAttachedToRope)
            {
                Rigidbody.velocity = direction;
            }
            if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
                isFacingRight = !FlipHorizontally();
            if ((IsTouchingCeiling && !IsGrounded && !isFlippedVertically)||(!IsTouchingCeiling && isFlippedVertically))
                FlipVertically();
        }

        /// <summary>
        /// Moves character.
        /// </summary>
        /// <param name="x">Argument takes a reference to float x value.</param>
        /// <param name="y">Argument takes a reference to float y value.</param>
        protected void Move(ref float x, ref float y)
        {
            Rigidbody.velocity = new Vector2(x, y);
            if ((x > 0 && !isFacingRight) || (x < 0 && isFacingRight))
                isFacingRight = !FlipHorizontally();
            if ((IsTouchingCeiling && !IsGrounded && !isFlippedVertically)||(!IsTouchingCeiling && isFlippedVertically))
                FlipVertically();
        }

        /// <summary>
        /// Moves character by adding x as force.
        /// </summary>
        /// <param name="x">Argument takes a reference to float x value.</param>
        /// <param name="y">Argument takes a reference to float y value.</param>
        /// <param name="direction">Argument takes a reference to float direction value</param>
        protected void Move(ref float x, ref float y, float direction)
        {
            Rigidbody.AddForce(x * Vector2.right);
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, y);
            if ((direction > 0.01f && !isFacingRight && isFlippedHorizontally) || (direction < 0f && isFacingRight && !isFlippedHorizontally))
                isFacingRight = !FlipHorizontally();
            if ((IsTouchingCeiling && !IsGrounded && !isFlippedVertically)||(!IsTouchingCeiling && isFlippedVertically))
                FlipVertically();
        }
        
        protected void Jump()
        {
            Vector2 jump = new Vector2(Rigidbody.velocity.x, jumpForce);
            Rigidbody.velocity = jump;
        }
        
        protected void Stomp() {
            Vector2 stomp = new Vector2(0f, stompForce);
            Rigidbody.velocity -= stomp;
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
            fixed (bool* ptr = &IsGrounded)
                return ptr;
        }

        public unsafe bool* GetIsTouchingWallPointer()
        {
            fixed (bool* ptr = &IsTouchingWall)
                return ptr;
        }

        public unsafe bool* GetIsTouchingCeilingPointer()
        {
            fixed (bool* ptr = &IsTouchingCeiling)
                return ptr;
        }

        public unsafe bool* GetIsAttachedToRopePointer()
        {
            fixed (bool* ptr = &IsAttachedToRope)
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
            Rigidbody = GetComponent<Rigidbody2D>();
            HingeJoint = GetComponent<HingeJoint2D>();
            _tempMovementSpeed = movementSpeed;
        }

        protected void FixedUpdate()
        {
            if (groundCollider != null)
                IsGrounded = CheckFloorCollision();
            if (wallCollider != null)
                IsTouchingWall = CheckWallCollision();
            if (ceilingCollider != null)
                IsTouchingCeiling = Physics2D.OverlapCircle(ceilingCollider.position, ceilingCheckSize, BaseWorld.World.ceilingLayer);
        }
        #endregion
    }
}
