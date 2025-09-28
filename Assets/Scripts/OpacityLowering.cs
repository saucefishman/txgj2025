using System;
using UnityEngine;

public class OpacityLowering : MonoBehaviour
{
    public float lowestOpacity = 0.5f; // Opacity when player is behind the object
    public float distanceThreshold = 1.0f; // Distance threshold to start lowering opacity
    private SpriteRenderer _spriteRenderer;
    private Transform playerTransform;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerTransform is not null)
        {
            if (Math.Abs(transform.position.x - playerTransform.position.x) < distanceThreshold)
            {
                Color color = _spriteRenderer.color;
                float distance = Math.Abs(transform.position.x - playerTransform.position.x);
                color.a = Mathf.Lerp(1.0f, lowestOpacity, (distanceThreshold - distance) / distanceThreshold);
                _spriteRenderer.color = color;
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
            Color color = _spriteRenderer.color;
            color.a = 1.0f;
            _spriteRenderer.color = color;
        }
    }
}
