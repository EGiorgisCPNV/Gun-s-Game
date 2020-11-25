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

    //int jumpCooldown = 0;

    bool trueJump = true;

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
       

        toMove();
        toJump();
        toCrouch();
        toSprint();
        GetGravity();        
    }

    private void toMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //Déplacement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

    }
    

    //Methode pour pouvoir sauter
    private void toJump()
    {
        /*
        jumpCooldown--;

        if(jumpCooldown > 0)
        {
            return;
        }
        */

        //saut
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //jumpCooldown = 120;
            velocity.y = jump;
            trueJump = false;

        }
    }

    /*
    void OnColliderEnter(Collision col)
    {
        if (col.CompareTag("Ground"))
        {
            trueJump = true;
        }
    }
    */

    //Methode pour pouvoir s'acroupir
    private void toCrouch()
    {
        //s'acroupir
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            controller.height /= 2;
            speed /= 3;
        }
        //fin de s'acroupire
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            controller.height *= 2;
            speed *= 3;
        }
    }

    //Methode pour pouvoir courir
    private void toSprint()
    {
        //courir 
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed *= 2;
        }
        //fin de courir
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed /= 2;
        }
    }

    //Methode pour la gravité
    private void GetGravity()
    {
        //Gravité
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


}
