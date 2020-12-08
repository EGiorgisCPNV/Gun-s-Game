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
    Vector3 randomShotJump;
    Vector3 randomShotSprint;


    //cette accesseur sert a retourner la valeur des dégats
    public float Damage
    {
        get { return damage; }
    }


    public Vector3 RandomShotJump
    {
        get { return randomShotJump; }
        /*
        set
        {
            randomShotJump = value;
            //Debug.Log("JUUUUUUUUUUMP" + randomShotJump);
        }
        */
    }

    public Vector3 RandomShotSprint
    {
        get { return randomShotSprint; }
        /*
        set
        {
            randomShotSprint = value;
            //Debug.Log("SPRIIIIIIIIIIIINT" + randomShotSprint);

        }
        */
    }

    //cette methode est appelée au lancement du programe 
    private void Start()
    {
        shootSound = GetComponent<AudioSource>();
    }



    // Update is called once per frame
    void Update()
    {
        randomShotJump = new Vector3(Random.Range(-0.065f, 0.065f), Random.Range(-0.065f, 0.065f), 0f);
        randomShotSprint = new Vector3(Random.Range(-0.07f, 0.07f), Random.Range(-0.07f, 0.07f), 0f);

        Debug.Log("asdasdas" + RandomShotJump);

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
               nextTimeToFire = Time.time + 1f / fireRate;
               shootSound.Play();
               Shoot();
            
        }
    }

    /*
    public void ShowRandomShotJumpValue()
    {
        Debug.Log("Juuuuump  " + randomShotJump);
    }

    public void ShowRandomShotSprintValue()
    {
        Debug.Log("Spriiiiiint  " + randomShotSprint);
    }
    */


    //cette methode permet 
    void Shoot()
    {
        

        if (Input.GetButton("Space"))
        {
            //Debug.Log("JumpShot");
            JumpShot();
        }
        else if (Input.GetButton("LeftShift"))
        {
            //Debug.Log("SprintShot");
            SprintShot();
        }
        else
        {
            //Debug.Log("StaticShot");
            StaticShot();
        }
        
      
    }


    private void StaticShot()
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


    private void JumpShot()
    {
        muzzleFlash.Play();//affiche l'effet

        

        RaycastHit hit;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + randomShotJump, out hit, maxRange))
        {
            //Debug.Log(hit.transform.name);
            //Debug.Log(randomShotJump);

            Targget target = hit.transform.GetComponent<Targget>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
                        
            GameObject bulletEffect = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bulletEffect, 1f);
        }
    }


    private void SprintShot()
    {
        muzzleFlash.Play();//affiche l'effet

      
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + randomShotSprint, out hit, maxRange))
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
