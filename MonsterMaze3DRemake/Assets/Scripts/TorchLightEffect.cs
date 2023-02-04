using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchLightEffect : MonoBehaviour
{

    public float min, max, delay;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        GetComponent<Light>().intensity = Random.Range(min, max);
        yield return new WaitForSeconds(delay);
        StartCoroutine(Flicker());
    }
}
