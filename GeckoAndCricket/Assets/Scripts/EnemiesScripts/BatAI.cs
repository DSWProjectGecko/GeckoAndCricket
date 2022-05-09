using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatAI : MonoBehaviour
{
    [SerializeField] GameObject path;
    private List<Vector3> BatPathPositions = new List<Vector3>();
    private int currentPosition;
    public bool detectPlayer = false;
    PolygonCollider2D batVisionCollider;
    public bool isImmune = true;
    public static float checkTimer = 3f;
    public float currentCheckTimer = checkTimer;
    public float speedTime = 10f;


    // Start is called before the first frame update
    void Start()
    {
        batVisionCollider = this.gameObject.transform.GetChild(0).GetComponent<PolygonCollider2D>();
        foreach (Transform child in path.transform) {
            BatPathPositions.Add(child.position);
        }
        currentPosition = 0;
        transform.position = BatPathPositions[0];

    }

    // Update is called once per frame
    void Update()
    {
        Attack();
        CheckTerrain();
    }
    void Attack() {
        if (detectPlayer)
        {
            batVisionCollider.enabled = false;
            isImmune = false;
            if (currentPosition == 1)
            {
                speedTime = 20f;
            }
            else
            {
                speedTime = 10f;
            }
            if (currentPosition <= 1 && transform.position != BatPathPositions[currentPosition + 1]) {
                transform.position = Vector3.MoveTowards(transform.position, BatPathPositions[currentPosition + 1], speedTime * Time.deltaTime);
                if (transform.position == BatPathPositions[currentPosition + 1]) {
                    currentPosition += 1;   
                }
            }
            else {
                if (transform.position != BatPathPositions[0])
                {
                    transform.position = Vector3.MoveTowards(transform.position, BatPathPositions[0], speedTime * Time.deltaTime);
                }
                else {
                    currentPosition = 0;
                    detectPlayer = false;
                    isImmune = true;
                }
            }
        }
    }
    void CheckTerrain() {
        if (isImmune)
        {
            if (currentCheckTimer <= 0)
            {
                batVisionCollider.enabled = !batVisionCollider.enabled;
                currentCheckTimer = checkTimer;
            }
            else
            {
                currentCheckTimer -= Time.deltaTime;
            }
        }
        else {
            currentCheckTimer = checkTimer;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") {
            detectPlayer = true;
        }
    }
}
