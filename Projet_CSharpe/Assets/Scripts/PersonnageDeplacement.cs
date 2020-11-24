using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonnageDeplacement : MonoBehaviour
{
    /*
    public int BaseSpeed = 5;
    private Vector3 DirectionDeplacement = Vector3.zero;
    private CharacterController Player;
    public int Sensibility;
    public int Jump = 5;
    public int Gravity = -1;
    */

    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jump = 4;
    Vector3 velocity;
    

    // Start is called before the first frame update
    void Start()
    {
        /*
        Player = GetComponent<CharacterController>();
        */

    }

    // Update is called once per frame
    void Update()
    {
        /*
        DirectionDeplacement.z = Input.GetAxisRaw("Vertical");
        DirectionDeplacement.x = Input.GetAxisRaw("Horizontal");
        DirectionDeplacement = transform.TransformDirection(DirectionDeplacement);

        //Deplacement
        Player.Move(DirectionDeplacement * Time.deltaTime * BaseSpeed);
             
        //saut
        if (Input.GetKeyDown(KeyCode.Space) && Player.isGrounded)
        {
            DirectionDeplacement.y = Jump;
        }

        //Gravité
        if (!Player.isGrounded)
        {
            DirectionDeplacement.y += Gravity * Time.deltaTime;
        }
        */

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


        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed = speed * 2;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = speed / 2;
        }



        //Gravité
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);



    }
}
