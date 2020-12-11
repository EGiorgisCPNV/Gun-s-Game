


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Input;

public class gunsScript : MonoBehaviour
{

    //Public//
    public PersonnageDeplacement personnage;
    public float maxRange = 100f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impact;
    public float fireRate = 15f;

    //Private//
    private Vector3 variableAccuracy;
    private Vector3 accuracyDiminution;
    private float damage=10.0f;
    private float nextTimeToFire;
    AudioSource shootSound;
    Vector3 randomShotWalk;
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


    public Vector3 RandomShotWalk
    {
        get { return randomShotWalk; }
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
        nextTimeToFire = 0f;
        shootSound = GetComponent<AudioSource>();
        variableAccuracy = new Vector3(0f,0f,0f);
        accuracyDiminution = new Vector3(0f, 0.01f, 0);
    }



    // Update is called once per frame
    void Update()
    {

        randomShotWalk = new Vector3(Random.Range(-0.065f, 0.065f), Random.Range(-0.065f, 0.065f), 0f);
        randomShotJump = new Vector3(Random.Range(-0.08f, 0.07f), Random.Range(-0.07f, 0.07f), 0f);
        randomShotSprint = new Vector3(Random.Range(-0.08f, 0.08f), Random.Range(-0.08f, 0.08f), 0f);


        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            shootSound.Play();
            Shoot();

        }
        if (Input.GetButtonUp("Fire1"))
        {
            variableAccuracy = new Vector3(0f, 0f, 0f);
        }
    }
  


    //cette methode permet 
    void Shoot()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (Input.GetButton("Space") && personnage.FloorIsTouch==false)
        {
            Debug.Log("JumpShot");
            JumpShot();
        }
        else if (Input.GetButton("LeftShift") && (x == 1 || x == -1))
        {
            Debug.Log("SprintShot");
            SprintShot();
        }
        else if (x==1 || x==-1)
        {
            Debug.Log("WalkShot");
            WalkShot();
        }
        else
        {
            Debug.Log("StaticShot");
            StaticShot();
        }


    }


    private void StaticShot()
    {
        muzzleFlash.Play();//affiche l'effet

        variableAccuracy += accuracyDiminution;//Diminution de la précision au fil du tir

        RaycastHit hit;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + variableAccuracy, out hit, maxRange))
        {
            //Debug.Log(hit.transform.name);
            //fpsCam.transform;

            //Debug.Log(fpsCam.transform);

            Targget target = hit.transform.GetComponent<Targget>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            GameObject bulletEffect = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(bulletEffect, 1f);
        }
    }



    private void WalkShot()
    {
        muzzleFlash.Play();//affiche l'effet

        variableAccuracy += accuracyDiminution;//Diminution de la précision au fil du tir

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + randomShotWalk + variableAccuracy, out hit, maxRange))
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

    private void SprintShot()
    {
        muzzleFlash.Play();//affiche l'effet

        variableAccuracy += accuracyDiminution;//Diminution de la précision au fil du tir

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + randomShotSprint + variableAccuracy, out hit, maxRange))
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

        variableAccuracy += accuracyDiminution;//Diminution de la précision au fil du tir

        RaycastHit hit;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + randomShotJump + variableAccuracy, out hit, maxRange))
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


    
}
