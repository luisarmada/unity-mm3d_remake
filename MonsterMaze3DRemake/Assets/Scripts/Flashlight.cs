using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{

    public GameObject mainCamera;
    [SerializeField] private float speed;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, mainCamera.transform.rotation, speed * Time.deltaTime);
    }
}
