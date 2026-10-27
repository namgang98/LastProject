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
    private IStatModifier training;
    public event Action<ItemInstance, ICooldownSource> OnActionReady;
    public event Action OnBattleEnded;


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

        var stats = PlayerStatsBuilder.Build(playerClass, backpack, training);
        this.player.Health.SetMaxHp(stats.MaxHp);

        foreach(var item in backpack.EquippedItems())
        {
            if(item.Info is WeaponInfo weapon)
            {
                AttackData attack = item is ForgeableInstance forged ? forged.ApplyTo(weapon.Attack) : weapon.Attack;
                var data = stats.Apply(weapon.Type, attack);
                var weaponAction = AddAction(new AttackEffect(monster.Health, data, statuses), data.Cooldown, player.AttackWindup, player.Health, player.PlayAttack);
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

        this.player.Health.OnDie += EndBattle;
        this.monster.Health.OnDie += EndBattle;
        potionUser = new PotionUser(backpack, player.Health, statuses, potionThreshold, potionCooldown);
        potionUser.OnBattleStart(isBoss);
        isFighting = true;
    }
    public void Bind(Backpack backpack, ClassInfo playerClass, IStatModifier training)
    {
        this.backpack = backpack;
        this.playerClass = playerClass;
        this.training = training;
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
        IBattleEffect normal = new AttackEffect(player.Health, attacks.Normal, statuses);
        IBattleEffect skill = attacks.HasSkill ? new AttackEffect(player.Health, attacks.Skill, statuses) : null;
        MonsterAttackPattern pattern = new MonsterAttackPattern(normal, skill, attacks.SkillChance);

        AddAction(pattern, attacks.Normal.Cooldown, monster.AttackWindup, monster.Health, () => PlayMonsterAttack(pattern));
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
}
