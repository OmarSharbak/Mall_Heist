using UnityEngine;
using UnityEngine.InputSystem;

public class InputSchemeChecker : MonoBehaviour
{
    private PlayerInput playerInput;

    [HideInInspector]
    public string currentScheme;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        currentScheme = playerInput.currentControlScheme;
    }
}
