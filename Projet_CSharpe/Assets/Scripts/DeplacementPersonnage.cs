using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeplacementPersonnage : MonoBehaviour
{
    public int BaseSpeed = 5;
    private Vector3 DirectionDeplacement = Vector3.zero;
    private CharacterController Player;
    public int Sensibility;
    public int Jump = 5;
    public int gravity = -1;

    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponent<CharacterController>();


    }

    // Update is called once per frame
    void Update()
    {
        DirectionDeplacement.z = Input.GetAxisRaw("Vertical");
        DirectionDeplacement.x = Input.GetAxisRaw("Horizontal");
        DirectionDeplacement = transform.TransformDirection(DirectionDeplacement);

        //Deplacement
        Player.Move(DirectionDeplacement * Time.deltaTime * BaseSpeed);
        transform.Rotate(0, Input.GetAxisRaw("Mouse X") * Sensibility, 0);


        //saut
        if(Input.GetKeyDown(KeyCode.Space) && Player.isGrounded)
        {
            DirectionDeplacement.y = Jump;
        }

        //Gravité
        if (!Player.isGrounded)
        {
            DirectionDeplacement.y += gravity * Time.deltaTime;
        }

    }
}
