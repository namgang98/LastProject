using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CheatWindow : EditorWindow
{
    private int gold = 10000;

    private string itemFilter = string.Empty;
    private int itemIndex;
    private int itemCount = 1;
    private readonly List<ItemInfo> items = new();
    private string[] itemNames;

    private int forgeLevel = 5;

    private NpcType npc = NpcType.Merchant;
    private int favor = 100;

    private int blessingIndex;
    private string[] blessingNames;

    private int floor = 9;
    private float timeScale = 1f;

    private Vector2 scroll;

    [MenuItem("Tools/Packbound/치트 창")]
    public static void Open()
    {
        GetWindow<CheatWindow>("치트");
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 중에만 사용할 수 있습니다.", MessageType.Info);
            itemNames = null;
            blessingNames = null;
            return;
        }

        if (!CheatCommands.IsReady)
        {
            EditorGUILayout.HelpBox("데이터를 불러오는 중입니다.", MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawEconomy();
        DrawItems();
        DrawFavor();
        DrawRun();
        DrawTowers();
        DrawSystem();
        EditorGUILayout.EndScrollView();
    }

    private void DrawEconomy()
    {
        Header("골드");
        EditorGUILayout.LabelField("보유", GameSession.instance.Wallet.Gold.ToString());

        using (new EditorGUILayout.HorizontalScope())
        {
            gold = EditorGUILayout.IntField(gold);

            if (GUILayout.Button("추가", GUILayout.Width(80)))
                CheatCommands.AddGold(gold);
        }
    }

    private void DrawItems()
    {
        Header("아이템");

        string filter = EditorGUILayout.TextField("검색 (이름/ID)", itemFilter);

        if (itemNames == null || filter != itemFilter)
        {
            itemFilter = filter;
            RefreshItems();
        }

        if (items.Count == 0)
        {
            EditorGUILayout.HelpBox("검색 결과가 없습니다.", MessageType.None);
            return;
        }

        itemIndex = EditorGUILayout.Popup("아이템", itemIndex, itemNames);

        using (new EditorGUILayout.HorizontalScope())
        {
            itemCount = Mathf.Max(1, EditorGUILayout.IntField("개수", itemCount));

            if (GUILayout.Button("지급", GUILayout.Width(80)))
                CheatCommands.GiveItem(items[itemIndex].Id, itemCount);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            forgeLevel = EditorGUILayout.IntSlider("강화 수치", forgeLevel, 0, 10);

            if (GUILayout.Button("전부 적용", GUILayout.Width(80)))
                CheatCommands.SetForgeLevel(forgeLevel);
        }
    }

    private void DrawFavor()
    {
        Header("우호도");

        using (new EditorGUILayout.HorizontalScope())
        {
            npc = (NpcType)EditorGUILayout.EnumPopup(npc);
            EditorGUILayout.LabelField($"Lv.{GameSession.instance.Favor.GetLevel(npc)}", GUILayout.Width(50));
            favor = EditorGUILayout.IntField(favor, GUILayout.Width(60));

            if (GUILayout.Button("추가", GUILayout.Width(80)))
                CheatCommands.AddFavor(npc, favor);
        }
    }

    private void DrawRun()
    {
        Header("던전");

        bool inDungeon = CheatCommands.InDungeon;

        if (!inDungeon)
        {
            EditorGUILayout.HelpBox("던전에서만 사용할 수 있습니다.", MessageType.None);
            return;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            floor = Mathf.Max(1, EditorGUILayout.IntField("층 이동", floor));

            if (GUILayout.Button("이동", GUILayout.Width(80)))
                CheatCommands.JumpToFloor(floor);
        }

        if (blessingNames == null)
            blessingNames = DataManager.instance.Blessings.Select(b => $"{b.Id} {b.Name}").ToArray();

        using (new EditorGUILayout.HorizontalScope())
        {
            blessingIndex = EditorGUILayout.Popup("축복", blessingIndex, blessingNames);

            if (GUILayout.Button("받기", GUILayout.Width(80)))
                CheatCommands.AddBlessing(DataManager.instance.Blessings[blessingIndex].Id);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("몬스터 즉사"))
                CheatCommands.KillMonster();

            if (GUILayout.Button("체력 회복"))
                CheatCommands.HealPlayer();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("즉시 귀환"))
                CheatCommands.EndRun(RunEndReason.Return);

            if (GUILayout.Button("즉시 사망"))
                CheatCommands.EndRun(RunEndReason.Death);
        }
    }

    private void DrawTowers()
    {
        Header("탑");

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("다음 탑 정복"))
                CheatCommands.ConquerNextTower();

            if (GUILayout.Button("탑 진행 초기화"))
                CheatCommands.ResetTowers();
        }
    }

    private void DrawSystem()
    {
        Header("시스템");

        float scale = EditorGUILayout.Slider("게임 속도", timeScale, 0.1f, 8f);

        if (!Mathf.Approximately(scale, timeScale))
        {
            timeScale = scale;
            Time.timeScale = timeScale;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("저장 (마을)"))
                CheatCommands.SaveNow();

            if (GUILayout.Button("세이브 초기화") && EditorUtility.DisplayDialog("세이브 초기화", "세이브를 지우고 타이틀로 돌아갑니다.", "초기화", "취소"))
                CheatCommands.ResetSave();
        }
    }

    private void RefreshItems()
    {
        items.Clear();

        foreach (ItemInfo info in DataManager.instance.Items.OrderBy(i => i.Id))
        {
            if (!Matches(info))
                continue;

            items.Add(info);
        }

        itemNames = items.Select(i => $"{i.Id} {i.Name}").ToArray();
        itemIndex = Mathf.Clamp(itemIndex, 0, Mathf.Max(0, items.Count - 1));
    }

    private bool Matches(ItemInfo info)
    {
        if (string.IsNullOrEmpty(itemFilter))
            return true;

        return info.Name.Contains(itemFilter) || info.Id.ToString().Contains(itemFilter);
    }

    private static void Header(string title)
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }
}