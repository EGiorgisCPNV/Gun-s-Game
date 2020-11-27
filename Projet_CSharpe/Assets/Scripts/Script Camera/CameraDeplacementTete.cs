using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDeplacementTete : MonoBehaviour
{
    // A SAVOIR QUE [CHIFFRE]f SIGNIFIE DES DEGRE ex. 100f -> 100 degrés //
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //Masque le curseur au lancement du jeu 
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        //variable pour designer les axe de souris 
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 25f);//bloque la vue de 90 degré a -90 degrés
        

        //Deplacement camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);// axe Y


    }
}
