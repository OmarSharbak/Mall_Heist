using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;
using MoreMountains.Feedbacks;

public class Register : MonoBehaviour
{
    PlayerDamageHandler playerDamageHandler;
    ThirdPersonController thirdPersonController;
    AudioSource audioSource;
    Outlinable outlinable;

    public int moneyAmount = 10;

    MMFeedbacks mmFeedbacksSteal;

    bool playerIsNear = false;
    public bool interacted = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        outlinable = GetComponent<Outlinable>();
        mmFeedbacksSteal = GameObject.Find("MMFeedbacks(steal)").GetComponent<MMFeedbacks>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
        {
            playerIsNear = true;
            playerDamageHandler = other.GetComponent<PlayerDamageHandler>();
            thirdPersonController = other.GetComponent<ThirdPersonController>();
            thirdPersonController.SetNearbyRegister(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerInvisible"))
        {
            playerIsNear = false;
            playerDamageHandler = null;
            thirdPersonController.ClearNearbyRegister();
            thirdPersonController = null; 
        }
    }

    public void Interact()
    {
        if(!interacted && playerIsNear)
        {
            if(playerDamageHandler != null)
            {
                Debug.Log("Money added.");
                interacted = true;
                audioSource.Play();

                if (mmFeedbacksSteal != null)
                    mmFeedbacksSteal.PlayFeedbacks();

                playerDamageHandler.AddMoney(moneyAmount);
                EscalatorManager.Instance.moneyCollected += moneyAmount;
                PopupTextManager.Instance.ShowPopupText("+ " + moneyAmount);
                outlinable.enabled = false;
            }
        }
    }
}
