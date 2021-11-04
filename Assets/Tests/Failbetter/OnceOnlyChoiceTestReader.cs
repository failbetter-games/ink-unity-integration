using Ink;
using Ink.Parsed;
using Ink.Runtime;
using Ink.UnityIntegration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


public class OnceOnlyChoiceTestReader : MonoBehaviour
{
    [SerializeField] private TextAsset _storyFile;

    private Ink.Runtime.Story _story;

    void Start()
    {
        _story = new Ink.Runtime.Story(_storyFile.text);
		_story.BindExternalFunction("TestUnityFunction", () => { return TestUnityFunction(); });
		_story.onChoiceCreated += OnChoiceCreated;

		Read();
		_story.ChooseChoiceIndex(1);
		Debug.Log("-------- CHOICE CHOSEN --------");
		Read();
	}

    void Read()
    {
		while (_story.canContinue)
		{
			Debug.Log(_story.Continue());
		}

		if (_story.currentChoices.Count > 0)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < _story.currentChoices.Count; ++i)
			{
				Ink.Runtime.Choice choice = _story.currentChoices[i];
				sb.Append($"CHOICE: {choice.condition} [{choice.isTrue}] {choice.text}\n");
			}
			Debug.Log(sb.ToString());
		}
		else
		{
			Debug.Log("FIN");
		}

	}

	void OnChoiceCreated(Ink.Runtime.Choice choice)
	{
		if (choice == null)
        {
			Debug.Log($"[CHOICE CREATED] NULL CHOICE");
		} else
		{
			Debug.Log($"[CHOICE CREATED] {choice.text}");
		}
	}

	bool TestUnityFunction()
    {
		Debug.Log("[TEST CALLED]");
		return true;
    }
}
