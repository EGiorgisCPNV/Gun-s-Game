using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    //Public//
    public float heatlh = 50f;
    public GameObject destroyedVersion;


    //Private//
    private ArrayList bulletHoles = new ArrayList();


    //cette methode va permettre de faire perdre de la vie a l'objet qui prend des balles
    public void TakeDamage(float amount)
    {
      
       
            heatlh -= amount;
            if (heatlh <= 0f)
            {
                Die();
            }
       
    }

    //Cette methode sert a pouvoir mourir dans le jeu
    void Die()
    {
        Instantiate(destroyedVersion, transform.position, transform.rotation);
        Destroy(gameObject);

    }

}