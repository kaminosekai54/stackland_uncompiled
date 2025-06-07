using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GreedCutscenes
{
	public static string Title
	{
		get
		{
			return WorldManager.instance.CutsceneTitle;
		}
		set
		{
			WorldManager.instance.CutsceneTitle = value;
		}
	}

	public static string Text
	{
		get
		{
			return WorldManager.instance.CutsceneText;
		}
		set
		{
			WorldManager.instance.CutsceneText = value;
		}
	}

	private static QueuedAnimation currentAnimation
	{
		set
		{
			WorldManager.instance.currentAnimation = value;
		}
	}

	private static void Stop(bool keepCameraPosition = false)
	{
		GreedCutscenes.Text = "";
		GreedCutscenes.Title = "";
		GameCamera.instance.TargetPositionOverride = null;
		GameCamera.instance.CameraPositionDistanceOverride = null;
		GameCamera.instance.TargetCardOverride = null;
		CutsceneScreen.instance.IsAdvisorCutscene = false;
		CutsceneScreen.instance.IsEndOfMonthCutscene = false;
		CutsceneScreen.instance.CheckAdvisorCutscene();
		if (keepCameraPosition)
		{
			GameCamera.instance.KeepCameraAtCurrentPos();
		}
		GameCanvas.instance.SetScreen<GameScreen>();
		GreedCutscenes.currentAnimation = null;
	}

	public static IEnumerator FinalDemandStart(Demand demand)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Vector3 randomPos = WorldManager.instance.GetRandomSpawnPosition();
		GameCamera.instance.TargetPositionOverride = randomPos;
		yield return new WaitForSeconds(2f);
		GreedCutscenes.FindOrCreateGameCard("merchant", randomPos);
		WorldManager.instance.CreateSmoke(randomPos);
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_merchant");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Royal royal = GreedCutscenes.FindOrCreateGameCard("royal", null).CardData as Royal;
		GreedCutscenes.Title = "";
		GameCamera.instance.TargetPositionOverride = royal.transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_text");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_text_2");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_text_3");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_text_4");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		DemandManager.instance.QuestStarted(demand);
	}

	public static IEnumerator FinalDemandEndSuccess(bool shouldStop)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Royal royal = GreedCutscenes.FindOrCreateGameCard("royal", null).CardData as Royal;
		DragonEgg egg = GreedCutscenes.FindOrCreateGameCard("dragon_egg", null).CardData as DragonEgg;
		GreedCutscenes.Title = "";
		GameCamera.instance.TargetPositionOverride = royal.transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GreedCutscenes.Text = "...";
		GameCamera.instance.TargetPositionOverride = egg.transform.position;
		yield return new WaitForSeconds(0.5f);
		egg.CrackedState = 1;
		AudioManager.me.PlaySound2D(egg.CrackedSound, Random.Range(1.1f, 1.3f), 0.5f);
		yield return new WaitForSeconds(0.5f);
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text_2");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = royal.transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text_3");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		GreedCutscenes.Text = "...";
		GameCamera.instance.TargetPositionOverride = egg.transform.position;
		yield return new WaitForSeconds(0.5f);
		egg.CrackedState = 2;
		AudioManager.me.PlaySound2D(egg.CrackedSound2, Random.Range(1.1f, 1.3f), 0.5f);
		yield return new WaitForSeconds(0.5f);
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text_4");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = royal.transform.position;
		AudioManager.me.PlaySound2D(DemandManager.instance.FailedDemandSound, Random.Range(1.1f, 1.3f), 0.5f);
		AngryRoyal angryRoyal = WorldManager.instance.ChangeToCard(royal.MyGameCard, "angry_royal") as AngryRoyal;
		WorldManager.instance.CreateSmoke(royal.Position);
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text_5");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = egg.transform.position;
		AudioManager.me.PlaySound2D(egg.CrackedSound2, Random.Range(1.1f, 1.3f), 0.5f);
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text_6");
		yield return new WaitForSeconds(1f);
		Combatable dragon = WorldManager.instance.ChangeToCard(egg.MyGameCard, "baby_dragon") as Combatable;
		WorldManager.instance.CreateSmoke(dragon.transform.position);
		AudioManager.me.PlaySound2D(dragon.PickupSound, Random.Range(1.1f, 1.3f), 0.4f);
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text_7");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		dragon.MyGameCard.CardAnimations.Add(new CardAnimation_FakeMeleeAttack(dragon.MyGameCard, angryRoyal.MyGameCard));
		AudioManager.me.PlaySound2D(AudioManager.me.Crit, Random.Range(0.8f, 1f), 0.1f);
		GreedCutscenes.Text = SokLoc.Translate("label_final_demand_end_text_8");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		yield return new WaitForSeconds(1f);
		AudioManager.me.PlaySound2D(AudioManager.me.Crit, Random.Range(0.8f, 1f), 0.1f);
		angryRoyal.DieInCutscene();
		GreedCutscenes.Text = "";
		yield return new WaitForSeconds(1f);
		yield return GreedCutscenes.FinalDemandLiftCurse(shouldStop);
		if (shouldStop)
		{
			GreedCutscenes.Stop();
		}
	}

	public static IEnumerator GreedWearCrown()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		GameCard spirit = GreedCutscenes.FindOrCreateGameCard("greed_spirit", null);
		GameCamera.instance.TargetPositionOverride = spirit.transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_outro_wear_crown");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		spirit.DestroyCard();
		GreedCutscenes.Stop();
	}

	public static IEnumerator NewVillager()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		GameCard targetCardOverride = GreedCutscenes.FindOrCreateGameCard("royal", null);
		GameCamera.instance.TargetCardOverride = targetCardOverride;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_new_villager");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GreedCutscenes.Text = SokLoc.Translate("label_greed_new_villager_2");
		CardData targetCardOverride2 = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "villager", faceUp: true, checkAddToStack: false);
		GameCamera.instance.TargetCardOverride = targetCardOverride2;
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
	}

	public static IEnumerator FinalDemandLiftCurse(bool shouldStop)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		CardData spirit = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "greed_spirit");
		GameCamera.instance.TargetPositionOverride = spirit.transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_lift_curse");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCard targetCardOverride = GreedCutscenes.FindOrCreateGameCard("royal_crown", null);
		GameCamera.instance.TargetCardOverride = targetCardOverride;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_lift_curse_2");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = WorldManager.instance.GetCard<Curse>().transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_lift_curse_3");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		if (spirit != null)
		{
			spirit.MyGameCard.DestroyCard();
		}
		yield return new WaitForSeconds(0.5f);
		if (shouldStop)
		{
			GreedCutscenes.Stop();
		}
	}

	public static IEnumerator KillRoyalLiftCurse()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		CardData spirit = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "greed_spirit");
		GameCamera.instance.TargetPositionOverride = spirit.transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_lift_curse_kill_royal");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCard targetCardOverride = GreedCutscenes.FindOrCreateGameCard("royal_crown", null);
		GameCamera.instance.TargetCardOverride = targetCardOverride;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_lift_curse_2");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = WorldManager.instance.GetCard<BaseVillager>().transform.position;
		GreedCutscenes.Text = SokLoc.Translate("label_greed_lift_curse_3");
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		if (spirit != null)
		{
			spirit.MyGameCard.DestroyCard();
		}
		yield return new WaitForSeconds(0.5f);
		GreedCutscenes.Stop();
	}

	public static IEnumerator StartDemand(Demand demand)
	{
		GreedCutscenes.Title = SokLoc.Translate("greed_quest_demand_title");
		foreach (GreedAnimationState questStartAnimationState in demand.QuestStartAnimationStates)
		{
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(questStartAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = gameCard.transform.position;
			}
			if (!string.IsNullOrEmpty(questStartAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = SokLoc.Translate(questStartAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(questStartAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = SokLoc.Translate(questStartAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate(questStartAnimationState.ContinueTerm));
		}
		GreedCutscenes.Text = DemandManager.instance.GetDemandStartDescription(demand);
		GameCard royal = GreedCutscenes.FindOrCreateGameCard("royal", null);
		GameCamera.instance.TargetCardOverride = royal;
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		if (demand.BlueprintIds.Any((string id) => !string.IsNullOrEmpty(id) && !WorldManager.instance.HasFoundCard(id)))
		{
			GreedCutscenes.Text = SokLoc.Translate("greed_quest_demand_description_not_found");
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			foreach (string blueprintId in demand.BlueprintIds)
			{
				CardData cardData = WorldManager.instance.CreateCard(royal.transform.position, blueprintId);
				GameCamera.instance.TargetCardOverride = cardData;
				cardData.MyGameCard.SendIt();
			}
		}
		DemandManager.instance.QuestStarted(demand);
	}

	public static IEnumerator FinishDemandSuccess(DemandEvent demandEvent)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = SokLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = SokLoc.Translate("label_demand_complete_start");
		GameCamera.instance.TargetCardOverride = GreedCutscenes.FindOrCreateGameCard("royal", null);
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		foreach (GreedAnimationState questSuccessAnimationState in demandEvent.Demand.QuestSuccessAnimationStates)
		{
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(questSuccessAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = gameCard.transform.position;
			}
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			if (!string.IsNullOrEmpty(questSuccessAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = SokLoc.Translate(questSuccessAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(questSuccessAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = SokLoc.Translate(questSuccessAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		GreedCutscenes.Text = DemandManager.instance.GetRandomSuccessDescription(demandEvent.Demand);
		GameCard gameCard2 = GreedCutscenes.FindOrCreateGameCard("royal", null);
		if (gameCard2 != null)
		{
			GameCamera.instance.TargetPositionOverride = gameCard2.transform.position;
		}
		GameCamera.instance.CameraPositionDistanceOverride = null;
		if (demandEvent.Demand.ShouldDestroyOnComplete)
		{
			GreedCutscenes.Text = "";
			float speedup = 1f;
			for (int i = 0; i < demandEvent.Demand.Amount - demandEvent.AmountGiven; i++)
			{
				CardData card = WorldManager.instance.GetCard(demandEvent.Demand.CardToGet);
				if (card != null)
				{
					GameCamera.instance.TargetPositionOverride = card.Position;
					yield return new WaitForSeconds(0.2f * speedup);
					WorldManager.instance.CreateSmoke(card.Position);
					card.MyGameCard.DestroyCard();
					yield return new WaitForSeconds(0.3f * speedup);
					speedup -= 0.1f;
					speedup = Mathf.Max(0.4f, speedup);
				}
			}
			GameCamera.instance.TargetPositionOverride = null;
			yield return new WaitForSeconds(0.5f);
			GreedCutscenes.Text = SokLoc.Translate("label_demand_collected");
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		else
		{
			CardData card2 = WorldManager.instance.GetCard(demandEvent.Demand.CardToGet);
			if (card2 != null)
			{
				GameCamera.instance.TargetCardOverride = card2;
			}
			GreedCutscenes.Text = SokLoc.Translate("label_demand_collected_2");
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		DemandManager.instance.DemandFinishedSuccess(demandEvent.Demand);
	}

	public static IEnumerator FinishDemandSuccessPreMoon(Demand demand)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = SokLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = "";
		foreach (GreedAnimationState questSuccessAnimationState in demand.QuestSuccessAnimationStates)
		{
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(questSuccessAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = gameCard.transform.position;
			}
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			if (!string.IsNullOrEmpty(questSuccessAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = SokLoc.Translate(questSuccessAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(questSuccessAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = SokLoc.Translate(questSuccessAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		GreedCutscenes.Title = SokLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = DemandManager.instance.GetRandomSuccessDescription(demand);
		GameCard gameCard2 = GreedCutscenes.FindOrCreateGameCard("royal", null);
		if (gameCard2 != null)
		{
			GameCamera.instance.TargetPositionOverride = gameCard2.transform.position;
		}
		GameCamera.instance.CameraPositionDistanceOverride = null;
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		DemandManager.instance.DemandFinishedSuccess(demand);
		GreedCutscenes.Stop();
	}

	public static IEnumerator FinishDemandFailed(Demand demand)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = SokLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = SokLoc.Translate("label_demand_complete_start");
		GameCamera.instance.TargetCardOverride = GreedCutscenes.FindOrCreateGameCard("royal", null);
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		int amountToTake = WorldManager.instance.GetCardCount((CardData x) => x.Id == demand.CardToGet);
		if (demand.ShouldDestroyOnComplete && amountToTake > 0)
		{
			GreedCutscenes.Text = "";
			float speedup = 1f;
			for (int i = 0; i < amountToTake; i++)
			{
				CardData card = WorldManager.instance.GetCard(demand.CardToGet);
				GameCamera.instance.TargetPositionOverride = card.Position;
				yield return new WaitForSeconds(0.2f * speedup);
				WorldManager.instance.CreateSmoke(card.Position);
				card.MyGameCard.DestroyCard();
				yield return new WaitForSeconds(0.3f * speedup);
				speedup -= 0.1f;
				speedup = Mathf.Max(0.4f, speedup);
			}
			GameCamera.instance.TargetPositionOverride = null;
			yield return new WaitForSeconds(0.5f);
		}
		foreach (GreedAnimationState questFailedAnimationState in demand.QuestFailedAnimationStates)
		{
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(questFailedAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = gameCard.transform.position;
			}
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			if (!string.IsNullOrEmpty(questFailedAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = SokLoc.Translate(questFailedAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(questFailedAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = SokLoc.Translate(questFailedAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		GreedCutscenes.Title = SokLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = DemandManager.instance.GetRandomFailedDescription(demand);
		GameCard gameCard2 = GreedCutscenes.FindOrCreateGameCard("royal", null);
		if (gameCard2 != null)
		{
			GameCamera.instance.TargetCardOverride = gameCard2;
		}
		GameCamera.instance.CameraPositionDistanceOverride = null;
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		AudioManager.me.PlaySound2D(DemandManager.instance.FailedDemandSound, 0.9f, 0.3f);
		if (WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Count == 0)
		{
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = SokLoc.Translate("label_greed_demand_failed_first_time");
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			yield break;
		}
		GreedCutscenes.Title = "";
		if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			GreedCutscenes.Text = SokLoc.Translate("label_greed_demand_failed_fight_peaceful");
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
			float speedup = 1f;
			int i = 0;
			int coinsToTake = 3 * DemandManager.instance.GetTimesDemandFailed();
			for (int j = 0; j < coinsToTake; j++)
			{
				CardData card = WorldManager.instance.GetCard("gold");
				if (card != null)
				{
					GameCamera.instance.TargetPositionOverride = card.Position;
					yield return new WaitForSeconds(0.2f * speedup);
					WorldManager.instance.CreateSmoke(card.Position);
					card.MyGameCard.DestroyCard();
					i++;
					yield return new WaitForSeconds(0.3f * speedup);
				}
				else
				{
					foreach (Chest chest in WorldManager.instance.GetCards<Chest>())
					{
						if (coinsToTake == i)
						{
							break;
						}
						if (chest != null && chest.CoinCount > 0)
						{
							int b = coinsToTake - i;
							int take = Mathf.Min(chest.CoinCount, b);
							if (take > 0)
							{
								GameCamera.instance.TargetPositionOverride = card.Position;
								yield return new WaitForSeconds(0.2f * speedup);
								WorldManager.instance.CreateSmoke(chest.Position);
								chest.CoinCount -= take;
								i += take;
								yield return new WaitForSeconds(0.3f * speedup);
							}
						}
					}
				}
				if (coinsToTake != i)
				{
					speedup -= 0.1f;
					speedup = Mathf.Max(0.4f, speedup);
					continue;
				}
				break;
			}
		}
		else
		{
			GreedCutscenes.Text = SokLoc.Translate("label_greed_demand_failed_fight");
			List<Combatable> source = DemandManager.instance.SpawnEnemies();
			GameCamera.instance.TargetCardOverride = source.FirstOrDefault();
			yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		}
	}

	public static IEnumerator WaitForAnswer(params string[] answers)
	{
		CutsceneScreen.instance.CreateMultipleOptions(answers);
		WorldManager.instance.ContinueClicked = false;
		while (!WorldManager.instance.ContinueClicked)
		{
			yield return null;
			if (!(GameCanvas.instance.CurrentScreen is CutsceneScreen))
			{
				GameCanvas.instance.SetScreen<CutsceneScreen>();
			}
		}
		CutsceneScreen.instance.ClearMultipleOptions();
		WorldManager.instance.ShowContinueButton = false;
	}

	public static IEnumerator WaitForContinueClicked(string text)
	{
		WorldManager.instance.ContinueClicked = false;
		WorldManager.instance.ContinueButtonText = text;
		WorldManager.instance.ShowContinueButton = true;
		while (!WorldManager.instance.ContinueClicked)
		{
			yield return null;
			if (!(GameCanvas.instance.CurrentScreen is CutsceneScreen))
			{
				GameCanvas.instance.SetScreen<CutsceneScreen>();
			}
		}
		WorldManager.instance.ShowContinueButton = false;
	}

	public static IEnumerator TryAttackRoyal(Royal royal, int tries)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = royal.transform.position;
		GreedCutscenes.Title = SokLoc.Translate("label_try_attack_royal_title");
		if (tries < 4)
		{
			GreedCutscenes.Text = SokLoc.Translate("label_try_attack_royal_description");
		}
		if (tries >= 4 && tries < 8)
		{
			GreedCutscenes.Text = SokLoc.Translate("label_try_attack_royal_description_4");
		}
		if (tries == 8)
		{
			GreedCutscenes.Text = SokLoc.Translate("label_try_attack_royal_description_8");
		}
		yield return GreedCutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GreedCutscenes.Stop();
	}

	public static GameCard FindOrCreateGameCard(string cardId, Vector3? position = null)
	{
		CardData cardData = WorldManager.instance.GetCard(cardId);
		if (cardData == null)
		{
			cardData = ((!position.HasValue) ? WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), cardId, faceUp: true, checkAddToStack: false) : WorldManager.instance.CreateCard(position.Value, cardId, faceUp: true, checkAddToStack: false));
		}
		if (cardData == null)
		{
			return null;
		}
		return cardData.MyGameCard;
	}
}
