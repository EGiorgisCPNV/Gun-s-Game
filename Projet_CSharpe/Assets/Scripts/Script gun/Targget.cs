using UnityEngine;
using System.Collections;

public class Targget : MonoBehaviour
{
    public float heatlh = 10f;
    public bool damageable = true;
    private ArrayList bulletHoles = new ArrayList();

    public void TakeDamage(float amount)
    {
        if (damageable)
        {
            heatlh -= amount;
            if (heatlh <= 0f)
            {
                Die();
            }
        }
    }

    void Die()
    {
        
        foreach(GameObject gameobject in bulletHoles)
        {
            Destroy(gameobject);
        }
        Destroy(gameObject);

    }

    public void addBulletHole(GameObject bulletHole)
    {
        bulletHoles.Add(bulletHole);
        if(!damageable)
        {
            Destroy(bulletHole, 3f);
        }
    }

    public float Health
    {
        get { return heatlh; }
        
    }
        
}
