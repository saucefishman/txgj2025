using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerController player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Terrain"))
            player.onEnterGround();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Terrain"))
            player.onStayGround();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Terrain"))
            player.onLeaveGround();
    }
}
