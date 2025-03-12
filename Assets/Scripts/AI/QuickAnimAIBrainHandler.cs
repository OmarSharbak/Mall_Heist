using System.Collections;
using UnityEngine;
using RedLabsGames.Tools.QuickAnim;

public class QuickAnimAIBrainHandler : MonoBehaviour
{
    private QuickAnimComponent quickAnim;


    //----------------------------------------------------------------
    private void Awake()
    {
        quickAnim = this.GetComponent<QuickAnimComponent>();
    }


    //----------------------------------------------------------------
    public void PlayAnimationInAutoDelayOut()
    {
        quickAnim.Play("In");
        StartCoroutine(DelayOutRoutine());
    }


    //----------------------------------------------------------------
    private IEnumerator DelayOutRoutine()
    {
        yield return new WaitForSeconds(1);

        quickAnim.Play("Out");
    }
}