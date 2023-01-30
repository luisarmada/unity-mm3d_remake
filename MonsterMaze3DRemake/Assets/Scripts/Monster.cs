using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{

    [SerializeField] private GameObject player;

    [SerializeField] private TMP_Text stateText;
    [SerializeField] private GameObject sprite;

    [SerializeField] private int farDistance, midDistance;

    private bool canSeePlayer;

    [SerializeField] private float moveForce;


    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float runMoveSpeed;

    [SerializeField] private GameObject mazeGenObj;

    [SerializeField] private Color ChaseColour;


    public float movementSpeed;

    public AudioClip[] seenGrowlAudio;
    [SerializeField] private float seenGrowlDelay = 10f;
    private bool canGrowl = true;
    [SerializeField] private AudioSource audioSource;

    private bool isBeingLookedAt = false;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float inViewSensitivity = 0.6f;


    // Start is called before the first frame update
    void Start()
    {

        gameObject.AddComponent<NavMeshAgent>();
        gameObject.GetComponent<NavMeshAgent>().baseOffset = 2.5f;
        gameObject.GetComponent<NavMeshAgent>().radius = 1.68f;
        gameObject.GetComponent<NavMeshAgent>().height = 5f;

        gameObject.GetComponent<NavMeshAgent>().autoBraking = false;
        gameObject.GetComponent<NavMeshAgent>().angularSpeed = 150f;
        gameObject.GetComponent<NavMeshAgent>().obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        gameObject.GetComponent<NavMeshAgent>().speed = movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if(CheckIsBeingLookedAt()){
            if (canGrowl)
            {
                audioSource.PlayOneShot(seenGrowlAudio[seenGrowlAudio.Length - 1]);
                StartCoroutine(SeenGrowlDelay());
            }
        }
        UpdateState();
        gameObject.GetComponent<NavMeshAgent>().SetDestination(player.transform.position);
        player.GetComponent<FirstPersonController>().SetSprint(true);
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

    private void UpdateState()
    {
        if (player != null)
        {
            Vector3 targetLook = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            sprite.transform.LookAt(targetLook);
            float distance = Vector3.Distance(transform.position, player.transform.position);

            RaycastHit hit;

            if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, Mathf.Infinity))
            {
                if (hit.transform == player.transform)
                {
                    canSeePlayer = true;
                    if (distance > farDistance)
                    {
                        stateText.SetText("FOOTSTEPS APPROACHING");
                        stateText.color = ChaseColour;
                    }
                    else if (distance > midDistance)
                    {
                        stateText.SetText("REX HAS SEEN YOU");
                        stateText.color = ChaseColour;
                    }
                    else
                    {
                        stateText.SetText("REX IS BEHIND YOU");
                        stateText.color = ChaseColour;
                    }
                    
                }
                else
                {
                    canSeePlayer = false;
                    if (distance > farDistance)
                    {
                        stateText.SetText("REX LIES IN WAIT");
                        stateText.color = Color.white;
                    }
                    else
                    {
                        stateText.SetText("REX IS HUNTING FOR YOU");
                        stateText.color = Color.white;
                    }
                }
            }
            else
            {
                canSeePlayer = false;
                if (distance > farDistance)
                {
                    stateText.SetText("REX LIES IN WAIT");
                    stateText.color = Color.white;
                }
                else
                {
                    stateText.SetText("REX IS HUNTING FOR YOU");
                    stateText.color = Color.white;
                }
            }
        }
    }

    IEnumerator SeenGrowlDelay()
    {
        canGrowl = false;
        yield return new WaitForSeconds(seenGrowlDelay);
        canGrowl = true;
    }

}
