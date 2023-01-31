using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TextTypewrite : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string targetText;
    [SerializeField] private float typewriteSpeed = 0.06f;
    [SerializeField] private float eventDelay = 2f;
    [SerializeField] private UnityEvent onFinishEvent;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Typewrite());
    }

    IEnumerator Typewrite()
    {
        string currentText = "";
     
        for (int i = 0; i < targetText.Length; i++)
        {
            currentText += targetText[i];
            gameObject.GetComponent<TMP_Text>().SetText(currentText);
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.volume = 0.5f;
            if(targetText[i] != '\n')audioSource.PlayOneShot(audioClip);
            yield return new WaitForSeconds(targetText[i] == '\n' ? typewriteSpeed + 0.5f : typewriteSpeed);
        }
        yield return new WaitForSeconds(eventDelay);
        audioSource.pitch = 1f;
        audioSource.volume = 1f;
        onFinishEvent.Invoke();
    }
    
}
