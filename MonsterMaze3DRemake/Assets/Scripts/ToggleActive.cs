using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    public GameObject obj;

    public void Toggle()
    {
        obj.SetActive(!obj.activeSelf);
    }
}
