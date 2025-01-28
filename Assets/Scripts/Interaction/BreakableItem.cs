using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MoreMountains.Feedbacks;


public class BreakableItem : NetworkBehaviour
{
    [SerializeField] private GameObject _visual;
    [SerializeField] private GameObject _interactableItem;
    [SerializeField] private Collider _collider;
    [SerializeField] private ParticleSystem _breakEffect;
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private MMF_Player _mMF_Player;

    public void Hit()
    {
        _renderer.enabled = false;
        _collider.enabled = false;

        if (_breakEffect != null)
            _breakEffect.gameObject.SetActive(true);

        _mMF_Player.PlayFeedbacks();
        _interactableItem.SetActive(true);
    }
}
