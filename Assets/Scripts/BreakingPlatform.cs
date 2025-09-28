using UnityEngine;

public class BreakingPlatform : MonoBehaviour
{
    public float breakDelay = 1.0f;
    public float respawnDelay = 1.0f;
    public Jitterer jitterer;

    private Timer breakTimer;
    private Timer respawnTimer;
    private Collider2D coll;

    void Start()
    {
        coll = GetComponent<Collider2D>();
        breakTimer = new Timer(breakDelay);
        respawnTimer = new Timer(respawnDelay);
        setComponentsActive(true);
    }

    void Update()
    {
        if (breakTimer.hasJustFinished())
        {
            respawnTimer.restart();
            jitterer.disableJitter();
            setComponentsActive(false);
        }

        if (respawnTimer.isFinished())
        {
            setComponentsActive(true);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            breakTimer.restart();
            jitterer.enableJitter();
        }
    }

    void setComponentsActive(bool active)
    {
        coll.enabled = active;
        jitterer.gameObject.SetActive(active);
    }
}
