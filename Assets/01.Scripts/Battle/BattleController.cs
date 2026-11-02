using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    [Header("물약")]
    [SerializeField, Range(0f, 1f)] float potionThreshold = 0.2f;
    [SerializeField] float potionCooldown = 2f;
    private PotionUser potionUser;

    private Backpack backpack;
    private readonly List<ItemAction> actions = new();
    private readonly StatusEffects statuses = new();
    private bool isFighting;
    private UnitController player;
    private UnitController monster;
    private ClassInfo playerClass;
    private IStatModifier[] bonuses;
    public event Action<ItemInstance, ICooldownSource> OnActionReady;
    public event Action OnBattleEnded;
    private IRunBonus runBonus = EmptyRunBonus.Instance;
    private TowerRuleSet towerRules = new();
    private void Update()
    {
        if(!isFighting)
            return;
        
        for(int i = 0; i < actions.Count; i++)
        {
            float speed = statuses.GetSpeedMultiplier(actions[i].Owner);
            actions[i].Tick(Time.deltaTime * speed);

            if(!isFighting)
                return;
        }
        potionUser?.Tick(Time.deltaTime);
        statuses.Tick(Time.deltaTime);
    }

    public void StartBattle(UnitController player, UnitController monster, MonsterAttackSet monsterAttacks, bool isBoss)
    {
        if(isFighting)
            EndBattle();

        actions.Clear();
        if(backpack == null || playerClass == null)
        {
            Debug.LogError("[BattleController] Bind 되지 않았습니다.", this);
            return;
        }
        this.player = player;
        this.monster = monster;

        var stats = PlayerStatsBuilder.Build(playerClass, backpack, bonuses);
        this.player.Health.SetMaxHp(stats.MaxHp);
        var rules = new PlayerAttackRules(runBonus, player.Health, isBoss);
        float firstStrike = Mathf.Clamp(runBonus.Sum(BlessingStat.FirstStrike) * 0.5f, 0f, 0.9f);

        if(player.Health is PlayerHealth playerHealth)
        {
            playerHealth.SetDefense(runBonus.Sum(BlessingStat.DamageReduction), runBonus.Sum(BlessingStat.Dodge));
            playerHealth.SetRevive(runBonus.Sum(BlessingStat.Revive));
        }

        foreach(var item in backpack.EquippedItems())
        {
            if(item.Info is WeaponInfo weapon)
            {
                AttackData attack = item is ForgeableInstance forged ? forged.ApplyTo(weapon.Attack) : weapon.Attack;
                var data = rules.ApplyElement(stats.Apply(weapon.Type, attack));
                var weaponAction = AddAction(new AttackEffect(monster.Health, data, statuses, rules), data.Cooldown, player.AttackWindup, player.Health, player.PlayAttack);
                weaponAction.Advance(firstStrike);
                OnActionReady?.Invoke(item, weaponAction);
                continue;
            }

            if(item.Info is not EquipmentInfo equip || !equip.Effect.HasEffect)
                continue;

            IBattleEffect effect = BattleEffectFactory.Create(equip.Effect, player.Health);

            if(effect == null)
                continue;


            var action = AddAction(effect, equip.Effect.Cooldown, 0f, player.Health, null);
            OnActionReady?.Invoke(item, action);
        }

        AddMonsterAction(monsterAttacks);

        this.player.Health.AddShield(Mathf.RoundToInt(runBonus.Sum(BlessingStat.StartShield)));
        this.player.Health.OnHit += HandlePlayerHit;
        this.player.Health.OnDie += EndBattle;
        this.monster.Health.OnDie += EndBattle;
        potionUser = new PotionUser(backpack, player.Health, statuses, potionThreshold, potionCooldown, runBonus.Sum(BlessingStat.PotionPower));
        potionUser.OnBattleStart(isBoss);
        isFighting = true;
    }
    public void Bind(Backpack backpack, ClassInfo playerClass, params IStatModifier[] bonuses)
    {
        this.backpack = backpack;
        this.playerClass = playerClass;
        this.bonuses = bonuses;
    }

    public void EndBattle()
    {
        if(!isFighting)
            return;

        isFighting = false;
        player.Health.ClearShield();
        statuses.Clear();
        actions.Clear();
        potionUser = null;
        OnBattleEnded?.Invoke();
        player.Health.OnDie -= EndBattle;
        monster.Health.OnDie -= EndBattle;
        player.Health.OnHit -= HandlePlayerHit;
    }

    private ItemAction AddAction(IBattleEffect effect, float cooldown, float windup, UnitHealth owner, Action onFired)
    {
        ItemAction action = new ItemAction(effect, cooldown, windup, owner);

        if(onFired != null)
            action.OnFired += onFired;

        actions.Add(action);
        return action;
    }
    private void AddMonsterAction(MonsterAttackSet attacks)
    {
        var rules = new MonsterAttackRules(monster.Health, towerRules.MonsterLifesteal);
        AttackData normalData = towerRules.ModifyMonsterAttack(attacks.Normal);
        IBattleEffect normal = new AttackEffect(player.Health, normalData, statuses, rules);
        IBattleEffect skill = attacks.HasSkill ? new AttackEffect(player.Health, towerRules.ModifyMonsterAttack(attacks.Skill), statuses, rules) : null;
        MonsterAttackPattern pattern = new MonsterAttackPattern(normal, skill, attacks.SkillChance);

        AddAction(pattern, normalData.Cooldown, monster.AttackWindup, monster.Health, () => PlayMonsterAttack(pattern));
        AddMonsterRegen();
    }

    private void AddMonsterRegen()
    {
        int amount = Mathf.RoundToInt(monster.Health.MaxHp * towerRules.MonsterRegen);

        if(amount <= 0)
            return;

        AddAction(new HealEffect(monster.Health, amount), 1f, 0f, monster.Health, null);
    }
    public void SetRunBonus(IRunBonus bonus)
    {
        runBonus = bonus ?? EmptyRunBonus.Instance;
    }
    public void SetTowerRules(TowerRuleSet rules)
    {
        towerRules = rules ?? new TowerRuleSet(); 
    }
    private void PlayMonsterAttack(MonsterAttackPattern pattern)
    {
        if(pattern.Prepare())
        {
            monster.PlaySkill();
            return;
        }

        monster.PlayAttack();
    }

    private void HandlePlayerHit(DamageInfo info)
    {
        float thorns = runBonus.Sum(BlessingStat.Thorns);

        if(info.IsTick || thorns <= 0f || monster == null || monster.Health.IsDead)
            return;

        int reflect = Mathf.Max(1, Mathf.RoundToInt(info.Amount * thorns));
        monster.Health.TakeDamage(new DamageInfo(reflect, false, DamageElement.None, true));
    }

}
