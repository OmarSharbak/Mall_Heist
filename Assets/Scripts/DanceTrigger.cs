using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EmeraldAI;

public class DanceTrigger : MonoBehaviour
{

    Animator animator;
    EmeraldAISystem aiSystem;
    EmeraldAIEventsManager eventsManager;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (this.gameObject.tag == "Guard")
        {
            aiSystem = GetComponent<EmeraldAISystem>();
            eventsManager = GetComponent<EmeraldAIEventsManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (this.gameObject.tag == "Guard")
            {
                eventsManager.StopMovement();
                aiSystem.DetectionRadius = 0;
                eventsManager.ClearTarget();
                Debug.Log("GUARD");
            }
            animator.SetTrigger("Dance");
        }
    }
}
