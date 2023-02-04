using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;

public class TextTypewrite : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string targetText;
    [SerializeField] private float typewriteSpeed = 0.06f;
    [SerializeField] private float eventDelay = 2f;
    [SerializeField] private UnityEvent onFinishEvent;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private bool isFinished = false;

    private bool lockEndEvents = false;

    public bool isEscapeScreen = false;

    private float previousVolume;

    // Start is called before the first frame update
    void Start()
    {
        
        if (isEscapeScreen)
        {
            targetText.Insert(12, "" + PlayerPrefs.GetInt("MoneyEarned"));
        }

        StartCoroutine(Typewrite());
        previousVolume = audioSource.volume;
    }

    void Update()
    {
        if (!isFinished && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            isFinished = true;
            StopCoroutine(Typewrite());
            gameObject.GetComponent<TMP_Text>().SetText(targetText);
            
            audioSource.pitch = 1f;
            audioSource.volume = previousVolume;
            onFinishEvent.Invoke();
            lockEndEvents = true;
        }
    }

    IEnumerator Typewrite()
    {
        string currentText = "";
     
        for (int i = 0; i < targetText.Length; i++)
        {

            if (isFinished)
            {
                currentText = targetText;
                gameObject.GetComponent<TMP_Text>().SetText(currentText);
                yield break;
            }
            currentText += targetText[i];
            gameObject.GetComponent<TMP_Text>().SetText(currentText);
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.volume = 0.5f;
            if(targetText[i] != '\n')audioSource.PlayOneShot(audioClip);
            yield return new WaitForSeconds(targetText[i] == '\n' ? typewriteSpeed + 0.5f : typewriteSpeed);
        }
        isFinished = true;
        yield return new WaitForSeconds(eventDelay);
        if (!lockEndEvents)
        {
            audioSource.pitch = 1f;
            audioSource.volume = previousVolume;
            onFinishEvent.Invoke();
        }
    }
    
}
