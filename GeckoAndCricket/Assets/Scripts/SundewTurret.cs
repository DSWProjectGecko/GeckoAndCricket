using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseScripts;

public class SundewTurret : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform mouthPosition;
    static private float attackTimer = 1f;
    private float currentAttackTime = attackTimer;
    void Update()
    {
        if (currentAttackTime <= 0f)
        {
            currentAttackTime = attackTimer;
            Shooting();
        }
        else {
            currentAttackTime -= Time.deltaTime;
        }
    }
    void Shooting() {
        GameObject enemyBulletClone=Instantiate(bulletPrefab, mouthPosition);
        Rigidbody2D rb = enemyBulletClone.GetComponent<Rigidbody2D>();
        rb.velocity = mouthPosition.right * 10f;
    }
}
