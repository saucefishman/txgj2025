using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private static string TERRAIN_TAG = "Terrain";
    public PlayerController player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(TERRAIN_TAG))
            player.onEnterGround(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(TERRAIN_TAG))
            player.onStayGround();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(TERRAIN_TAG))
            player.onLeaveGround();
    }
}
