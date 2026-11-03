#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CheatCommands
{
    private static readonly List<ForgeableInstance> forgeables = new();

    public static bool IsReady => DataManager.HasInstance && DataManager.instance.IsLoaded && GameSession.HasInstance;
    public static bool InDungeon => Object.FindAnyObjectByType<StageController>() != null;

    public static void AddGold(int amount)
    {
        if(!IsReady)
            return;

        GameSession.instance.Wallet.Earn(amount);
        SaveIfVillage();
        Notify($"골드 +{amount}");
    }

    public static void GiveItem(int id, int count)
    {
        if(!IsReady || !DataManager.instance.TryGetItem(id, out ItemInfo info))
            return;

        int given = 0;

        for(int i = 0; i < count; i++)
        {
            if(GameSession.instance.TryReceive(info.CreateInstance()))
                given++;
        }

        SaveIfVillage();
        Notify($"{info.Name} {given}/{count}개 지급");
    }

    public static void SetForgeLevel(int level)
    {
        if(!IsReady)
            return;

        GameSession.instance.CollectForgeables(forgeables);

        foreach(ForgeableInstance item in forgeables)
        {
            item.SetLevel(level);
        }

        SaveIfVillage();
        Notify($"장비 {forgeables.Count}개 +{level}");
    }

    public static void AddFavor(NpcType npc, int amount)
    {
        if(!IsReady)
            return;

        int levelUps = GameSession.instance.Favor.AddFavor(npc, amount);
        SaveIfVillage();
        Notify($"{npc.ToKorean()} 우호도 +{amount} (레벨 +{levelUps})");
    }

    public static void AddBlessing(int id)
    {
        RunController run = Object.FindAnyObjectByType<RunController>();

        if(run == null || run.Blessings == null || !DataManager.instance.TryGetBlessing(id, out BlessingInfo info))
            return;

        bool added = run.Blessings.TryAdd(info);
        Notify(added ? $"축복 {info.Name} 추가" : $"축복 {info.Name} 최대 중첩");
    }

    public static void KillMonster()
    {
        MonsterHealth monster = Object.FindAnyObjectByType<MonsterHealth>();

        if(monster == null || monster.IsDead)
            return;

        monster.TakeDamage(new DamageInfo(monster.CurrentHP, false, DamageElement.None));
    }

    public static void HealPlayer()
    {
        PlayerHealth player = Object.FindAnyObjectByType<PlayerHealth>();

        if(player == null)
            return;

        player.Heal(player.MaxHp);
    }

    public static void JumpToFloor(int floor)
    {
        StageController stage = Object.FindAnyObjectByType<StageController>();

        if(stage == null)
            return;

        stage.DebugJumpTo(floor);
        Notify($"{floor}층으로 이동");
    }

    public static void EndRun(RunEndReason reason)
    {
        StageController stage = Object.FindAnyObjectByType<StageController>();

        if(stage == null)
            return;

        stage.EndRun(reason);
    }

    public static void ConquerNextTower()
    {
        if(!IsReady)
            return;

        GameSession session = GameSession.instance;

        foreach(TowerInfo tower in session.Towers.Towers)
        {
            if(tower.IsEndless || !session.Towers.CanEnter(tower))
                continue;

            session.SelectTower(tower);
            session.FinishRun(RunEndReason.Conquest);
            string scene = session.ConsumeCutscene() == CutsceneKind.None ? SceneNames.Village : SceneNames.TowerMap;
            SceneLoader.instance.Load(scene);
            return;
        }
    }

    public static void ResetTowers()
    {
        if(!IsReady)
            return;

        GameSession.instance.ResetTowers();
        SceneLoader.instance.Load(SceneNames.Village);
    }

    public static void ResetSave()
    {
        if(!IsReady || !GameSession.instance.ResetSave())
            return;

        SceneLoader.instance.Load(SceneNames.Title);
    }

    public static void SaveNow()
    {
        if(!IsReady || !SaveIfVillage())
            return;

        Notify("저장 완료");
    }

    private static bool SaveIfVillage()
    {
        if(SceneManager.GetActiveScene().name != SceneNames.Village)
            return false;

        GameSession.instance.SaveInVillage();
        return true;
    }

    private static void Notify(string text)
    {
        EventManager.instance.PostNotification(new OnFeedback($"[테스트] {text}", FeedbackType.Info));
        Debug.Log($"[Cheat] {text}");
    }
}
#endif