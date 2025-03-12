using UnityEngine;
using TMPro;
using ProgressionSystem.Scripts.Variables;

public class IntVariableTextExt : MonoBehaviour
{
    [SerializeField] private string prefixText;
    [SerializeField] private IntVariable IntVariable;
    private TextMeshProUGUI text;

    private void Awake() { text = GetComponent<TextMeshProUGUI>(); }

    private void UpdateText() { text.text = $"{prefixText}{IntVariable.Value}"; }

    private void OnEnable()
    {
        UpdateText();
        IntVariable.Changed += UpdateText;
    }

    private void OnDisable() { IntVariable.Changed -= UpdateText; }
}