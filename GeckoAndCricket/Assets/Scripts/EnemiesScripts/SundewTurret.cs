using UnityEngine;

namespace EnemiesScripts
{
    public class SundewTurret : MonoBehaviour
    {
        // Private variables:
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _mouthPosition;

        private const float AttackTimer = 1f;
        private float currentAttackTime = AttackTimer;

        private void Update()
        {
            if (currentAttackTime <= 0f)
            {
                currentAttackTime = AttackTimer;
                Shooting();
            }
            else {
                currentAttackTime -= Time.deltaTime;
            }
        }
        private void Shooting() 
        {
            GameObject enemyBulletClone=Instantiate(_bulletPrefab, _mouthPosition);
            Rigidbody2D rb = enemyBulletClone.GetComponent<Rigidbody2D>();
            rb.velocity = _mouthPosition.right * 10f;
        }
    }
}
