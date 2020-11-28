using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Input;

public class gunsScript : MonoBehaviour
{

    //Public//
    public float damage = 10f;
    public float range = 100f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impact;
    public float fireRate = 15f;


    //Private//
    private float nextTimeToFire = 0f;
    AudioSource shootSound;

    //cette methode est appelée au lancement du programe 
    private void Start()
    {
        shootSound = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update()
    {      
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
               nextTimeToFire = Time.time + 1f / fireRate;
               shootSound.Play();
               Shoot();
            }    
    }


    //cette accesseur sert a retourner la valeur des dégats
    public float Damage
    {
        get { return damage; }
    }


    //cette methode permet 
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

            GameObject bulletEffect = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bulletEffect, 1f);
        }
    }


}
