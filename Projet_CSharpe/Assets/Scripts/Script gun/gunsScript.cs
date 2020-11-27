using UnityEngine;

public class gunsScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;



    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (!Input.GetKeyDown(KeyCode.LeftControl))
            {
                Shoot();
            }
            
        }
    }

    void Shoot()
    {

        muzzleFlash.Play();//affiche l'effet

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Targget target = hit.transform.GetComponent<Targget>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }
}
