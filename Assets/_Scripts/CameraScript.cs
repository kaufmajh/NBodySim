using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform followObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followObject != null)
        {
            transform.position = new Vector3(followObject.position.x, followObject.position.y, transform.position.z);
        }
    }
}
