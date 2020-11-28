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
    public Transform LeftLeg;
    public Transform RightLegCuisse;
    public Transform RightLegTibia;


    //Private//
    private float basicSpeed;
    private bool FloorIsTouch = false;
    Vector3 velocity;//qlq chose de type Vector3 c'est juste pour indiquer une position -> velocity(1,1,1) = position 1,1,1 tout bêtement


    // Start is called before the first frame update
    void Start()
    {
        basicSpeed = speed;
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


    //cette methode sert a chager la valeur de l'attribut FloorIsTouch en fonction de si le personnage touche le sol ou non
    //A savoir que cette methode est comme la methode Update chaque frame elle est appelée
    void OnCollisionEnter(Collision col)
    {

        if (col.collider.name == "Ground_02")
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
        //s'acroupir
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            controller.height /= 2;
            speed /= 3;
            LeftLeg.localRotation = Quaternion.Euler(LeftLeg.localRotation.x, LeftLeg.localRotation.y, -100f);
            RightLegCuisse.localRotation = Quaternion.Euler(2.213f, -88.58f, 0f);
            RightLegTibia.localRotation = Quaternion.Euler(0f, 0f, -88.284f);
        }
        //fin de s'acroupire
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            controller.height *= 2;
            speed *= 3;
            LeftLeg.localRotation = Quaternion.Euler(LeftLeg.localRotation.x, LeftLeg.localRotation.y, 0f);
            RightLegCuisse.localRotation = Quaternion.Euler(2.213f, -88.58f, -88.284f);
            RightLegTibia.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }


    //Methode pour pouvoir courir
    private void toSprint()
    {
        
        //courir 
        if (Input.GetButtonDown("Fire3") && FloorIsTouch && speed == basicSpeed)
        {
            speed *= 2;
        }
        //fin de courir
        if (Input.GetButtonUp("Fire3") && FloorIsTouch && speed == basicSpeed * 2)
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
