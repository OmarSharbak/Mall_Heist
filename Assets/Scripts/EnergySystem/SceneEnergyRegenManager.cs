using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEnergyRegenManager : MonoBehaviour
{
    public bool RegenOnStart = false;
    [SerializeField] private GameObject regenText;


    //----------------------------------------------------------------
    private void Start()
    {
        regenText.SetActive(false);

        if (RegenOnStart) StartRegen(); else StopRegen();
    }


    //----------------------------------------------------------------
    private void Update()
    {
        if (ScenesManager.Instance.MainMenuScene.SceneName == SceneManager.GetActiveScene().name)
            HideRegenTextOnFullHealth();
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Button click.
    /// </summary>
    public void StartRegen()
    {
        EnergySystem.Instance.RegenToggle(true);
        regenText.SetActive(true);
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Button click.
    /// </summary>
    public void StopRegen()
    {
        EnergySystem.Instance.RegenToggle(false);
        regenText.SetActive(false);
    }


    //----------------------------------------------------------------
    private void HideRegenTextOnFullHealth()
    {
        if (EnergySystem.Instance.FullHealth) regenText.SetActive(false);
        else regenText.SetActive(true);
    }
}