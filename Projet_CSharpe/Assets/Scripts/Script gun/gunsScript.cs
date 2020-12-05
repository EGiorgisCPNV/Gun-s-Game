using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Input;

public class gunsScript : MonoBehaviour
{



    //Public//
    public float maxRange = 100f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impact;
    public float fireRate = 15f;


    //Private//
    private float damage = 10f;
    private float nextTimeToFire = 0f;
    AudioSource shootSound;
    Vector3 randomPosition;






    //cette accesseur sert a retourner la valeur des dégats
    public float Damage
    {
        get { return damage; }
    }


    public Vector3 RandomPosition
    {
        get
        {
            return randomPosition;
        }
    }

    //cette methode est appelée au lancement du programe 
    private void Start()
    {

        shootSound = GetComponent<AudioSource>();
    }


    // Update is called once per frame
    void Update()
    {
        //Random.Range(100f, 100f)

        //ne peut pas être initialiser en haut dans les attribut, que dans des methode
        randomPosition = new Vector3(0.1f, 0, 33f);

        //Debug.Log(randomPosition);

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
               nextTimeToFire = Time.time + 1f / fireRate;
               shootSound.Play();
               Shoot();
            
        }



    }

    

    //cette methode permet 
    void Shoot()
    {
        if (Input.GetButton("Space"))
        {
            Debug.Log("JumpShot");
            Debug.Log(randomPosition);
            JumpShot();
        }
        else
        {
            Debug.Log("StaticShot");
            Debug.Log(randomPosition);
            StaticShot();
        }
        
       
    }


    private void JumpShot()
    {
        muzzleFlash.Play();//affiche l'effet


        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, maxRange))
        {
            //Debug.Log(hit.transform.name);


            Targget target = hit.transform.GetComponent<Targget>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
                        

            GameObject bulletEffect = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bulletEffect, 1f);


        }
    }


    private void StaticShot()
    {
        muzzleFlash.Play();//affiche l'effet
        

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward , out hit, maxRange))
        {
            //Debug.Log(hit.transform.name);

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
