using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

public class KeyBindingImageLoader : MonoBehaviour
{
	public InputActionAsset inputActions; // Your InputActionAsset
	public Image targetImage; // UI Image to display the key image
	public string actionName;

	void Start()
	{
		LoadKeyBindingImage(actionName); // Example: "Move" is the action name
	}

	void LoadKeyBindingImage(string actionName)
	{
		// Get the input action map and action
		var action = inputActions.FindActionMap("Player").FindAction(actionName);

		// Loop through all the bindings of this action
		foreach (var binding in action.bindings)
		{
			if (!binding.isComposite)
			{
				// Get the key from the binding
				string keyName = binding.ToDisplayString();

				// Generate the image name based on the key (e.g., "B_Key_flat_dark")
				string imageName = $"Images/{keyName}_Key_flat_dark"; // Customize this for your needs

				// Load the image from Resources folder
				Sprite keyImage = Resources.Load<Sprite>(imageName);

				// If the image is found, assign it to the UI Image
				if (keyImage != null)
				{
					targetImage.sprite = keyImage;
				}
				else
				{
					Debug.LogWarning("Image not found: " + imageName);
				}
			}
		}
	}
}
