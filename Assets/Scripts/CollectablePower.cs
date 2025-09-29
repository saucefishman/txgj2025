using UnityEngine;

public class CollectablePower : MonoBehaviour
{
    public Power power;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController pc = other.GetComponent<TriggerHandler>()?.pc;
        if (pc is null)
        {
            return;
        }
        if (power == Power.Dash)
        {
            pc.giveDashAbility();
        }
        else if (power == Power.WallJump)
        {
            pc.giveWallJumpAbility();
        }
        
        Destroy(gameObject);
    }
}
