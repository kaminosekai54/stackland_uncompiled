using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Combatable : CardData
{
	[Header("Combat")]
	public bool CanHaveInventory;

	public bool CanAttack = true;

	public List<string> PossibleEquipableIds = new List<string>();

	public bool InheritCombatStatsFromOtherCard;

	[Card]
	public string InheritCombatStatsFrom;

	public AttackType BaseAttackType;

	public CombatStats BaseCombatStats;

	private string _combatableDescription;

	[ExtraData("health")]
	public int HealthPoints = 3;

	private int previouseHealthPoints;

	[ExtraData("attack_timer")]
	[HideInInspector]
	public float AttackTimer;

	[HideInInspector]
	public bool BeingAttacked;

	[HideInInspector]
	public float StunTimer;

	protected List<Combatable> combatableTargets = new List<Combatable>();

	[HideInInspector]
	public bool InAttack;

	[HideInInspector]
	public AttackType CurrentAttackType;

	[HideInInspector]
	public bool Attacked;

	[HideInInspector]
	public List<Combatable> AttackTargets;

	[HideInInspector]
	public float InAttackTimer;

	[HideInInspector]
	public bool AttackIsHit;

	public Conflict MyConflict;

	private SpecialHit AttackSpecialHit;

	public List<AttackAnimation> AttackAnimations = new List<AttackAnimation>();

	[HideInInspector]
	public HitText CurrentHitText;

	[HideInInspector]
	private bool isDead;

	public List<Equipable> PossibleEquipables
	{
		get
		{
			if (!Application.isPlaying)
			{
				return (from id in this.PossibleEquipableIds
					select (Equipable)new GameDataLoader().GetCardFromId(id) into e
					where e != null
					select e).ToList();
			}
			return (from id in this.PossibleEquipableIds
				select (Equipable)WorldManager.instance.GameDataLoader.GetCardFromId(id) into e
				where e != null
				select e).ToList();
		}
	}

	public AttackType ProcessedAttackType
	{
		get
		{
			Equipable equipableOfEquipableType = base.GetEquipableOfEquipableType(EquipableType.Weapon);
			if (equipableOfEquipableType != null)
			{
				return equipableOfEquipableType.AttackType;
			}
			AttackType baseAttackType = this.BaseAttackType;
			if (this.InheritCombatStatsFromOtherCard)
			{
				Combatable obj = WorldManager.instance.GameDataLoader.GetCardFromId(this.InheritCombatStatsFrom) as Combatable;
				if (obj.InheritCombatStatsFromOtherCard)
				{
					Debug.LogError("The InheritCombatStatsFromOtherCard referenced by " + base.Id + " also inherits from another card");
				}
				baseAttackType = obj.BaseAttackType;
			}
			return baseAttackType;
		}
	}

	public override bool HasInventory => this.CanHaveInventory;

	public CombatStats RealBaseCombatStats
	{
		get
		{
			CombatStats combatStats = new CombatStats();
			if (!this.InheritCombatStatsFromOtherCard)
			{
				combatStats.InitStats(this.BaseCombatStats);
			}
			else
			{
				Combatable combatable = WorldManager.instance.GameDataLoader.GetCardFromId(this.InheritCombatStatsFrom) as Combatable;
				if (!combatable)
				{
					Debug.LogError("The InheritCombatStatsFromOtherCard referenced by " + base.Id + " is not set or incorrect");
				}
				else if (combatable.InheritCombatStatsFromOtherCard)
				{
					Debug.LogError("The InheritCombatStatsFromOtherCard referenced by " + base.Id + " also inherits from another card");
				}
				else
				{
					combatStats.InitStats(combatable.BaseCombatStats);
				}
			}
			return combatStats;
		}
	}

	public CombatStats ProcessedCombatStats
	{
		get
		{
			CombatStats realBaseCombatStats = this.RealBaseCombatStats;
			foreach (Equipable allEquipable in base.GetAllEquipables())
			{
				realBaseCombatStats.AddStats(allEquipable.MyStats);
			}
			realBaseCombatStats.MaxHealth = ((realBaseCombatStats.MaxHealth < 1) ? 1 : realBaseCombatStats.MaxHealth);
			return realBaseCombatStats;
		}
	}

	[HideInInspector]
	public bool InConflict => this.MyConflict != null;

	public bool CanLeaveConflict
	{
		get
		{
			if (this.MyConflict != null)
			{
				return this.MyConflict.CanLeaveConflict(this);
			}
			return false;
		}
	}

	public float TimeToAttackNormalized => this.AttackTimer / this.GetAttackTime();

	public Team Team
	{
		get
		{
			if (this is BaseVillager || this is CitiesCombatable)
			{
				return Team.Player;
			}
			return Team.Enemy;
		}
	}

	public AttackAnimation CurrentAttackAnimation
	{
		get
		{
			if (this.AttackAnimations.Count > 0)
			{
				return this.AttackAnimations[0];
			}
			return null;
		}
	}

	public float DamageMultiplier
	{
		get
		{
			if (base.HasStatusEffectOfType<StatusEffect_Drunk>())
			{
				return 2f;
			}
			return 1f;
		}
	}

	public override void OnLanguageChange()
	{
		this._combatableDescription = null;
		base.OnLanguageChange();
	}

	protected virtual float GetHitChance()
	{
		float num = this.ProcessedCombatStats.HitChance;
		if (base.HasStatusEffectOfType<StatusEffect_Drunk>())
		{
			num *= 0.6f;
		}
		return num;
	}

	protected virtual float GetAttackTime()
	{
		if (base.HasStatusEffectOfType<StatusEffect_Frenzy>())
		{
			return CombatStats.IncrementAttackSpeed(this.ProcessedCombatStats.AttackSpeed, 1);
		}
		return this.ProcessedCombatStats.AttackSpeed;
	}

	public string GetCombatTypeTitle()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return SokLoc.Translate("label_melee_title");
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return SokLoc.Translate("label_ranged_title");
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return SokLoc.Translate("label_magic_title");
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return SokLoc.Translate("label_air_title");
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return SokLoc.Translate("label_foot_title");
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return SokLoc.Translate("label_armour_title");
		}
		return "";
	}

	public string GetCombatTypeDescription()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return SokLoc.Translate("label_melee_description");
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return SokLoc.Translate("label_ranged_description");
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return SokLoc.Translate("label_magic_description");
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return SokLoc.Translate("label_air_description");
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return SokLoc.Translate("label_foot_description");
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return SokLoc.Translate("label_armour_description");
		}
		return "";
	}

	public string GetCombatTypeLore()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return SokLoc.Translate("label_melee_lore");
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return SokLoc.Translate("label_ranged_lore");
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return SokLoc.Translate("label_magic_lore");
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return SokLoc.Translate("label_air_lore");
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return SokLoc.Translate("label_foot_lore");
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return SokLoc.Translate("label_armour_lore");
		}
		return "";
	}

	public override void OnEquipItem(Equipable equipable)
	{
		this._combatableDescription = null;
		if (this.HealthPoints > this.ProcessedCombatStats.MaxHealth)
		{
			this.HealthPoints = this.ProcessedCombatStats.MaxHealth;
		}
	}

	public override void OnUnequipItem(Equipable equipable)
	{
		this._combatableDescription = null;
	}

	private void StartOrJoinConflictInStack()
	{
		if (base.MyGameCard.HasTransportCard())
		{
			return;
		}
		Conflict conflictInStack = this.GetConflictInStack();
		if (conflictInStack != null)
		{
			conflictInStack.JoinConflict(this);
			return;
		}
		List<CardData> list = base.CardsInStackMatchingPredicate((CardData x) => x is Combatable && x != this);
		Conflict conflict = Conflict.StartConflict(this);
		foreach (CardData item in list)
		{
			conflict.JoinConflict(item as Combatable);
		}
	}

	public void OnHealthChange()
	{
		this._combatableDescription = null;
	}

	private Conflict GetConflictInStack()
	{
		foreach (GameCard item in base.MyGameCard.GetAllCardsInStack())
		{
			if (item.CardData is Combatable { MyConflict: not null } combatable)
			{
				return combatable.MyConflict;
			}
		}
		return null;
	}

	public override void UpdateCard()
	{
		if (this.previouseHealthPoints != this.HealthPoints)
		{
			this.OnHealthChange();
		}
		base.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.HealthIcon;
		if (base.MyGameCard != null && base.MyGameCard.IsDemoCard)
		{
			base.MyGameCard.SpecialValue = this.ProcessedCombatStats.MaxHealth;
		}
		else
		{
			base.MyGameCard.SpecialValue = this.HealthPoints;
		}
		this.UpdateCombatableTargets();
		if ((this.combatableTargets.Count > 0 || this.GetConflictInStack() != null) && !this.InConflict)
		{
			this.StartOrJoinConflictInStack();
		}
		if (this.MyConflict != null && this.MyConflict.Initiator == this)
		{
			this.MyConflict.UpdateConflict();
		}
		if (this.InConflict)
		{
			bool flag = this.MyConflict.TimeSinceLastAttack <= 0.3f;
			if (this.CanAttack && !this.InAttack && !flag && !base.MyGameCard.BeingDragged)
			{
				this.AttackTimer += Time.deltaTime * WorldManager.instance.TimeScale;
			}
			bool flag2 = base.HasStatusEffectOfType<StatusEffect_Stunned>();
			if (!this.InAttack && this.StunTimer <= 0f && this.CanAttack && !flag2 && !flag && !this.BeingAttacked && this.AttackTimer >= this.GetAttackTime())
			{
				this.StartAttack();
			}
			if (this.InAttack)
			{
				this.UpdateAttackAnimations();
			}
		}
		this.StunTimer -= Time.deltaTime * WorldManager.instance.TimeScale;
		if (base.MyGameCard.BeingHovered && this.InConflict)
		{
			this.DrawConflictArrows(onlyVeryEffective: false);
		}
		this.previouseHealthPoints = this.HealthPoints;
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		base.descriptionOverride = SokLoc.Translate(base.DescriptionTerm);
		base.descriptionOverride = base.descriptionOverride + "\n\n<i>" + this.GetCombatableDescription() + "</i>";
		if (AdvancedSettingsScreen.AdvancedCombatStatsEnabled || GameCanvas.instance.CurrentScreen is CardopediaScreen)
		{
			base.descriptionOverride = base.descriptionOverride + "\n\n<i>" + this.GetCombatableDescriptionAdvanced() + "</i>";
		}
		base.UpdateCardText();
	}

	private void UpdateAttackAnimations()
	{
		for (int i = 0; i < this.AttackAnimations.Count; i++)
		{
			AttackAnimation attackAnimation = this.AttackAnimations[i];
			if (!attackAnimation.HasStarted)
			{
				attackAnimation.Start();
			}
			attackAnimation.Update();
			if (attackAnimation.IsDone)
			{
				this.AttackAnimations.RemoveAt(i);
				i--;
			}
			else if (attackAnimation.IsBlocking)
			{
				break;
			}
		}
		if (this.AttackAnimations.Count == 0)
		{
			this.CompleteAttack();
		}
	}

	private void StartAttack()
	{
		if (this.InAttack)
		{
			Debug.LogError("Already in attack!");
		}
		this.MyConflict.TimeSinceLastAttack = 0f;
		this.InAttack = true;
		this.InAttackTimer = 0f;
		this.Attacked = false;
		this.AttackTimer = 0f;
		Combatable target = this.MyConflict.GetTarget(this);
		this.AttackIsHit = Random.value <= this.GetHitChance();
		this.CurrentAttackType = this.ProcessedAttackType;
		if (!this.AttackIsHit)
		{
			this.AttackSpecialHit = null;
			this.AttackTargets = target.AsList();
		}
		else
		{
			this.AttackSpecialHit = this.DetermineSpecialHit();
			if (this.AttackSpecialHit == null || this.AttackSpecialHit.HitType == SpecialHitType.None)
			{
				this.AttackTargets = target.AsList();
			}
			else
			{
				Debug.Log($"Special hit by {base.Name}: {this.AttackSpecialHit.HitType}");
				this.AttackTargets = this.GetSpecialHitTargets(this.AttackSpecialHit, target);
			}
		}
		foreach (Combatable attackTarget in this.AttackTargets)
		{
			attackTarget.BeingAttacked = true;
		}
		foreach (Combatable attackTarget2 in this.AttackTargets)
		{
			Vector3 attackTargetPosition;
			if (this.AttackIsHit)
			{
				attackTargetPosition = attackTarget2.transform.position;
			}
			else
			{
				Vector2 vector = Random.insideUnitCircle.normalized * WorldManager.instance.CombatMissOffset;
				attackTargetPosition = attackTarget2.transform.position + new Vector3(vector.x, 0f, vector.y);
			}
			AttackAnimation attackAnimation = ((this.CurrentAttackType != AttackType.Ranged) ? ((this.CurrentAttackType != AttackType.Magic) ? ((this.CurrentAttackType != AttackType.Armour) ? ((this.CurrentAttackType != AttackType.Foot) ? ((this.CurrentAttackType != AttackType.Air) ? ((AttackAnimation)new AttackAnimationMelee()) : ((AttackAnimation)new AttackAnimationBullet())) : new AttackAnimationBullet()) : new AttackAnimationBullet()) : new AttackAnimationMagic()) : new AttackAnimationRanged());
			attackAnimation.Origin = this;
			attackAnimation.Target = attackTarget2;
			attackAnimation.AttackStartPosition = base.MyGameCard.transform.position;
			attackAnimation.AttackTargetPosition = attackTargetPosition;
			this.AttackAnimations.Add(attackAnimation);
		}
	}

	public virtual void NotifyParticipantUpdate(Combatable oldParticipant, Combatable newParticipant)
	{
		if (this.AttackTargets != null)
		{
			for (int i = 0; i < this.AttackTargets.Count; i++)
			{
				if (this.AttackTargets[i] == oldParticipant)
				{
					this.AttackTargets[i] = newParticipant;
				}
			}
		}
		if (this.AttackAnimations.Count <= 0)
		{
			return;
		}
		foreach (AttackAnimation attackAnimation in this.AttackAnimations)
		{
			if (attackAnimation.Target == oldParticipant)
			{
				attackAnimation.Target = newParticipant;
			}
		}
	}

	public Projectile CreateProjectile(Projectile projectilePrefab, Combatable target, AttackAnimation originAnimation)
	{
		Projectile projectile = Object.Instantiate(projectilePrefab);
		projectile.ShotBy = this;
		projectile.Target = target;
		projectile.transform.position = (projectile.StartPosition = base.transform.position);
		projectile.TargetPosition = originAnimation.AttackTargetPosition;
		projectile.OriginAnimation = originAnimation;
		return projectile;
	}

	public void CompleteAttack()
	{
		foreach (Combatable attackTarget in this.AttackTargets)
		{
			attackTarget.BeingAttacked = false;
		}
		this.AttackTargets = null;
		this.InAttack = false;
		this.Attacked = false;
	}

	public void DrawConflictArrows(bool onlyVeryEffective)
	{
		if (this.InAttack || this.Attacked || base.MyGameCard.BeingDragged)
		{
			return;
		}
		foreach (Combatable combatableTarget in this.MyConflict.GetCombatableTargets(this))
		{
			if (!(combatableTarget == null) && (!combatableTarget.InAttack || combatableTarget.CurrentAttackAnimation == null || !(combatableTarget.CurrentAttackAnimation is AttackAnimationMelee)) && !combatableTarget.MyGameCard.BeingDragged)
			{
				bool flag = this.IsVeryEffective(this.ProcessedAttackType, combatableTarget.ProcessedAttackType);
				if (!onlyVeryEffective || flag)
				{
					Vector3 vector = base.transform.position + Vector3.up * 0.1f;
					Vector3 vector2 = combatableTarget.transform.position + Vector3.up * 0.1f;
					Vector3 normalized = (vector2 - vector).normalized;
					float conflictArrowLengthDecrease = WorldManager.instance.ConflictArrowLengthDecrease;
					Color color = (onlyVeryEffective ? ColorManager.instance.EffectiveCombatLineColor : ColorManager.instance.CombatLineColor);
					ConflictArrow conflictArrow = default(ConflictArrow);
					conflictArrow.Start = vector + normalized * conflictArrowLengthDecrease;
					conflictArrow.End = vector2 - normalized * conflictArrowLengthDecrease;
					conflictArrow.Color = color;
					conflictArrow.VeryEffective = flag;
					ConflictArrow conflictArrow2 = conflictArrow;
					DrawManager.instance.DrawShape(conflictArrow2);
				}
			}
		}
	}

	public override void StoppedDragging()
	{
		Combatable combatable = base.MyGameCard.Parent?.Combatable;
		if (this.InConflict)
		{
			if (combatable != null && combatable.InConflict)
			{
				if (combatable.MyConflict == this.MyConflict)
				{
					this.MyConflict.SetParticipantTeamIndex(this, this.MyConflict.GetIndexInTeam(combatable));
				}
				else
				{
					this.MyConflict.LeaveConflict(this);
					this.StartOrJoinConflictInStack();
				}
				base.MyGameCard.RemoveFromStack();
				return;
			}
			if (!this.CanLeaveConflict)
			{
				base.MyGameCard.RemoveFromStack();
				return;
			}
			Conflict overlappingConflict = base.MyGameCard.GetOverlappingConflict();
			if (overlappingConflict != null && overlappingConflict != this.MyConflict)
			{
				this.MyConflict.LeaveConflict(this);
				overlappingConflict.JoinConflict(this);
			}
			if (overlappingConflict == null)
			{
				this.MyConflict.LeaveConflict(this);
			}
		}
		else
		{
			if (combatable != null && combatable.Team != this.Team)
			{
				base.MyGameCard.transform.position = combatable.transform.position;
				this.StartOrJoinConflictInStack();
			}
			Conflict overlappingConflict2 = base.MyGameCard.GetOverlappingConflict();
			if (overlappingConflict2 != null && !this.InConflict)
			{
				overlappingConflict2.JoinConflict(this);
			}
		}
	}

	public void CreateAndEquipCard(string cardId, bool markAsFound)
	{
		CardData cardPrefab = WorldManager.instance.GetCardPrefab(cardId);
		if (!(cardPrefab is Equipable))
		{
			Debug.LogError("Can't give " + cardId + " to " + base.Id + " because it is not an Equipable");
		}
		else
		{
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardPrefab, faceUp: false, checkAddToStack: true, playSound: true, markAsFound);
			cardData.MyGameCard.MyBoard = base.MyGameCard.MyBoard;
			base.EquipItem(cardData as Equipable);
			cardData.MyGameCard.Visuals.gameObject.SetActive(value: false);
		}
	}

	public void ExitConflict()
	{
		this.AttackAnimations.Clear();
		this.AttackTimer = 0f;
		this.BeingAttacked = false;
		this.InAttack = false;
		this.Attacked = false;
	}

	public SpecialHit DetermineSpecialHit()
	{
		WeightedRandomBag<SpecialHit> weightedRandomBag = new WeightedRandomBag<SpecialHit>();
		float num = 0f;
		foreach (SpecialHit specialHit2 in this.ProcessedCombatStats.SpecialHits)
		{
			num += specialHit2.Chance;
			weightedRandomBag.AddEntry(specialHit2, specialHit2.Chance);
		}
		SpecialHit specialHit = new SpecialHit();
		specialHit.HitType = SpecialHitType.None;
		specialHit.Target = SpecialHitTarget.Target;
		specialHit.Chance = 100f - num;
		weightedRandomBag.AddEntry(specialHit, specialHit.Chance);
		return weightedRandomBag.Choose();
	}

	public int GetDamage(Combatable target)
	{
		if (target.HasStatusEffectOfType<StatusEffect_Invulnerable>())
		{
			return 0;
		}
		int attackDamage = this.ProcessedCombatStats.AttackDamage;
		int num = CombatStats.IncrementAttackDefence(this.ProcessedCombatStats.AttackDamage, 1);
		int num2 = ((Random.value < 0.5f) ? attackDamage : num);
		int defence = target.ProcessedCombatStats.Defence;
		int num3 = num2 - Mathf.CeilToInt((float)defence * 0.5f);
		num3 = Mathf.RoundToInt((float)num3 * this.GetCombatRuleMultiplier(target, this) * this.DamageMultiplier);
		if (num3 > 0)
		{
			return num3;
		}
		return Mathf.RoundToInt(Random.value);
	}

	public bool IsVeryEffective(AttackType self, AttackType target)
	{
		if (self == AttackType.Melee && target == AttackType.Magic)
		{
			return true;
		}
		if (self == AttackType.Magic && target == AttackType.Ranged)
		{
			return true;
		}
		if (self == AttackType.Ranged && target == AttackType.Melee)
		{
			return true;
		}
		return false;
	}

	public float GetCombatRuleMultiplier(Combatable target, Combatable self)
	{
		if (!this.IsVeryEffective(self.ProcessedAttackType, target.ProcessedAttackType))
		{
			return 1f;
		}
		return 1.4f;
	}

	private List<Combatable> GetSpecialHitTargets(SpecialHit specialHit, Combatable target)
	{
		if (specialHit.HitType == SpecialHitType.HealLowest)
		{
			List<Combatable> list = (from x in this.MyConflict.GetFriendlyParticipants(this)
				orderby x.HealthPoints
				select x).ToList();
			if (list.Count > 0)
			{
				return list[0].AsList();
			}
		}
		return specialHit.Target switch
		{
			SpecialHitTarget.Self => this.AsList(), 
			SpecialHitTarget.Target => target.AsList(), 
			SpecialHitTarget.RandomFriendly => this.MyConflict.GetFriendlyParticipants(this).Choose().AsList(), 
			SpecialHitTarget.RandomEnemy => this.MyConflict.GetEnemyParticipants(this).Choose().AsList(), 
			SpecialHitTarget.AllFriendly => this.MyConflict.GetFriendlyParticipants(this), 
			SpecialHitTarget.AllEnemy => this.MyConflict.GetEnemyParticipants(this), 
			_ => target.AsList(), 
		};
	}

	public virtual void PerformAttack(Combatable target, Vector3 attackPos)
	{
		if (target == null)
		{
			return;
		}
		target.StunTimer = 0.05f;
		if (this.AttackIsHit)
		{
			int num = Mathf.Clamp(this.GetDamage(target), 0, 100);
			if (this.AttackSpecialHit != null && this.AttackSpecialHit.HitType != 0)
			{
				if (!target.HasStatusEffectOfType<StatusEffect_Invulnerable>())
				{
					this.PerformSpecialHit(this.AttackSpecialHit, target, num);
				}
				else
				{
					target.Damage(num);
				}
				this.ShowHitText(this, target, attackPos, this.AttackIsHit, num, this.AttackSpecialHit.HitType);
			}
			else
			{
				target.Damage(num);
				this.ShowHitText(this, target, attackPos, this.AttackIsHit, num, null);
			}
		}
		else
		{
			this.ShowHitText(this, target, attackPos, this.AttackIsHit, -1, null);
		}
	}

	private void PerformSpecialHit(SpecialHit specialHit, Combatable target, int dmg)
	{
		Debug.Log($"Special hit by {base.Name}: {specialHit.HitType}");
		if (specialHit.HitType == SpecialHitType.Poison)
		{
			if (!target.HasStatusEffectOfType<StatusEffect_Poison>())
			{
				target.AddStatusEffect(new StatusEffect_Poison());
			}
		}
		else if (specialHit.HitType == SpecialHitType.Crit)
		{
			dmg *= 2;
		}
		else if (specialHit.HitType == SpecialHitType.Stun)
		{
			target.RemoveStatusEffect<StatusEffect_Stunned>();
			target.AddStatusEffect(new StatusEffect_Stunned());
		}
		else if (specialHit.HitType == SpecialHitType.Bleeding)
		{
			if (!target.HasStatusEffectOfType<StatusEffect_Bleeding>())
			{
				target.AddStatusEffect(new StatusEffect_Bleeding());
			}
		}
		else if (specialHit.HitType == SpecialHitType.Frenzy)
		{
			target.RemoveStatusEffect<StatusEffect_Frenzy>();
			target.AddStatusEffect(new StatusEffect_Frenzy());
		}
		else if (specialHit.HitType == SpecialHitType.Sick)
		{
			if (!target.HasEquipableWithId("plague_mask"))
			{
				target.RemoveStatusEffect<StatusEffect_Sick>();
				target.AddStatusEffect(new StatusEffect_Sick());
				AudioManager.me.PlaySound2D(AudioManager.me.GetSick, 1f, 0.5f);
			}
		}
		else if (specialHit.HitType == SpecialHitType.HealLowest)
		{
			target.HealthPoints = Mathf.Clamp(target.HealthPoints + 2, 0, target.ProcessedCombatStats.MaxHealth);
		}
		else if (specialHit.HitType == SpecialHitType.Heal)
		{
			target.HealthPoints = Mathf.Clamp(target.HealthPoints + 2, 0, target.ProcessedCombatStats.MaxHealth);
		}
		else if (specialHit.HitType == SpecialHitType.LifeSteal)
		{
			this.HealthPoints = Mathf.Clamp(this.HealthPoints + dmg, 0, this.ProcessedCombatStats.MaxHealth);
		}
		else if (specialHit.HitType == SpecialHitType.Invulnerable)
		{
			if (!target.HasStatusEffectOfType<StatusEffect_Invulnerable>())
			{
				target.AddStatusEffect(new StatusEffect_Invulnerable());
			}
		}
		else if (specialHit.HitType == SpecialHitType.Anxious && !target.HasStatusEffectOfType<StatusEffect_Anxious>())
		{
			target.AddStatusEffect(new StatusEffect_Anxious());
		}
		bool flag = specialHit.Target == SpecialHitTarget.Target || specialHit.Target == SpecialHitTarget.RandomEnemy || specialHit.Target == SpecialHitTarget.AllEnemy;
		if (specialHit.Target == SpecialHitTarget.Self && (specialHit.HitType == SpecialHitType.Crit || specialHit.HitType == SpecialHitType.Stun || specialHit.HitType == SpecialHitType.Bleeding))
		{
			flag = true;
		}
		if (specialHit.HitType == SpecialHitType.HealLowest)
		{
			flag = false;
		}
		if (flag)
		{
			target.Damage(dmg);
		}
	}

	private void ShowHitText(Combatable origin, Combatable effectTarget, Vector3 targetPosition, bool isHit, int damage, SpecialHitType? type = null)
	{
		bool veryEffective = this.IsVeryEffective(origin.ProcessedAttackType, effectTarget.ProcessedAttackType);
		if (!type.HasValue)
		{
			if (isHit)
			{
				if (damage == 0)
				{
					if (effectTarget.HasStatusEffectOfType<StatusEffect_Invulnerable>())
					{
						effectTarget.CreateHitText("block", PrefabManager.instance.BlockHitText);
					}
					else
					{
						effectTarget.CreateHitText("block", PrefabManager.instance.BlockHitText);
					}
					AudioManager.me.PlaySound2D(AudioManager.me.Block, Random.Range(0.8f, 1.2f), 0.2f);
				}
				else
				{
					effectTarget.CreateHitText($"{damage}").SetVeryEffective(veryEffective);
					AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
				}
			}
			else
			{
				effectTarget.CreateHitText("miss", PrefabManager.instance.MissHitText).transform.position = targetPosition;
				if (WorldManager.instance.CurrentBoard.Id == "cities")
				{
					AudioManager.me.PlaySound2D(AudioManager.me.MissCities, Random.Range(0.8f, 1.2f), 0.5f);
				}
				else
				{
					AudioManager.me.PlaySound2D(AudioManager.me.Miss, Random.Range(0.8f, 1.2f), 0.5f);
				}
			}
			return;
		}
		switch (type)
		{
		case SpecialHitType.Heal:
			AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
			effectTarget.CreateHitText("2", PrefabManager.instance.HealHitText);
			break;
		case SpecialHitType.HealLowest:
			AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
			effectTarget.CreateHitText("2", PrefabManager.instance.HealHitText);
			break;
		case SpecialHitType.Crit:
			AudioManager.me.PlaySound2D(AudioManager.me.Crit, Random.Range(0.8f, 1.2f), 0.2f);
			effectTarget.CreateHitText($"{damage}!", PrefabManager.instance.CritHitText).SetVeryEffective(veryEffective);
			break;
		case SpecialHitType.Stun:
			AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
			effectTarget.CreateHitText("stun", PrefabManager.instance.CritHitText).SetVeryEffective(veryEffective);
			break;
		case SpecialHitType.Damage:
			AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
			effectTarget.CreateHitText($"{damage}").SetVeryEffective(veryEffective);
			break;
		case SpecialHitType.LifeSteal:
			AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
			AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
			effectTarget.CreateHitText($"{damage}", PrefabManager.instance.BleedHitText).SetVeryEffective(veryEffective);
			origin.CreateHitText($"{damage}", PrefabManager.instance.HealHitText);
			break;
		case SpecialHitType.Frenzy:
		case SpecialHitType.Invulnerable:
			effectTarget.CreateHitText("buff", PrefabManager.instance.HitTextPrefab);
			AudioManager.me.PlaySound2D(AudioManager.me.Buff, Random.Range(0.8f, 1.2f), 0.2f);
			break;
		case SpecialHitType.Poison:
		case SpecialHitType.Bleeding:
		case SpecialHitType.Sick:
		case SpecialHitType.Anxious:
			effectTarget.CreateHitText($"{damage}", PrefabManager.instance.HitTextPrefab).SetVeryEffective(veryEffective);
			AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
			break;
		default:
			effectTarget.CreateHitText("NYI");
			AudioManager.me.PlaySound2D(this.GetAttackTypeHitSound(), Random.Range(0.8f, 1.2f), 0.2f);
			break;
		}
	}

	public List<AudioClip> GetAttackTypeHitSound()
	{
		if (this.ProcessedAttackType == AttackType.Melee)
		{
			return AudioManager.me.HitMelee;
		}
		if (this.ProcessedAttackType == AttackType.Ranged)
		{
			return AudioManager.me.HitRanged;
		}
		if (this.ProcessedAttackType == AttackType.Magic)
		{
			return AudioManager.me.HitMagic;
		}
		if (this.ProcessedAttackType == AttackType.Foot)
		{
			return AudioManager.me.HitFoot;
		}
		if (this.ProcessedAttackType == AttackType.Armour)
		{
			return AudioManager.me.HitArmour;
		}
		if (this.ProcessedAttackType == AttackType.Air)
		{
			return AudioManager.me.HitAir;
		}
		return AudioManager.me.HitMelee;
	}

	public HitText CreateHitText(string txt, HitText prefab = null)
	{
		if (prefab == null)
		{
			prefab = PrefabManager.instance.NormalHitText;
		}
		HitText hitText = WorldManager.instance.CreateHitText(base.transform.position, txt, prefab);
		hitText.TargetCombatable = this;
		this.CurrentHitText = hitText;
		return hitText;
	}

	public virtual void Damage(int damage)
	{
		this.HealthPoints -= damage;
		this.HealthPoints = Mathf.Max(this.HealthPoints, 0);
		this.StunTimer = 0.05f;
		GameCamera.instance.Screenshake = 0.3f;
		base.MyGameCard.SetHitEffect(delegate
		{
			this.CheckDeath();
		});
		base.MyGameCard.RotWobble(0.5f);
		base.MyGameCard.transform.localScale *= 1.5f;
	}

	private void CheckDeath()
	{
		if (!this.isDead && this.HealthPoints <= 0)
		{
			this.isDead = true;
			this.InAttack = false;
			QuestManager.instance.SpecialActionComplete(base.Id + "_killed");
			this.Die();
		}
	}

	public virtual void Die()
	{
		if (this.MyConflict != null)
		{
			this.MyConflict.LeaveConflict(this);
		}
		base.MyGameCard.GetAllCardsInStack().Remove(base.MyGameCard);
		base.MyGameCard.DestroyCard(spawnSmoke: true);
	}

	public virtual void UpdateCombatableTargets()
	{
		this.combatableTargets.Clear();
		GameCard gameCard = base.MyGameCard.GetRootCard();
		while (gameCard != null)
		{
			if (gameCard.CardData is Combatable combatable && gameCard.CardData != this && combatable.Team != this.Team)
			{
				this.combatableTargets.Add(combatable);
			}
			gameCard = gameCard.Child;
		}
	}

	public string GetCombatableDescription()
	{
		if (!string.IsNullOrEmpty(this._combatableDescription))
		{
			return this._combatableDescription;
		}
		string text = "";
		if (base.MyGameCard != null && !base.MyGameCard.IsDemoCard)
		{
			text = text + SokLoc.Translate("label_health_info", LocParam.Create("health", this.HealthPoints.ToString()), LocParam.Create("maxhealth", this.ProcessedCombatStats.MaxHealth.ToString())) + "\n";
		}
		int num = Mathf.RoundToInt(this.RealBaseCombatStats.CombatLevel);
		int num2 = Mathf.RoundToInt(this.ProcessedCombatStats.CombatLevel);
		if (num2 != num)
		{
			text = text + SokLoc.Translate("label_base_combatlevel", LocParam.Create("level", num.ToString())) + "\n";
			text += SokLoc.Translate("label_total_combatlevel", LocParam.Create("level", num2.ToString()));
		}
		else
		{
			text += SokLoc.Translate("label_combatlevel", LocParam.Create("level", num2.ToString()));
		}
		string text2 = this.ProcessedCombatStats.SummarizeSpecialHits();
		if (text2.Length > 0)
		{
			text = text + "\n\n" + text2;
		}
		this._combatableDescription = text;
		return text;
	}

	public string GetCombatableDescriptionAdvanced()
	{
		string text = SokLoc.Translate("label_combat_speed");
		string text2 = SokLoc.Translate("label_hit_chance");
		string text3 = SokLoc.Translate("label_damage");
		string text4 = SokLoc.Translate("label_defence");
		CombatStats processedCombatStats = this.ProcessedCombatStats;
		string attackSpeedTranslation = processedCombatStats.GetAttackSpeedTranslation();
		string attackDamageTranslation = processedCombatStats.GetAttackDamageTranslation();
		string hitChanceTranslation = processedCombatStats.GetHitChanceTranslation();
		string defenceTranslation = processedCombatStats.GetDefenceTranslation();
		string text5 = SokLoc.Translate("label_seconds_format", LocParam.Create("seconds", processedCombatStats.AttackSpeed.ToString()));
		return $"<size=80%>{text} {attackSpeedTranslation} ({text5})\n{text2} {hitChanceTranslation} ({processedCombatStats.HitChance * 100f}%)\n{text3} {attackDamageTranslation} ({processedCombatStats.AttackDamage})\n{text4}: {defenceTranslation} ({processedCombatStats.Defence})</size>";
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying || !this.InConflict)
		{
			return;
		}
		foreach (Combatable combatableTarget in this.MyConflict.GetCombatableTargets(this))
		{
			Gizmos.DrawLine(base.transform.position, combatableTarget.transform.position);
		}
		Bounds bounds = this.MyConflict.GetBounds();
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}

	public static bool SpecialHitTypeIsAttack(SpecialHitType hitType)
	{
		if (hitType == SpecialHitType.Heal || hitType == SpecialHitType.HealLowest || hitType == SpecialHitType.Invulnerable)
		{
			return false;
		}
		return true;
	}

	public void LogBaseCombatLevel()
	{
		Debug.Log($"Base combat level: {this.BaseCombatStats.CombatLevel}");
	}
}
