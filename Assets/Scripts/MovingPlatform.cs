using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform startTransform;
    private Vector2 startPoint;
    public Transform endTransform;
    private Vector2 endPoint;
    public float cycleTime = 2f;
    
    private Direction direction;
    private float relPosition;
    private Rigidbody2D rb;
    
    void Start()
    {
        direction = Direction.Right;
        rb = GetComponent<Rigidbody2D>();
        rb.position = startPoint;
        relPosition = 0;
        startPoint = startTransform.position;
        endPoint = endTransform.position;
    }

    void Update()
    {
        if (direction == Direction.Right)
        {
            relPosition += Time.deltaTime / cycleTime;
            if (relPosition >= 1) direction = Direction.Left;
        }
        else
        {
            relPosition -= Time.deltaTime / cycleTime;
            if (relPosition <= 0) direction = Direction.Right;
        }
        
        rb.position = Vector2.Lerp(startPoint, endPoint, relPosition);
    }
}
