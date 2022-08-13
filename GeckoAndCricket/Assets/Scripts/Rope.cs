using UnityEngine;

public class Rope : MonoBehaviour
{
    public Rigidbody2D hook;
    public GameObject prefSeg;
    public int numLinks = 5;

    private void Start()
    {
        GenerateRope();   
    }

    private void GenerateRope()
    {
        Rigidbody2D prevRb = hook;
        for (int i = 0; i < numLinks; i++) 
        {
            GameObject newSeg = Instantiate(prefSeg);
            newSeg.transform.parent = transform;
            newSeg.transform.position = transform.position;
            HingeJoint2D hj = newSeg.GetComponent<HingeJoint2D>();
            hj.connectedBody = prevRb;
            prevRb = newSeg.GetComponent<Rigidbody2D>();
        }
    }
}
