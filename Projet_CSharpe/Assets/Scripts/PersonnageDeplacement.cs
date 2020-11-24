using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonnageDeplacement : MonoBehaviour
{
    
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jump = 4;
    Vector3 velocity;
    

    // Start is called before the first frame update
    void Start()
    {

        /* COMPRENDRE transform
         si il y a transform.position = Vector3.zero
        alors au lancement du jeu le personnage ira a la position (0, 0, 0)
        */
    }

    // Update is called once per frame
    void Update()
    {
        

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //Déplacement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);


        //saut
        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jump;
        }


        //courir 
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed = speed * 2;           
        }
        //fin de courir
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = speed / 2;
        }



        //Gravité
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);



    }
}
