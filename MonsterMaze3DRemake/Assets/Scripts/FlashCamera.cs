using System.Collections;
using TMPro;
using UnityEngine;

public class FlashCamera : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private GameObject flashLight;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float picDelay = 8f;
    private bool canTakePicture = true;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cameraFlashClip;

    [SerializeField] TMP_Text scoreText, multiplierText;

    [SerializeField] private GameObject monster, mainCamera;

    [SerializeField] private float inViewSensitivity = 0.6f;
    [SerializeField] private float centeredShotSentivity = 0.95f;

    [SerializeField] private float closeShotDistance = 0.95f;

    private int score = 0;

    private Vector3 lastPicPosition = new Vector3(-999, -999, -999);
    [SerializeField] private float minDistanceFromLastPos = 30f;

    void Start()
    {
        scoreText.SetText("$" + score);
        multiplierText.SetText("");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canTakePicture)
        {
            int picScore = scoreCalculateFromDistance(monster);
            StartCoroutine(PictureFlash(picScore));
            audioSource.PlayOneShot(cameraFlashClip);
        }
        
    }

    private int scoreCalculateFromDistance(GameObject target)
    {
        RaycastHit hit;

        Vector3 forwardsVectorTowardsCamera = (mainCamera.transform.position - monster.transform.position).normalized;
        float dotProductResult = -Vector3.Dot(mainCamera.transform.forward, forwardsVectorTowardsCamera);

        if(dotProductResult < inViewSensitivity)
        {
            return 0;
        }



        float distanceFromLastPos = Vector3.Distance(transform.position, lastPicPosition);
        Debug.Log(dotProductResult);

        if(distanceFromLastPos < minDistanceFromLastPos)
        {
            return -1;
        }


        if (Physics.Raycast(monster.transform.position, (transform.position - monster.transform.position), out hit, Mathf.Infinity))
        {
            if (hit.transform == transform)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                bool stillShot = GetComponent<FirstPersonController>().isStandingStill;
                

                lastPicPosition = transform.position;

                bool closeShot = distance < closeShotDistance;
                bool centeredShot = dotProductResult > centeredShotSentivity;

                if(stillShot && closeShot && centeredShot)
                {
                    multiplierText.SetText("PERFECT SHOT!");
                }else if(!stillShot && closeShot && centeredShot)
                {
                    multiplierText.SetText("GOOD SHOT!");
                } else if(stillShot && !closeShot && centeredShot)
                {
                    multiplierText.SetText("GREAT SHOT!");
                }
                else if (stillShot && closeShot && !centeredShot)
                {
                    multiplierText.SetText("NICE SHOT!");
                }
                else if (stillShot && !closeShot && !centeredShot)
                {
                    multiplierText.SetText("STEADY SHOT!");
                }
                else if (!stillShot && closeShot && !centeredShot)
                {
                    multiplierText.SetText("CLOSE-UP SHOT!");
                }
                else if (!stillShot && !closeShot && centeredShot)
                {
                    multiplierText.SetText("CENTERED SHOT!");
                }

                

                float standingStillMultiplier = stillShot ? dotProductResult - 0.15f : 0.3f * dotProductResult;
                float closeShotMultiplier = closeShot ? distance * 2.5f : distance * 8f;
                float centeredShotMultiplier = centeredShot ? (dotProductResult * dotProductResult) : (dotProductResult * dotProductResult) * 0.6f;

                return Mathf.RoundToInt((1000 - closeShotMultiplier) * centeredShotMultiplier * standingStillMultiplier * (1f * 0.33f));
            } else
            {
                return 0;
            }
        } else
        {
            return 0;
        }
    }

    IEnumerator PictureFlash(int addScore)
    {
        canTakePicture = false;
        
        scoreText.SetText(addScore > 0 ? "+" + addScore + "$" : (addScore == -1 ? "PICTURE IS TOO SIMILAR!": "REX NOT IN FRAME!"));
        addScore = Mathf.Max(0, addScore);
        if(addScore == 0) multiplierText.SetText("");
        score += addScore;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            flashLight.GetComponent<Light>().intensity = strength;
            yield return null;
        }
        multiplierText.GetComponent<Animator>().SetBool("visible", true); // fade out multiplier text

        scoreText.SetText("CAMERA RELOADING.");
        yield return new WaitForSeconds(picDelay/3);
        scoreText.SetText("CAMERA RELOADING..");
        yield return new WaitForSeconds(picDelay / 3);
        scoreText.SetText("CAMERA RELOADING...");
        yield return new WaitForSeconds(picDelay / 3);
        scoreText.SetText(score + "$");

        multiplierText.SetText("");
        multiplierText.GetComponent<Animator>().SetBool("visible", false);

        canTakePicture = true;
        
    }
}
