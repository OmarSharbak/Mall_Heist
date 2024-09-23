using EPOOutline;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LockDoor : MonoBehaviour
{
    [SerializeField] List<SealableDoor> sealableDoors;
    [SerializeField] TMP_Text doorOpenedText;

    Outlinable outlinable;
    Animator animator;
    bool isOpened = false;
    MMFeedbacks mmFeedbacksOpenDoor;
    private void Start()
    {
        outlinable = GetComponent<Outlinable>();
        animator = GetComponent<Animator>();
        doorOpenedText.enabled = false; // Ensure the text is initially disabled
        mmFeedbacksOpenDoor = GameObject.Find("MMFeedbacks(opendoor)").GetComponent<MMFeedbacks>();
    }

    void Update()
    {
        if (!isOpened && AreAllDoorsSealed())
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        Debug.Log("opened");
        isOpened = true;
        animator.SetTrigger("OpenDoor");
        outlinable.FrontParameters.Color = Color.green;
        outlinable.BackParameters.Color = Color.green;

        StartCoroutine(ShowDoorOpenedText()); // Start the coroutine
    }

    IEnumerator ShowDoorOpenedText()
    {
        if (mmFeedbacksOpenDoor != null)
            mmFeedbacksOpenDoor.PlayFeedbacks();

        doorOpenedText.enabled = true; // Enable the text
        yield return new WaitForSeconds(3); // Wait for 2 seconds
        doorOpenedText.enabled = false; // Disable the text
    }

    private bool AreAllDoorsSealed()
    {
        foreach (SealableDoor door in sealableDoors)
        {
            if (!door.isSealed)
                return false;
        }
        return true;
    }
}
