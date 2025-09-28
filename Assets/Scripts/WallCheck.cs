using System;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public Direction wallDirection;
    public PlayerController player;
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
            player.onEnterWall(wallDirection, collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
            player.onStayWall(wallDirection, collision);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Terrain"))
            player.onLeaveWall();
    }
}
