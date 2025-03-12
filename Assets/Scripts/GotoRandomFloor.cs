using UnityEngine;
using MoreMountains.TopDownEngine;

public class GotoRandomFloor : MonoBehaviour
{
    /// <summary>
    /// Button click / inspector event.
    /// </summary>
    public void GetFloor()
    {
        // Get this teleporter that we interacted with.
        var teleporter = this.GetComponent<Teleporter>();

        // Get a random floor to go to.
        Room floor = MineralGameplayManager.Instance.FloorShapes[Random.Range(0, MineralGameplayManager.Instance.FloorShapes.Length)];
        while (floor.gameObject.name == this.gameObject.GetComponentInParent<Room>().gameObject.name)
        {
            floor = MineralGameplayManager.Instance.FloorShapes[Random.Range(0, MineralGameplayManager.Instance.FloorShapes.Length)];
        }
        Debug.Log(floor.gameObject.name);

        // Set the floor as the target room.
        teleporter.TargetRoom = floor;

        // Set the destination where we will spawn in the target room.
        foreach (Transform child in floor.transform)
        {
            if (child.gameObject.name == "Center")
            {
                teleporter.Destination = child.GetComponent<Teleporter>();
                break;
            }
        }

        //// Set the target room floor as active.
        //teleporter.TargetRoom.gameObject.SetActive(true);

        //// Set the current floor we are in as inactive.
        //teleporter.GetComponentInParent<Room>().gameObject.SetActive(false);
    }
}