using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * F1 골드 +10000
 * F2 강화 재료 세트 지급 (각 5개)
 * F3 후반 장비 지급 (불검, 얼음검, 철검, 베테랑 갑옷, 힘의 반지)
 * F4 가진 무기, 갑옷 전부 +5로 설정
 * F5 회복 물약 5개
 * F6 지금 몬스터 즉사
 * F7 플레이어 체력 완전 회복
 */


public class DebugCheats : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] int gold = 10000;
    [SerializeField] int materialCount = 5;
    [SerializeField] int[] materialIds = { 8001, 8003, 8006, 8007, 8008, 8011, 8012, 8013, 8014, 8015 };
    [SerializeField] int[] gearIds = { 1003, 1002, 1001, 4002, 5000 };
    [SerializeField] int potionId = 8501;
    [SerializeField] int potionCount = 5;
    [SerializeField] int forgeLevel = 5;

    private readonly List<ForgeableInstance> forgeables = new();

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if(keyboard == null || GameSession.instance == null)
            return;

        if(keyboard.f1Key.wasPressedThisFrame)
            AddGold();

        if(keyboard.f2Key.wasPressedThisFrame)
            GiveItems(materialIds, materialCount, "강화 재료");

        if(keyboard.f3Key.wasPressedThisFrame)
            GiveItems(gearIds, 1, "후반 장비");

        if(keyboard.f4Key.wasPressedThisFrame)
            SetForgeLevel();

        if(keyboard.f5Key.wasPressedThisFrame)
            GiveItems(new[] { potionId }, potionCount, "회복 물약");

        if(keyboard.f6Key.wasPressedThisFrame)
            KillMonster();

        if(keyboard.f7Key.wasPressedThisFrame)
            HealPlayer();

        if(keyboard.f8Key.wasPressedThisFrame)
            SceneLoader.instance.Load(SceneNames.TowerRise);

        if(keyboard.f9Key.wasPressedThisFrame)
            ConquerNextTower();
        if(keyboard.f10Key.wasPressedThisFrame)
            ResetTowers();
    }
    private void ResetTowers()
    {
        GameSession.instance.ResetTowers();
        SceneLoader.instance.Load(SceneNames.Village);
    }
    private void AddGold()
    {
        GameSession.instance.Wallet.Earn(gold);
        Notify($"골드 +{gold}");
    }

    private void GiveItems(int[] ids, int count, string label)
    {
        GameSession session = GameSession.instance;
        int given = 0;
        int failed = 0;

        foreach(int id in ids)
        {
            if(!DataManager.instance.TryGetItem(id, out ItemInfo info))
                continue;

            for(int i = 0; i < count; i++)
            {
                if(session.TryReceive(info.CreateInstance()))
                {
                    given++;
                    continue;
                }

                failed++;
            }
        }

        session.SaveInVillage();
        Notify(failed > 0 ? $"{label} {given}개 지급 (자리 부족 {failed}개)" : $"{label} {given}개 지급");
    }

    private void SetForgeLevel()
    {
        GameSession session = GameSession.instance;
        session.CollectForgeables(forgeables);

        foreach(ForgeableInstance item in forgeables)
        {
            item.SetLevel(forgeLevel);
        }

        session.SaveInVillage();
        Notify($"장비 {forgeables.Count}개 +{forgeLevel}");
    }

    private void KillMonster()
    {
        MonsterHealth monster = FindAnyObjectByType<MonsterHealth>();

        if(monster == null || monster.IsDead)
            return;

        monster.TakeDamage(new DamageInfo(monster.CurrentHP, false, DamageElement.None));
    }

    private void HealPlayer()
    {
        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();

        if(player == null)
            return;

        player.Heal(player.MaxHp);
    }

    private static void Notify(string text)
    {
        EventManager.instance.PostNotification(new OnFeedback($"[테스트] {text}", FeedbackType.Info));
    }
    private void ConquerNextTower()
    {
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
#endif
}