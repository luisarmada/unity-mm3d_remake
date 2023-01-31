using System.Collections;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class Monster : MonoBehaviour
{

    [SerializeField] private GameObject mazeGenObj;
    private MazeGenerator mazeGen;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float inViewSensitivity = 0.6f;

    [Header("State Message")]
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Color ChaseColour;
    [Space(10)]

    [Header("Sprite")]
    [SerializeField] private GameObject sprite;
    [SerializeField] private Sprite retreatSprite;
    [SerializeField] private GameObject monsterImage;

    [Header("Audio")]
    public AudioClip[] seenGrowlAudio;
    [SerializeField] private float seenGrowlDelay = 10f;
    private bool canGrowl = true;
    [SerializeField] private AudioSource audioSource;
    [Space(10)]

    [Header("AI")]
    [SerializeField] private float angerTimer = 30f; // How long to free roam until he starts hunting
    [SerializeField] private float loseChaseTimer = 20f; // How long to chase until he can be shaken off
    private float currentChaseTimer = 0f;
    private float currentAngerTimer = 0f;
    private bool firstLook = true;

    private int angerLevel; // 0 = free roam, 1 = slowFollow, 2 = hunting player, 3 = chaseLosable, 4 = chaseLock, -1 = retreat

    public float roamSpeed, huntSpeed, chaseSpeed, retreatSpeed;
    private float desiredSpeed;
    private NavMeshAgent agent;

    private int pictureTolerance;
    [HideInInspector] public int picCount = 0;
    public int minPicTolerance, maxPicTolerance;

    [SerializeField] private GameObject retreatCanvas;

    public AudioClip waitSfx, huntSfx, seenSfx, startRetreatSFX;

    private bool wentOverPicThreshold = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.baseOffset = 2.5f;
        agent.radius = 1.68f;
        agent.height = 5f;

        agent.autoRepath = false;

        agent.autoBraking = true;
        agent.angularSpeed = 150f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        agent.speed = roamSpeed;

        player.GetComponent<AudioSource>().PlayOneShot(waitSfx);
        angerLevel = 0;

        mazeGen = mazeGenObj.GetComponent<MazeGenerator>();

        pictureTolerance = Random.Range(minPicTolerance, maxPicTolerance);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();

        if (angerLevel != -1 && canGrowl && CheckIsBeingLookedAt()) // Growl when looked at
        {
            audioSource.PlayOneShot(seenGrowlAudio[seenGrowlAudio.Length - 1]);
            StartCoroutine(SeenGrowlDelay());
            StartCoroutine(CameraShake(.15f, .4f));
        }

        // Face sprite to player always
        Vector3 targetLook = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        sprite.transform.LookAt(targetLook);
    }

    private bool CheckIsBeingLookedAt()
    {
        RaycastHit hit;
        bool raycastHit = false;
        if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, Mathf.Infinity))
        {
            raycastHit = (hit.transform == player.transform);
        }

        Vector3 forwardsVectorTowardsCamera = (mainCamera.transform.position - transform.position).normalized;
        float dotProductResult = -Vector3.Dot(mainCamera.transform.forward, forwardsVectorTowardsCamera);

        return (raycastHit && !(dotProductResult < inViewSensitivity));
            
    }
    private bool seenSoundPlayed = false;
    private void UpdateState()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);

        RaycastHit hit;

        bool canSeePlayer;

        if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, Mathf.Infinity)){
            canSeePlayer = hit.transform == player.transform;
        } else canSeePlayer = false;

        // RETREAT
        if(picCount > pictureTolerance)
        {
            desiredSpeed = retreatSpeed;
            angerLevel = -1;
            agent.stoppingDistance = 4f;

            player.GetComponent<FirstPersonController>().SetSpeedState(3);

            //StartCoroutine(RetreatFromPlayer());
            if (!wentOverPicThreshold || agent.remainingDistance <= agent.stoppingDistance)
            {
                if(!wentOverPicThreshold) player.GetComponent<AudioSource>().PlayOneShot(startRetreatSFX);
                wentOverPicThreshold = true;
                Vector3 point;
                if (GetFreeRoamTarget(out point))
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                    agent.SetDestination(point);
                }
            }

            StartCoroutine(RetreatCountdown());

            agent.isStopped = true;
            agent.isStopped = false;

            stateText.color = Color.white;
            stateText.SetText("REX IS RETREATING");
            monsterImage.GetComponent<Image>().sprite = retreatSprite;
        }

        if (CheckIsBeingLookedAt() && angerLevel != -1)
        {
            if(angerLevel != 4 && angerLevel != 3)
            {
                seenSoundPlayed = false;
                angerLevel = 4;
                
            }
        } else if (canSeePlayer) // not being looked at but can see player
        {
            if (angerLevel == 0)
            {
                angerLevel = 1;
            }
        }

        // MONSTER RETREAT COMPLETE
        if(angerLevel == -1 && !canSeePlayer && !CheckIsBeingLookedAt())
        {
            stateText.SetText("REX HAS RETREATED");
            sprite.SetActive(false);
            StartCoroutine(RetreatOptions());
        }

        if (angerLevel == 0 || angerLevel == 1)
        {
            desiredSpeed = roamSpeed; // Set monster speed
            agent.autoBraking = true;

            stateText.color = Color.white;

            // Set target based on anger level. 0 = free roam, 1 = slow walk towards player
            if (angerLevel == 0)
            {
                if(agent.remainingDistance <= agent.stoppingDistance)
                {
                    Vector3 point;
                    if(GetFreeRoamTarget(out point))
                    {
                        Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                        agent.SetDestination(point);
                    }
                }

                stateText.SetText("REX LIES IN WAIT");
            }
            else
            {
                agent.SetDestination(player.transform.position);
                stateText.SetText("REX HAS SEEN YOU");
                if (!seenSoundPlayed)
                {
                    player.GetComponent<AudioSource>().PlayOneShot(seenSfx);
                    seenSoundPlayed = true;
                }
            }

            // Disable player sprint to give the sense of player sneaking around
            player.GetComponent<FirstPersonController>().SetSpeedState(0);

            // Increment time until monster starts hunting, prevents infinite searching game
            if (currentAngerTimer > angerTimer)
            {
                agent.SetDestination(player.transform.position);
                player.GetComponent<AudioSource>().PlayOneShot(huntSfx);
                angerLevel = 2;
            } else
            {
                currentAngerTimer = Mathf.Min(angerTimer + 1, currentAngerTimer + Time.deltaTime);
            }

            firstLook = true;
        }

        if(angerLevel == 2)
        {
            agent.autoBraking = false;
            desiredSpeed = huntSpeed;

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.SetDestination(player.transform.position);
            }

            player.GetComponent<FirstPersonController>().SetSpeedState(1);

            stateText.color = Color.white;
            if (!canSeePlayer)
            {
                stateText.SetText("HE IS HUNTING FOR YOU");
            } else
            {
                if(distance > 25)
                {
                    stateText.SetText("FOOTSTEPS APPROACHING");
                } else
                {
                    stateText.SetText("RUN HE IS BEHIND YOU");
                }
            }
        }

        if (angerLevel == 4 || angerLevel == 3)
        {
            agent.autoBraking = false;
            desiredSpeed = chaseSpeed;

            agent.SetDestination(player.transform.position);
            player.GetComponent<FirstPersonController>().SetSpeedState(2);

            stateText.color = ChaseColour;



            // Increment chase timer
            if (!canSeePlayer)
            {
                currentChaseTimer = Mathf.Min(loseChaseTimer + 1, currentChaseTimer + Time.deltaTime);
            }
            
            if(currentChaseTimer > loseChaseTimer)
            {
                if (!canSeePlayer)
                {
                    player.GetComponent<AudioSource>().PlayOneShot(waitSfx);
                    angerLevel = 0;
                } else
                {
                    angerLevel = 3;
                }
            }

            if (CheckIsBeingLookedAt())
            {
                if (firstLook)
                {
                    stateText.SetText("REX HAS SEEN YOU");
                    firstLook = false;
                } else
                {
                    stateText.SetText("RUN HE IS BESIDE YOU");
                }
                
            } else
            {
                if (canSeePlayer)
                {
                    stateText.SetText("RUN HE IS BEHIND YOU");
                } else
                {
                    stateText.SetText("FOOTSTEPS APPROACHING");
                }
            }
            // Decrease anger timer for when monster returns to free roam state
            currentAngerTimer = Mathf.Max(0f, currentAngerTimer - Time.deltaTime * 2);
        }
        else if (!CheckIsBeingLookedAt()) // Decrement chase timer if not being looked at, allowing for monster to be shaken off
        {
            currentChaseTimer = Mathf.Max(0f, currentChaseTimer - Time.deltaTime * 2);
            
        }

        agent.speed = Mathf.Max(desiredSpeed, Mathf.Min(desiredSpeed * 1.5f, desiredSpeed * distance /30));
        Debug.Log("Speed: " + agent.speed);
    }

    IEnumerator RetreatOptions()
    {
        stateText.SetText("REX HAS RETREATED");
        player.GetComponent<AudioSource>().PlayOneShot(waitSfx);
        yield return new WaitForSeconds(2);
        stateText.SetText("REX HAS RETREATED");
        retreatCanvas.SetActive(true);
        StopAllCoroutines();
        player.GetComponent<FirstPersonController>().canLoadNextLevel = true;
        Destroy(gameObject);
    }

    IEnumerator RetreatCountdown()
    {
        yield return new WaitForSeconds(10f);
        StartCoroutine(RetreatOptions());
    }

    private bool GetFreeRoamTarget(out Vector3 result)
    {

        Vector3 randomPoint = new Vector3(mazeGen.getGridLength() * 5f, 0, mazeGen.getGridWidth() * 5f) + Random.insideUnitSphere * Mathf.Max(mazeGen.getGridLength() * 5f, mazeGen.getGridWidth() * 5f);
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 startPosition = mainCamera.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.localPosition = new Vector3(x, y, startPosition.z);
            yield return null;
        }

        mainCamera.transform.localPosition = startPosition;

    }

    /*private Collider[] results = new Collider[10];

    public LayerMask hidableLayers;
    public float hideSensitivity = 0;
    private IEnumerator RetreatFromPlayer()
    {
        while (true)
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, 30f, results, hidableLayers);

            for(int i=0; i<hits; i++)
            {
                if (NavMesh.SamplePosition(results[i].transform.position, out NavMeshHit hit, 10f, agent.areaMask))
                {
                    if(!NavMesh.FindClosestEdge(hit.position, out hit, agent.areaMask)) // to find normal for use in dot product, to determine which direction edge is facing
                    {
                        Debug.LogError($"Unable to find edge close to {hit.position}");
                        break;
                    }

                    if (Vector3.Dot(hit.normal, (player.transform.position - hit.position).normalized) < hideSensitivity)
                    {
                        agent.SetDestination(hit.position);
                        break;
                    }
                    else
                    {
                        // since previous spot wasnt facing away from player, try other side of object
                        if (NavMesh.SamplePosition(results[i].transform.position - (player.transform.position - hit.position).normalized * 2, out NavMeshHit hit2, 10f, agent.areaMask))
                        {
                            if (!NavMesh.FindClosestEdge(hit2.position, out hit2, agent.areaMask)) // to find normal for use in dot product, to determine which direction edge is facing
                            {
                                Debug.LogError($"Unable to find edge close to {hit2.position}");
                            }

                            if (Vector3.Dot(hit2.normal, (player.transform.position - hit2.position).normalized) < hideSensitivity)
                            {
                                agent.SetDestination(hit2.position);
                                break;
                            }
                        }
                    }
                } else
                {
                    Debug.LogError($"Unable to find NavMesh near object");
                    break;
                }
            }
            yield return null;
        }
    }*/

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == player)
        {
            player.GetComponent<FirstPersonController>().SetSpeedState(3);
            agent.isStopped = true;
            Debug.Log("AHHHH");
        }
    }

    IEnumerator SeenGrowlDelay()
    {
        canGrowl = false;
        yield return new WaitForSeconds(seenGrowlDelay);
        canGrowl = true;
    }

}
