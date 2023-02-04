using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{

    public string prefName;
    public GameObject textobj, moneyDisplay;

    public void OnClick()
    {
        int money = PlayerPrefs.GetInt("Money");
        int cost = PlayerPrefs.GetInt(prefName) * 1000;
        if (money < cost) return;

        PlayerPrefs.SetInt("Money", money - cost);
        PlayerPrefs.SetInt(prefName, PlayerPrefs.GetInt(prefName) + 1);
        textobj.GetComponent<PlayerPrefText>().UpdateText();
        moneyDisplay.GetComponent<PlayerPrefText>().UpdateText();

    }
}
