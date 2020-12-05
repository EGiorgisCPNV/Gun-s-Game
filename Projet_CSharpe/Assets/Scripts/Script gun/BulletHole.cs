using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BulletHole : MonoBehaviour
{

    //Public//
    public GameObject bulletHole ;
    public float distance = 1000f;
    

    //Private//
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log("BulletHole " + takeMethode.RandomPosition);


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
