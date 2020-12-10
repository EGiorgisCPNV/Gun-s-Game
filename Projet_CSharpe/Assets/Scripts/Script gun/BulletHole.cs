using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BulletHole : MonoBehaviour
{

    //Public//
    public GameObject bulletHole;
    public float distance = 1000f;
    gunsScript takeMethode;

    //Private//
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        takeMethode = new gunsScript();
    }


    // Update is called once per frame
    void Update()
    {

        /*
        takeMethode.RandomShotJump = new Vector3(Random.Range(-0.065f, 0.065f), Random.Range(-0.065f, 0.065f), 0f);
        takeMethode.RandomShotSprint = new Vector3(Random.Range(-0.07f, 0.07f), Random.Range(-0.07f, 0.07f), 0f);
        Debug.Log("BulletHole " + takeMethode.RandomShotJump);
        */
        //Debug.Log("BulletHole jump shot " + takeMethode.RandomShotJump);



        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;


            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distance))
            {
                Targget target = hit.transform.GetComponent<Targget>();
                if (target.heatlh > 0)
                {
                    GameObject bH = Instantiate(bulletHole, hit.point + new Vector3(0f, 0f, -.02f), Quaternion.LookRotation(-hit.normal));

                    target.addBulletHole(bH);
                }

            }
        }
    }
}
