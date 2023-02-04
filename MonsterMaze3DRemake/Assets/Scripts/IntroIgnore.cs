using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroIgnore : MonoBehaviour
{

    public GameObject monster, camera, player, introcanvas;

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("CurrentLevel") > 1)
        {
            monster.SetActive(true);
            camera.SetActive(false);
            player.SetActive(true);
            introcanvas.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
