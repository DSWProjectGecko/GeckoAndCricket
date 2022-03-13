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

        [Header("Character colliders:")] 
        public Transform groundCollider;
        public float groundCheckSize = 0.2f;
        public Transform wallCollider;
        public float wallCheckSize = 0.6f;
        public Transform ceilingCollider;
        public float ceilingCheckSize = 0.2f;

        // Character protected fields:
        protected int? SurfaceType;
        protected float TempMovementSpeed;

        protected Rigidbody2D Rigidbody;
        protected HingeJoint2D HingeJoint;

        [Header("Character flags:")] 
        public bool isFacingRight = true;
        public bool isFlippedVertically;
        public bool isFlippedHorizontally;

        // Character protected flags:
        protected bool IsGrounded = true;
        protected bool IsTouchingWall;
        protected bool IsTouchingCeiling = false;
        protected bool IsAttachedToRope = false;

        protected bool WasTouchingDifferentSurface;

        [Header("Debug BaseCharacter:")] 
        public Vector3 resetPosition;
#pragma warning restore 8618

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
        /// Moves character
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
        protected void Jump()
        {
            Vector2 jump = new Vector2(0f, jumpForce);
            Rigidbody.velocity += jump;
        }
        protected void Stomp() {
            Vector2 stomp = new Vector2(0f, stompForce);
            Rigidbody.velocity -= stomp;
        }

        protected void InteractWithSurface()
        {
            if (SurfaceType == BaseWorld.SurfaceType.Lava)
            {
                Rigidbody.transform.localPosition = resetPosition;
                //Debug.Log("Lava");
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (SurfaceType == BaseWorld.SurfaceType.Honey && movementSpeed != BaseWorld.World.honeySpeed) 
            {
                WasTouchingDifferentSurface = true;
                movementSpeed = BaseWorld.World.honeySpeed;
                IsGrounded = true;
               //Debug.Log("Honey");
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (SurfaceType == BaseWorld.SurfaceType.Ice && movementSpeed != BaseWorld.World.iceSpeed)
            {
                WasTouchingDifferentSurface = true;
                movementSpeed = BaseWorld.World.iceSpeed;
                IsGrounded = true;
                Debug.Log("Ice");
            }
            else if (WasTouchingDifferentSurface && SurfaceType == 0)
            {
                WasTouchingDifferentSurface = false;
                movementSpeed = TempMovementSpeed;
                //Debug.Log("Normal");
            }
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
        #endif
        #endregion

        #region Unity
        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            HingeJoint = GetComponent<HingeJoint2D>();
            TempMovementSpeed = movementSpeed;
        }

        private void FixedUpdate()
        {
            //TODO: Implement collision detection.
            IsGrounded = Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, BaseWorld.World.groundLayer);
            IsTouchingWall = Physics2D.OverlapCircle(wallCollider.position, wallCheckSize, BaseWorld.World.wallLayer);
            IsTouchingCeiling = Physics2D.OverlapCircle(ceilingCollider.position, ceilingCheckSize, BaseWorld.World.ceilingLayer);

            foreach (int? type in BaseWorld.SurfaceType.GetSurfaceTypes())
            {
                if (type == null)
                    continue;
                
                if (Physics2D.OverlapCircle(groundCollider.position, ceilingCheckSize, (int) type) 
                    ||Physics2D.OverlapCircle(wallCollider.position, wallCheckSize, (int) type))
                {
                    SurfaceType = (int) type;
                    break;
                }
                SurfaceType = 0;
            }
        }
        #endregion
    }
}
