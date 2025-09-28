using UnityEngine;

public class Jitterer : MonoBehaviour
{
    public float jitterAmount = 0.1f;
    private SpriteRenderer spriteRenderer;
	private Vector3 originalPosition;
	private bool jittering;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;
        jittering = false;
    }

	void Update()
    {
	    if (jittering)
	    {
		    var newPosition = originalPosition + new Vector3(
			    Random.Range(-jitterAmount, jitterAmount),
			    Random.Range(-jitterAmount, jitterAmount),
			    0);
		    transform.position = newPosition;
	    }
	    else
	    {
		    transform.position = originalPosition;
	    }
    }
	
	public void enableJitter()
	{
		jittering = true;
	}
	
	public void disableJitter()
	{
		jittering = false;
	}
}