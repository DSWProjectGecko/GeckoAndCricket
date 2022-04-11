using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEvent : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
        if (collision.tag == "EnemyWeakPoint")
        {
            Destroy(collision.transform.parent.gameObject);
        }
    }
}
