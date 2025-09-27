using UnityEngine;

public class HealthPack : MonoBehaviour
{
    public float amount = 10f;

    public float consumeAndGetAmount()
    {
        Destroy(gameObject);
        return amount;
    }
}
