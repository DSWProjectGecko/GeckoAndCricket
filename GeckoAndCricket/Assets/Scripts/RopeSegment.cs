using UnityEngine;

public class RopeSegment : MonoBehaviour
{
    public GameObject above, below;
    public bool isPlayerAttached;

    private void Start()
    {
        above = GetComponent<HingeJoint2D>().connectedBody.gameObject;
        RopeSegment aboveSeg = above.GetComponent<RopeSegment>();
        if (aboveSeg != null)
        {
            aboveSeg.below = gameObject;
            float spriteBottom = above.GetComponent<SpriteRenderer>().bounds.size.y;
            GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0, spriteBottom * -1);
        }
        else 
        {
            GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0, 0);
        }
    }
}
