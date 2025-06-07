using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableLabelCardopedia : MonoBehaviour
{
	public CustomButton MyButton;

	public Image PlusImage;

	public TextMeshProUGUI LabelText;

	public Sprite PlusSprite;

	public Sprite MinusSprite;

	public List<CardopediaEntryElement> Children = new List<CardopediaEntryElement>();

	public bool IsExpanded = true;

	public object Tag;

	public event Action OnExpand;

	public void SetText(string text)
	{
		this.LabelText.text = text;
	}

	public void SetCallback(Action callback)
	{
		this.OnExpand += callback;
	}

	private void Start()
	{
		this.MyButton.Clicked += delegate
		{
			this.SetExpanded(!this.IsExpanded);
			this.OnExpand?.Invoke();
		};
		this.MyButton.SetColor = false;
	}

	public void SetExpanded(bool expanded)
	{
		this.IsExpanded = expanded;
		foreach (CardopediaEntryElement child in this.Children)
		{
			child.IsEnabled = expanded;
		}
	}

	public void ShowChildrenCardopedia()
	{
		foreach (CardopediaEntryElement child in this.Children)
		{
			if (!CardopediaScreen.instance.IsSearching)
			{
				child.IsEnabled = this.IsExpanded && child.IsFilteredUpdate;
			}
			else
			{
				child.IsEnabled = child.wasFound && child.IsFiltered && this.IsExpanded;
			}
		}
	}

	private void Update()
	{
		this.PlusImage.sprite = ((!this.IsExpanded) ? this.PlusSprite : this.MinusSprite);
	}
}
