using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerPrefText : MonoBehaviour
{

    public int atIndex;
    public string playerPref;

    private string originalText;

    public bool upgradeIndex = false;
    // Start is called before the first frame update
    void Start()
    {
        originalText = GetComponent<TMP_Text>().text;
        UpdateText();
    }

    public void UpdateText()
    {
        string text = originalText;
        GetComponent<TMP_Text>().SetText(text.Insert(atIndex, "" + PlayerPrefs.GetInt(playerPref)));
        if (upgradeIndex)
        {
            GetComponent<TMP_Text>().SetText(GetComponent<TMP_Text>().text.Insert(GetComponent<TMP_Text>().text.Length - 1, "" + PlayerPrefs.GetInt(playerPref) * 1000));
        }
    }

}
