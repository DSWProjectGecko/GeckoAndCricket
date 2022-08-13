using UnityEngine;
using UnityEngine.Serialization;

namespace EnemiesScripts
{
    public class SundewTurret : MonoBehaviour
    {
        // Private variables:
        [FormerlySerializedAs("_bulletPrefab")] [SerializeField] private GameObject bulletPrefab;
        [FormerlySerializedAs("_mouthPosition")] [SerializeField] private Transform mouthPosition;

        private const float AttackTimer = 1f;
        private float _currentAttackTime = AttackTimer;

        private void Update()
        {
            if (_currentAttackTime <= 0f)
            {
                _currentAttackTime = AttackTimer;
                Shooting();
            }
            else {
                _currentAttackTime -= Time.deltaTime;
            }
        }
        private void Shooting() 
        {
            GameObject enemyBulletClone=Instantiate(bulletPrefab, mouthPosition);
            Rigidbody2D rb = enemyBulletClone.GetComponent<Rigidbody2D>();
            rb.velocity = mouthPosition.right * 10f;
        }
    }
}
