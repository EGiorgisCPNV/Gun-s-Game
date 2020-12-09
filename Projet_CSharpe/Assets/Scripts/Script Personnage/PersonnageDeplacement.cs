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
    Animator animation;


    //Private//
    private float basicSpeed;
    private bool FloorIsTouch = false;
    Vector3 velocity;//qlq chose de type Vector3 c'est juste pour indiquer une position -> velocity(1,1,1) = position 1,1,1 tout bêtement


    // Start is called before the first frame update
    void Start()
    {
        basicSpeed = speed;
        animation = gameObject.GetComponent<Animator>();

    }


    // Update is called once per frame
    void Update()
    {

        isCollider();
        toMove();
        toJump();
        toCrouch();
        toSprint();
        GetGravity();

    }


    //cette methode sert a chager la valeur de l'attribut FloorIsTouch en fonction de si le personnage touche le sol ou non
    private void isCollider()
    {
        if (controller.isGrounded)
        {
            FloorIsTouch = true;
        }
    }


    private void toMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //Déplacement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if(z == 1)
        {
            animation.SetBool("isWalking", true);
        }
        else if(z == -1)
        {

        }else if(z == 0)
        {
            animation.SetBool("isWalking", false);
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
                speed = basicSpeed;
                FloorIsTouch = false;
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
        if (Input.GetButtonDown("LeftShift") && FloorIsTouch && speed == basicSpeed)
        {
            speed *= 2;
            if (animation.GetBool("isWalking") == true)
            {
                animation.SetBool("isRunning", true);
            }
        }
        //fin de courir
        if (Input.GetButtonUp("LeftShift") && FloorIsTouch && speed == basicSpeed * 2)
        {
            speed /= 2;
            animation.SetBool("isRunning", false);

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
