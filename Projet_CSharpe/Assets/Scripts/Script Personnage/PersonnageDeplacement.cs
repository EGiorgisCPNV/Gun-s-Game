using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Input;

public class PersonnageDeplacement : MonoBehaviour
{

    //Public//
    public CharacterController controller;
    public float speed = 12f;  
    public float gravity = -9.81f;
    public float jump = 4;
    


    //Private//
    private float basicSpeed;
    public bool floorIsTouch = false;
    Animator animations;
    Vector3 velocity;//qlq chose de type Vector3 c'est juste pour indiquer une position -> velocity(1,1,1) = position 1,1,1 tout bêtement


    // Start is called before the first frame update
    void Start()
    {
        basicSpeed = speed;
        animations = gameObject.GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {

        isCollider();
        toJump();
        toMove();
        toCrouch();
        toSprint();
        GetGravity();

    }


    //cette methode sert a chager la valeur de l'attribut FloorIsTouch en fonction de si le personnage touche le sol ou non
    private void isCollider()
    {
        if (controller.isGrounded)
        {
            floorIsTouch = true;
        }
    }

    public bool FloorIsTouch
    {
        get
        {
            return floorIsTouch;
        }
    }


    private void toMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;


        //cette condition sert a garder la même vitesse quant appuie sur une seul touche ou deux touche en même temps
        //A savoir x c'est juste quand il va a droite x=1 quand bouge pas 0 et quand il va a gauche a -1
        if (x != 0 && z != 0)
        {
            controller.Move(move / 2 * speed * Time.deltaTime);
        }
        else
        {
            controller.Move(move * speed * Time.deltaTime);
        }




        if (z == 1)
        {
            animations.SetBool("isWalking", true);
        }
        else if(z == -1)
        {

        }else if(z == 0)
        {
            animations.SetBool("isWalking", false);
        }
        
    }


    //Methode pour pouvoir sauter
    private void toJump()
    {
        //saut
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (FloorIsTouch)
            {
                velocity.y = jump;

                while (speed > basicSpeed)
                {
                    speed = speed - 0.5f; //ATTENTION IL FAUT QU^'IL TERMINE PAS 0 OU 5 POUR QU'IL PUISSE VRM ATTEINDRE 5 

                    if (speed <= 0)
                    {
                        break;
                    }
                }

                //Debug.Log(speed);
                floorIsTouch = false;


            }
        }
    }


    //Methode pour pouvoir s'acroupir
    private void toCrouch()
    {
       
    }


    //Methode pour pouvoir courir
    private void toSprint()
    {
        
        //courir 
        if (Input.GetButtonDown("LeftShift") && floorIsTouch && speed == basicSpeed)
        {
            speed *= 2;
            if (animations.GetBool("isWalking") == true)
            {
                animations.SetBool("isRunning", true);
            }
        }
        //fin de courir
        if (Input.GetButtonUp("LeftShift") && floorIsTouch && speed == basicSpeed * 2)
        {
            speed /= 2;
            if (animations.GetBool("isWalking") == false)
            {
                animations.SetBool("isRunning", false);
            }

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
