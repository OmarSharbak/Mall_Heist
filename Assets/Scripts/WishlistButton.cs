using UnityEngine;

public class WishlistButton : MonoBehaviour
{
    public void OpenSteamPage()
    {
        // Replace the URL with your game's Steam store page URL
        Application.OpenURL("https://store.steampowered.com/app/2698040/480?utm_source=demo");
    }
}
