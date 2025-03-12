using UnityEngine;
using System.Collections.Generic;

public class SkinsChanger : MonoBehaviour
{
    public List<GameObject> skins = new List<GameObject>();
    private int currentSkinIndex = 0;

    private void Start()
    {
        // Ensure only the first skin is active at start
        for (int i = 1; i < skins.Count; i++)
        {
            skins[i].SetActive(false);
        }
        skins[0].SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeSkin();
        }
    }

    private void ChangeSkin()
    {
        // Deactivate the current skin
        skins[currentSkinIndex].SetActive(false);

        // Move to the next skin in the list, wrapping around if at the end
        currentSkinIndex = (currentSkinIndex + 1) % skins.Count;

        // Activate the new skin
        skins[currentSkinIndex].SetActive(true);
    }
}
