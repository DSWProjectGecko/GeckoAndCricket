using System.Collections.Generic;
using UnityEngine;

namespace EnemiesScripts
{
    public class BatAI : MonoBehaviour
    {
        [SerializeField] private GameObject path;
       
        // public variables:
        public const float CheckTimer = 3f;
        public float currentCheckTimer = CheckTimer;
        public float speedTime = 10f;
        
        // public flags:
        public bool detectPlayer;
        public bool isImmune = true;
        
        // private variables:
        private int _currentPosition;
        
        private readonly List<Vector3> _batPathPositions = new List<Vector3>();
        private PolygonCollider2D _batVisionCollider;

        #region BatAI
        private void CheckTerrain() 
        {
            if (isImmune)
            {
                if (currentCheckTimer <= 0)
                {
                    _batVisionCollider.enabled = !_batVisionCollider.enabled;
                    currentCheckTimer = CheckTimer;
                }
                else
                {
                    currentCheckTimer -= Time.deltaTime;
                }
            }
            else 
            {
                currentCheckTimer = CheckTimer;
            }
        }
        
        private void Attack()
        {
            if (!detectPlayer) return;
            
            _batVisionCollider.enabled = false;
            isImmune = false;
            
            speedTime = _currentPosition == 1 ? 20f : 10f;
            
            if (_currentPosition <= 1 && transform.position != _batPathPositions[_currentPosition + 1]) 
            {
                transform.position = Vector3.MoveTowards(transform.position, _batPathPositions[_currentPosition + 1], speedTime * Time.deltaTime);
                if (transform.position == _batPathPositions[_currentPosition + 1]) 
                {
                    _currentPosition += 1;   
                }
            }
            else 
            {
                if (transform.position != _batPathPositions[0])
                {
                    transform.position = Vector3.MoveTowards(transform.position, _batPathPositions[0], speedTime * Time.deltaTime);
                }
                else 
                {
                    _currentPosition = 0;
                    detectPlayer = false;
                    isImmune = true;
                }
            }
        }
        #endregion

        #region Unity
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player")) 
            {
                detectPlayer = true;
            }
        }
        
        private void Start()
        {
            _batVisionCollider = gameObject.transform.GetChild(0).GetComponent<PolygonCollider2D>();
            foreach (Transform child in path.transform)
                _batPathPositions.Add(child.position);
            
            _currentPosition = 0;
            transform.position = _batPathPositions[0];

        }
        
        private void Update()
        {
            Attack();
            CheckTerrain();
        }
        #endregion
    }
}
