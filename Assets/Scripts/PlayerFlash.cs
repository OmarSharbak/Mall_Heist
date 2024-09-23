using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlash : MonoBehaviour
{
    [SerializeField] float flashDuration = 1f; // Duration of flashing effect
    [SerializeField] float flashSpeed = 0.1f; //Speed of flashes
    [SerializeField] SkinnedMeshRenderer meshRenderer;

    private bool isInvincible = false;

    private IEnumerator FlashEffect()
    {
        //Set invincibilty
        isInvincible = true;
        float flashTime = flashDuration;

        while(flashTime > 0f)
        {
            //Toggle Visiblity

            meshRenderer.enabled = false;
            yield return new WaitForSeconds(flashSpeed);
            meshRenderer.enabled = true;
            yield return new WaitForSeconds(flashSpeed);

            // Reduce the pending flash time
            flashTime -= flashSpeed * 2;
        }

        //Reset invincibilty
        isInvincible = false;
    }

    public void TriggerFlash()
    {
        StartCoroutine(FlashEffect());
    }
}
