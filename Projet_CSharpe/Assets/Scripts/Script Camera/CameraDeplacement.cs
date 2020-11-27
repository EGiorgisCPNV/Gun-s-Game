using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDeplacement : MonoBehaviour
{
    // A SAVOIR QUE [CHIFFRE]f SIGNIFIE DES DEGRE ex. 100f -> 100 degrés //
    public float mouseSensitivity = 100f;
    

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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

     

        //Deplacement camera
        transform.Rotate(Vector3.up * mouseX);// axe X 
 

    }
}
