using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TransitionScreen : MonoBehaviour
{
	private enum TransitionState
	{
		None,
		Entering,
		Holding,
		Leaving
	}

	public static TransitionScreen instance;

	public static bool InTransition;

	public static bool InRealTransition;

	public Image TransitionImage;

	public TransitionType CurrentTransitionType;

	private TransitionState currentState;

	private float transitionAmount;

	private float holdTimer;

	public List<TransitionType> TransitionTypes = new List<TransitionType>();

	private Action onTransition;

	private float wantedHoldTime;

	public bool IsLeaving => this.currentState == TransitionState.Leaving;

	public float TransitionAmount => this.transitionAmount;

	private void Awake()
	{
		TransitionScreen.instance = this;
		foreach (TransitionType transitionType in this.TransitionTypes)
		{
			transitionType.Material = new Material(transitionType.Material);
			transitionType.Material.SetFloat("_TransitionAmount", this.transitionAmount);
		}
		this.CurrentTransitionType = this.TransitionTypes[0];
		this.TransitionImage.material = this.CurrentTransitionType.Material;
		if (TransitionScreen.InTransition)
		{
			this.transitionAmount = 1f;
			this.currentState = TransitionState.Leaving;
		}
		else
		{
			this.transitionAmount = 0f;
			this.currentState = TransitionState.None;
		}
	}

	private void Update()
	{
		TransitionScreen.InRealTransition = false;
		if (TransitionScreen.InTransition)
		{
			if (this.currentState == TransitionState.Entering)
			{
				TransitionScreen.InRealTransition = true;
				this.transitionAmount += Time.unscaledDeltaTime * this.CurrentTransitionType.TransitionSpeed;
				if (this.transitionAmount >= 1f)
				{
					this.currentState = TransitionState.Holding;
				}
			}
			else if (this.currentState == TransitionState.Holding)
			{
				TransitionScreen.InRealTransition = true;
				this.holdTimer += Time.unscaledDeltaTime;
				if (this.holdTimer >= this.wantedHoldTime)
				{
					this.transitionAmount = 1.4f;
					TransitionScreen.InRealTransition = false;
					this.currentState = TransitionState.Leaving;
					if (this.onTransition != null)
					{
						this.onTransition();
					}
				}
			}
			else if (this.currentState == TransitionState.Leaving)
			{
				TransitionScreen.InRealTransition = false;
				this.transitionAmount -= Time.unscaledDeltaTime * this.CurrentTransitionType.TransitionSpeed;
				if (this.transitionAmount <= 0f)
				{
					TransitionScreen.InTransition = false;
					this.currentState = TransitionState.None;
					this.transitionAmount = 0f;
				}
			}
		}
		foreach (TransitionType transitionType in this.TransitionTypes)
		{
			transitionType.Material.SetFloat("_TransitionAmount", this.transitionAmount);
		}
	}

	private void OnApplicationQuit()
	{
		foreach (TransitionType transitionType in this.TransitionTypes)
		{
			transitionType.Material.SetFloat("_TransitionAmount", 0f);
		}
	}

	public void StartTransition(Action onTransition, float wantedHoldTime = 0.2f)
	{
		this.StartTransition(onTransition, this.TransitionTypes[0], wantedHoldTime);
	}

	public void StartTransition(Action onTransition, string id, float wantedHoldTime = 0.2f)
	{
		TransitionType transitionType = this.TransitionTypes.FirstOrDefault((TransitionType x) => x.Id == id);
		if (transitionType == null)
		{
			Debug.LogError("No transition found with id '" + id + "'");
		}
		else
		{
			this.StartTransition(onTransition, transitionType, wantedHoldTime);
		}
	}

	private void StartTransition(Action onTransition, TransitionType transitionType, float wantedHoldTime = 0.2f)
	{
		this.CurrentTransitionType = transitionType;
		this.TransitionImage.material = this.CurrentTransitionType.Material;
		this.currentState = TransitionState.Entering;
		TransitionScreen.InTransition = true;
		this.holdTimer = 0f;
		this.wantedHoldTime = wantedHoldTime;
		this.onTransition = onTransition;
	}
}
