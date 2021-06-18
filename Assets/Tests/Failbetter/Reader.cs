using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Reader : MonoBehaviour
{

    [SerializeField] private TextAsset _storyFile;

    private Story _story;

    void Start()
    {
        _story = new Story(_storyFile.text);
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
			for (int i = 0; i < _story.currentChoices.Count; ++i)
			{
				Choice choice = _story.currentChoices[i];
				Debug.Log($"{choice.condition} [{choice.isTrue}] {choice.text}");
			}
		}
		else
		{
			Debug.Log("FIN");
		}
	}
}
