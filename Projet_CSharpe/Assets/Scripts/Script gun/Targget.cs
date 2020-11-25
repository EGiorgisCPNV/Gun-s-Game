using UnityEngine;

public class Targget : MonoBehaviour
{
    public float heatlh = 10f;

    public void TakeDamage(float amount)
    {
        heatlh -= amount;
        if (heatlh <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
