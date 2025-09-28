using UnityEngine;

public class Lightning : Damager
{
    public float cycleTime = 2f;
    public float strikeTime = 0.5f;
    
    private Timer cycleTimer;
    private Timer strikeTimer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D coll;
    void Start()
    {
        cycleTimer = new Timer(cycleTime);
        strikeTimer = new Timer(strikeTime);
        cycleTimer.restart();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (cycleTimer.isFinished())
        {
            animator.SetTrigger("strike");
            spriteRenderer.enabled = true;
            coll.enabled = true;
            cycleTimer.restart();
            strikeTimer.restart();
        }

        if (strikeTimer.hasJustFinished())
        {
            spriteRenderer.enabled = false;
            coll.enabled = false;
        }
    }
}
