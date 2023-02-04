using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelLoader : MonoBehaviour
{

    public GameObject escapeCanvas;

    private bool lockUpdate = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (lockUpdate) return;

        if(Input.GetKeyDown(KeyCode.Y)){
            SceneManager.LoadScene("GameScene");
            PlayerPrefs.SetInt("CurrentLevel", PlayerPrefs.GetInt("CurrentLevel") + 1);
            lockUpdate = true;
        } else if (Input.GetKeyDown(KeyCode.N)) {
            escapeCanvas.SetActive(true);
            PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money") + PlayerPrefs.GetInt("MoneyEarned"));
            lockUpdate = true;
        }

    }
}
