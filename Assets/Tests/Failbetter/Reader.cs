using Ink;
using Ink.Parsed;
using Ink.Runtime;
using Ink.UnityIntegration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


public class Reader : MonoBehaviour
{
    [SerializeField] private TextAsset _storyFile;

    private Ink.Runtime.Story _story;

    void Start()
    {
        _story = new Ink.Runtime.Story(_storyFile.text);
		
		Read();

		TestCondition();
	}

	void TestCondition()
	{
		string condition = "testFunction(\"hello\") && (T2 + 2 >= 7)";

		InkParser p = new InkParser(condition);
		Expression rootExpression = p.Expression();
		Debug.Log($"{condition} => {_story.EvaluateAtRuntime(rootExpression)}");
	}

    void Read()
    {
		while (_story.canContinue)
		{
			Debug.Log(_story.Continue() + JoinTags(_story.currentTags));
		}

		if (_story.currentChoices.Count > 0)
		{
			for (int i = 0; i < _story.currentChoices.Count; ++i)
			{
				Ink.Runtime.Choice choice = _story.currentChoices[i];
				Debug.Log($"{choice.condition} [{choice.isTrue}] {choice.text}" + JoinTags(choice.tags));
			}
			Debug.Log("Select 2 >");
			_story.ChooseChoiceIndex(1);
			Read();
		}
		else
		{
			Debug.Log("FIN");
		}

	}

	string JoinTags(List<string> tags)
	{
		if (tags == null || tags.Count <= 0)
		{
			return "";
		}

		return " - Tags: " + string.Join(", ", tags);
	}
}
