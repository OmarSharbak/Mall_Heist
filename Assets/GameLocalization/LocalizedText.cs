using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    void OnEnable()
    {
        FontManager.Instance?.RegisterText(GetComponent<TMP_Text>());
    }

    void OnDisable()
    {
        FontManager.Instance?.UnregisterText(GetComponent<TMP_Text>());
    }
}
