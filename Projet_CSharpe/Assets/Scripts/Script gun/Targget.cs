using UnityEngine;
using System.Collections;

public class Targget : MonoBehaviour
{
    //Public//
    public float heatlh = 10f;
    public bool damageable = true;


    //Private//
    private ArrayList bulletHoles = new ArrayList();


    //cette methode va permettre de faire perdre de la vie a l'objet qui prend des balles
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


    //Cette methode sert a pouvoir mourir dans le jeu
    void Die()
    {

        foreach (GameObject gameobject in bulletHoles)
        {
            Destroy(gameobject);
        }
        Destroy(gameObject);

    }


    //cette methode va permettre a un objet de prendre des projectil
    public void addBulletHole(GameObject bulletHole)
    {
        bulletHoles.Add(bulletHole);
        if (!damageable)
        {
            Destroy(bulletHole, 3f);
        }
    }


    //cette accesseur permet de retourner la valeur de la vie 
    public float Health
    {
        get { return heatlh; }

    }

}
