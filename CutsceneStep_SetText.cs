using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CutsceneStep_SetText : CutsceneStep
{
	[Term]
	public string TitleTerm;

	[Term]
	public string TextTerm;

	public bool WaitForClick = true;

	[Term]
	public string ButtonTerm;

	public override IEnumerator Process()
	{
		if (CutsceneScreen.instance.IsAdvisorCutscene && base.StepIndex > 0)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.AdvisorTalking, UnityEngine.Random.Range(0.8f, 1.2f), 0.4f);
		}
		if (!string.IsNullOrEmpty(this.TitleTerm))
		{
			CutsceneStep.Title = SokLoc.Translate(this.TitleTerm);
		}
		else
		{
			CutsceneStep.Title = "";
		}
		if (!string.IsNullOrEmpty(this.TextTerm))
		{
			CutsceneStep.Text = SokLoc.Translate(this.TextTerm);
		}
		else
		{
			CutsceneStep.Text = "";
		}
		if (this.WaitForClick)
		{
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate(this.ButtonTerm));
		}
	}
}
