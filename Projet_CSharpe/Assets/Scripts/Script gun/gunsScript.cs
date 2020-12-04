using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Input;

public class gunsScript : MonoBehaviour
{

    //Public//
    public float damage = 10f;
    public float maxRange = 100f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impact;
    public float fireRate = 15f;
    Vector3 randomPosition;

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
        //ne peut pas être initialiser en haut dans les attribut, que dans des methode
        randomPosition = new Vector3(Random.Range(0, 10), Random.Range(0, 10), 100);

        //Debug.Log(randomPosition);

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
               nextTimeToFire = Time.time + 1f / fireRate;
               shootSound.Play();
               Shoot();
            }


        
    }



    public Vector3 RandomPosition
    {
        get
        {
            return randomPosition;
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
        tireAssit();
        tirStatique();
    }


    private void tireAssit()
    {
        muzzleFlash.Play();//affiche l'effet

        //transform.TransformDirection(position)

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, randomPosition, out hit, maxRange) && Input.GetButtonDown("Space"))
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


    private void tirStatique()
    {
        muzzleFlash.Play();//affiche l'effet

        //transform.TransformDirection(position)

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward , out hit, maxRange))
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
