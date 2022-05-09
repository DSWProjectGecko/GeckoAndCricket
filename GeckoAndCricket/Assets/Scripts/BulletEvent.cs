using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEvent : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Destroy(gameObject);
        if (collider.tag == "EnemyWeakPoint")
        {
            GameObject enemy = collider.transform.parent.gameObject;
            if (enemy.GetComponent<BatAI>() == null)
            {
                Debug.Log("Pierwszy if");
                Destroy(enemy);
            }
            else if(!enemy.GetComponent<BatAI>().isImmune)
            {
                Debug.Log("Drugi if");
                Destroy(enemy);
            }
        }
    }
}
