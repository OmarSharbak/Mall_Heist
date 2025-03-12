using UnityEngine;
using TMPro;
using ProgressionSystem.Scripts.Variables;

public class IntVariableText : MonoBehaviour
{
    [SerializeField] private IntVariable IntVariable;
    private TextMeshProUGUI text;

    private void Awake() { text = GetComponent<TextMeshProUGUI>(); }

    private void UpdateText() { text.text = IntVariable.Value.ToString(); }

    private void OnEnable()
    {
        UpdateText();
        IntVariable.Changed += UpdateText;
    }

    private void OnDisable() { IntVariable.Changed -= UpdateText; }
}