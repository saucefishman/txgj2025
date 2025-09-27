using System;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public Direction wallDirection;
    public PlayerController player;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Terrain"))
            player.onEnterWall(wallDirection, other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Terrain"))
            player.onStayWall(wallDirection, other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Terrain"))
            player.onLeaveWall();
    }
}
