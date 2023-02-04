using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FirstPersonController : MonoBehaviour
{

    [SerializeField] private Transform mainCamera;

    [SerializeField] private float sensitivity;
    private Rigidbody rigidBody;
    [SerializeField] private float moveForce;

    private float maxMoveSpeed;

    [SerializeField] private float baseMoveSpeed, huntMoveSpeed, runMoveSpeed;
    private float pitch;
    private float yaw;

    public bool isStandingStill;
    [SerializeField] private float standingStillTimer = 1f;

    public bool canLoadNextLevel = false;

    private int moveSpeedMult;


    // Start is called before the first frame update
    void Start()
    {
        maxMoveSpeed = baseMoveSpeed;
        rigidBody = gameObject.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveSpeedMult = PlayerPrefs.GetInt("MovementSpeed");
    }

    private void Update()
    {
        if (canLoadNextLevel)
        {
            if(Input.GetKeyDown(KeyCode.Y)) {
                SceneManager.LoadScene("GameScene");
            }
        }
        Debug.Log("Money: " + PlayerPrefs.GetInt("MoneyEarned"));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        yaw += Input.GetAxisRaw("Mouse X") * sensitivity;
        pitch -= Input.GetAxisRaw("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.localRotation = Quaternion.Euler(0, yaw, 0);
        mainCamera.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    private void FixedUpdate()
    {
        Vector3 inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        rigidBody.AddRelativeForce(inputVector.normalized * moveForce);

        Vector3 vel = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
        rigidBody.AddForce(-vel * (moveForce / maxMoveSpeed), ForceMode.Acceleration);

        if(Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            //StartCoroutine(standingStillCheck());
            isStandingStill = true;
        } else
        {
            isStandingStill = false;
        }
    }

    IEnumerator standingStillCheck()
    {
        isStandingStill = false;
        yield return new WaitForSeconds(standingStillTimer);
        isStandingStill = true;
    }

    public void SetSpeedState(int state)
    {
        if (state == 0) maxMoveSpeed = baseMoveSpeed * moveSpeedMult;
        else if (state == 1) maxMoveSpeed = huntMoveSpeed * moveSpeedMult;
        else if (state == 2) maxMoveSpeed = runMoveSpeed * moveSpeedMult;
        else moveForce = 0;
    }

}
