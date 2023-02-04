using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public string levelName;
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("MovementSpeed"))
        {
            PlayerPrefs.SetInt("MovementSpeed", 1);
        }
        if (!PlayerPrefs.HasKey("CamReloadSpeed"))
        {
            PlayerPrefs.SetInt("CamReloadSpeed", 1);
        }
        if (!PlayerPrefs.HasKey("MoneyMultiplier"))
        {
            PlayerPrefs.SetInt("MoneyMultiplier", 1);
        }
        if (!PlayerPrefs.HasKey("MonsterSpeed"))
        {
            PlayerPrefs.SetInt("MonsterSpeed", 1);
        }
        if (!PlayerPrefs.HasKey("Money"))
        {
            PlayerPrefs.SetInt("Money", 0);
        }
        PlayerPrefs.SetInt("MoneyEarned", 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadGame(bool isLevelOne)
    {
        SceneManager.LoadScene(levelName);
        if (isLevelOne && levelName == "GameScene")
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
            PlayerPrefs.SetInt("MoneyEarned", 0);
        } else
        {
            PlayerPrefs.SetInt("CurrentLevel", PlayerPrefs.GetInt("CurrentLevel") + 1);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
