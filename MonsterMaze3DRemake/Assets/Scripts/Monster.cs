using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Monster : MonoBehaviour
{

    [SerializeField] private GameObject player;
    public LayerMask playerMask;

    [SerializeField] TMP_Text stateText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            transform.LookAt(player.transform);

            Vector3 pos = transform.position;
            Vector3 dir = (player.transform.position - transform.position).normalized;
            Debug.DrawLine(pos, pos + dir * 10, Color.red, Mathf.Infinity);
            if (Physics.Raycast(pos, pos + dir * 10, out var hit, Mathf.Infinity, playerMask))
            {
                Debug.Log("Hello");
            }
        }
    }
}
