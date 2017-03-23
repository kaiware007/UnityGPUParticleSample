using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour {

    public Vector3 axis = Vector3.up;
    public float rotSpeed = 100;
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(axis, rotSpeed * Time.deltaTime);
    }
}
