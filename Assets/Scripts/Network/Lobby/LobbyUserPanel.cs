using HeathenEngineering.SteamworksIntegration;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUserPanel : MonoBehaviour
{

	[SerializeField] private RawImage avatarImage;
	[SerializeField] private TextMeshProUGUI nameText;
	public void Initialize(UserData userData)
	{
		userData.LoadAvatar(SetAvatar);
		nameText.text = userData.Name;
	}

	private void SetAvatar( Texture2D userImage)
	{
		avatarImage.texture = userImage;
	}
}
