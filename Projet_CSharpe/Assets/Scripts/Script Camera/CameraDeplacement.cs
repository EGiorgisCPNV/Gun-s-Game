using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDeplacement : MonoBehaviour
{
    //Public//
    // A SAVOIR QUE [CHIFFRE]f SIGNIFIE DES DEGRE ex. 100f -> 100 degrés //
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public PersonnageDeplacement personneCam;



    // Start is called before the first frame update
    void Start()
    {
        //Masque le curseur au lancement du jeu 
        Cursor.lockState = CursorLockMode.Locked;

    }


    // Update is called once per frame
    void Update()
    {

        if (personneCam.Test == false)
        {
            //variable pour designer les axe de souris 
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            /*
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 45f);//bloque la vue de 90 degré a -90 degrés
            */

            //Deplacement camera
            //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);// axe Y
            playerBody.Rotate(Vector3.up * mouseX);// axe X 
        }

            


    }
}
