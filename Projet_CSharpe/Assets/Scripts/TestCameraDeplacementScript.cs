using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonnageCameraMouvement : MonoBehaviour
{

    [SerializeField] Transform target;
    Vector3 offsetCamera;

    // Start is called before the first frame update
    void Start()
    {
        offsetCamera = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPosition = target.position + offsetCamera;
        transform.position = cameraPosition;
    }
}
