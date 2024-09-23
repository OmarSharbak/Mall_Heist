using UnityEngine;
using EmeraldAI;

[RequireComponent(typeof(EmeraldAISystem))]
public class GuardInvincibility : MonoBehaviour
{
    EmeraldAISystem aiSystem;
    
    bool isInvincible = false;

    void Start()
    {
        aiSystem = GetComponent<EmeraldAISystem>();
    }

    public void MakeInvincible()
    {
        isInvincible = true;
    }

    public void MakeVulnerable()
    {
        isInvincible = false;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}
