using UnityEngine;

namespace PlayerScripts
{
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        #pragma warning disable 169
        #pragma warning disable 414
        #pragma warning disable 649
        [Header("Player colliders:")]
        // Public:
        public Transform groundCollider;
        public float groundCheckSize = 0.2f;
        public Transform wallCollider;
        public float wallCheckSize = 0.6f;
        public Transform ceilingCollider;
        public float ceilingCheckSize = 0.2f;
        
        
        // Player objects
        private Player _player;
        private Vector2 _input;
        private Rigidbody2D _rigidbody;

        // Movement variables:
        private int _jumpCount = 0;

        // Movement flags:
        private bool _isGrounded = true;
        private bool _isTouchingWall;
        private bool _isTouchingCeiling;
        private bool _isFacingRight = true;
        private bool _isFlippedVertically = false;

        /*private void Move(out float xSpeed, out float ySpeed)
        {
            //xSpeed = _input.x * _player.speed;
           // ySpeed = _isTouchingWall ? _input.y * _player.speed : _rigidbody.velocity.y;

            //int direction = _isFacingRight ? 1 : -1;
            //_isFacingRight = _player.FlipHorizontally(direction);
        }*/
        
        /*private void Awake()
        {
            _player = GetComponent<Player>();
            _input = _player.GetPlayerInputObject();
            _rigidbody = _player.GetRigidbodyObject();
        }*/

        private void Start()
        {
            _player = GetComponent<Player>();
            //_input = _player.GetPlayerInputObject();
           //_rigidbody = _player.GetRigidbodyObject();
        }

        private void Update()
        {
            _input.x = Input.GetAxis("Horizontal");
            _input.y = Input.GetAxis("Vertical");

            //Move(out float xSpeed, out float ySpeed);
            //_rigidbody.velocity = new Vector2(xSpeed, ySpeed);
            //Jump();
        }
        
        private void FixedUpdate()
        {
            //World world = _player.GetWorldObject();
            //_isGrounded = Physics2D.OverlapCircle(groundCollider.position, groundCheckSize, world.groundLayer);
            //_isTouchingWall = Physics2D.OverlapCircle(wallCollider.position, wallCheckSize, world.wallLayer);
            //_isTouchingCeiling = Physics2D.OverlapCircle(ceilingCollider.position, ceilingCheckSize, world.ceilingLayer);
            
            if (_isTouchingCeiling)
            {
                _jumpCount = 0;
                //_rigidbody.gravityScale = _input.y < 0 ? _player.GetWorldObject().GravityScale : 0f;
            }
            else
            {
                //_rigidbody.gravityScale = _player.GetWorldObject().GravityScale;
            }
            
            //_isFlippedVertically = _player.FlipVertically(_isFlippedVertically ? -1 : 1);
           // if ((!_isTouchingWall && _player.Rotation == 0) || (_isTouchingWall && (_player.Rotation == 90f || _player.Rotation == -90f)))
               // return;
            
            //_player.Rotate(_isTouchingWall ? 45f : 0f);
        }
        #pragma warning restore 169
        #pragma warning restore 414
        #pragma warning restore 649
    }
}
