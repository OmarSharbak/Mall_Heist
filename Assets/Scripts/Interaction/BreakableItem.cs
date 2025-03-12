using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MoreMountains.Feedbacks;


public class BreakableItem : NetworkBehaviour
{
    [SerializeField] private GameObject _visual;
    [SerializeField] private GameObject _interactableItem;
    [SerializeField] private Collider _collider;
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private MMF_Player _mMF_Player;
    [SerializeField] private BreakableWindow _breakableWindow;

    public void Hit()
    {
        _collider.enabled = false;
        _breakableWindow.breakWindow();
        _renderer.enabled = false;
        _interactableItem.GetComponent<Collider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<ThirdPersonController>(out var controller))
        {
            Hit();
        }

    }
}
