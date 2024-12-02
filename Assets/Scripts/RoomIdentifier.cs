using UnityEngine;
using EmeraldAI;

public class RoomIdentifier : MonoBehaviour
{
    public string roomName;  // Set this in the inspector for each room entrance.

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Guard")
        {
            return;
        }

        // Check if the guard has entered a room entrance.
        GuardRoom room = other.GetComponent<GuardRoom>();
        if (room)
        {
            //Set guard's room to the room he entered
            room.currentRoom = roomName;
            EmeraldAIEventsManager eventsManager = other.GetComponent<EmeraldAIEventsManager>();
            if (eventsManager != null)
            {
                if(eventsManager.EmeraldComponent.CurrentTarget != null)
                EscalatorManager.Instance.AlertOtherGuards(eventsManager.EmeraldComponent.CurrentTarget.GetComponent<ThirdPersonController>() ,other.GetComponent<EmeraldAIEventsManager>());
            }
            else
            {
                Debug.Log("Events null");
            }
            
        }
    }
}
