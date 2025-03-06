using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System;

public class InputRebindUI : MonoBehaviour
{
	public InputActionAsset inputActions; // Assign in Inspector
	public GameObject rowPrefab; // Assign a prefab with Text + Button
	public Transform tableParent; // Assign the UI panel/parent
	public ScrollRect scrollRect; // Reference to the ScrollRect component

	void Start()

	{
		LoadRebinds();
		GenerateTable();
		SetScrollViewAtStart();
	}
	void SetScrollViewAtStart()
	{
		// Set the horizontal scroll to 0 (left) and vertical scroll to 1 (top)
		scrollRect.verticalNormalizedPosition = 1f; // 1 is the top of the scroll view
	}
	void GenerateTable()
	{
		foreach (var actionMap in inputActions.actionMaps)
		{
			Debug.Log("action map" + actionMap.name);
			foreach (var action in actionMap.actions)
			{
				Debug.Log("action" + action.name);

				foreach (InputBinding binding in action.bindings)
				{
					if (!binding.isPartOfComposite)
					{

						// If the binding is one of the keys for movement (W, A, S, D)
						if (binding.path.Contains("Keyboard"))
						{

							GameObject row = Instantiate(rowPrefab, tableParent);
							TextMeshProUGUI actionText = row.transform.Find("ActionName").GetComponent<TextMeshProUGUI>();
							Button rebindButton = row.transform.Find("RebindButton").GetComponent<Button>();
							TextMeshProUGUI keyText = row.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();

							// Set the action name to the key binding (W, A, S, D)
							actionText.text = action.name; // This will display "W", "A", "S", "D"

							// Set the key display string
							keyText.text = binding.ToDisplayString();

							// Add rebind functionality to the button
							rebindButton.onClick.AddListener(() => StartRebinding(rebindButton,action, keyText));
						}
					}
				}
			}
		}
	}

	void StartRebinding(Button button,InputAction action, TextMeshProUGUI keyText)
	{
		button.GetComponentInChildren<TextMeshProUGUI>().text = "Listening...";
		action.Disable();
		action.PerformInteractiveRebinding()
			.WithControlsExcluding("Mouse") // Exclude mouse if needed
			.OnComplete(operation =>
			{
				action.Enable();
				keyText.text = action.bindings[0].ToDisplayString();
				operation.Dispose();
				SaveRebinds();
				button.GetComponentInChildren<TextMeshProUGUI>().text = "Rebind Key";

			})
			.Start();
	}

	void SaveRebinds()
	{
		string rebinds = inputActions.SaveBindingOverridesAsJson();
		ES3.Save("rebinds", rebinds);
	}

	void LoadRebinds()
	{
		if (ES3.KeyExists("rebinds"))
			inputActions.LoadBindingOverridesFromJson(ES3.Load<string>("rebinds"));
	}

	public void ResetAllRebinds()
	{
		// Loop through all actions in the Player action map
		foreach (var actionMap in inputActions.actionMaps)
		{
			Debug.Log("action map" + actionMap.name);
			foreach (var action in actionMap.actions)
			{
				Debug.Log("action" + action.name);

				foreach (InputBinding binding in action.bindings)
				{
					if (!binding.isPartOfComposite)
					{
						// Remove all custom key bindings for the action
						action.RemoveAllBindingOverrides();

						// Re-enable the action if needed (in case it was disabled)
						action.Enable();
					}
				}
			}
		}

		// Optionally, save the default bindings again if you are persisting the changes
		SaveRebinds();  // Save the defaults if you're saving them to a file or player preferences.

		ClearTable();
		LoadRebinds();
		GenerateTable();
		SetScrollViewAtStart();
	}

	private void ClearTable()
	{
		tableParent.ClearChildren();
	}
}
