using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{

    [SerializeField] private GameObject player;
    public LayerMask playerMask;

    [SerializeField] TMP_Text stateText;
    [SerializeField] GameObject sprite;

    [SerializeField] private int farDistance, midDistance;

    private bool canSeePlayer;
    private Rigidbody rigidBody;

    [SerializeField] private float moveForce;

    private float maxMoveSpeed;

    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float runMoveSpeed;

    private (int cx, int cy) currentPos;
    [SerializeField] private GameObject mazeGenObj;

    private (int u, int r) currentMovement;

    public float movementSpeed;



    // Start is called before the first frame update
    void Start()
    {
        maxMoveSpeed = baseMoveSpeed;

        gameObject.AddComponent<NavMeshAgent>();
        gameObject.GetComponent<NavMeshAgent>().baseOffset = 2.5f;
        gameObject.GetComponent<NavMeshAgent>().radius = 1.68f;
        gameObject.GetComponent<NavMeshAgent>().height = 5f;

        gameObject.GetComponent<NavMeshAgent>().autoBraking = false;
        gameObject.GetComponent<NavMeshAgent>().angularSpeed = 150f;
        gameObject.GetComponent<NavMeshAgent>().obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        gameObject.GetComponent<NavMeshAgent>().speed = movementSpeed;
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState(); // Sets state text and checks if monster can see player
        if (canSeePlayer)
        {
            maxMoveSpeed = runMoveSpeed;
        } else
        {
            maxMoveSpeed = baseMoveSpeed;
            //if(!stateSwitch) FreeRoam();
            //stateSwitch = true;
        }
        gameObject.GetComponent<NavMeshAgent>().SetDestination(player.transform.position);
        player.GetComponent<FirstPersonController>().SetSprint(true);
    }

    private void UpdateState()
    {
        if (player != null)
        {
            sprite.transform.LookAt(player.transform);
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
                    }
                    else if (distance > midDistance)
                    {
                        stateText.SetText("REX HAS SEEN YOU");
                    }
                    else
                    {
                        stateText.SetText("REX IS BEHIND YOU");
                    }
                }
                else
                {
                    canSeePlayer = false;
                    if (distance > farDistance)
                    {
                        stateText.SetText("REX LIES IN WAIT");
                    }
                    else
                    {
                        stateText.SetText("REX IS HUNTING FOR YOU");
                    }
                }
            }
            else
            {
                canSeePlayer = false;
                if (distance > farDistance)
                {
                    stateText.SetText("REX LIES IN WAIT");
                }
                else
                {
                    stateText.SetText("REX IS HUNTING FOR YOU");
                }
            }
        }
    }


    public void SetCurrentPos((int x, int y) t)
    {
        currentPos = t;
    }
}
