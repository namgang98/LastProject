using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingUI : MonoBehaviour
{
    [Header("팝업")]
    [SerializeField] GameObject popup;
    [SerializeField] Button closeButton;

    [Header("카드")]
    [SerializeField] ClassCardView cardPrefab;
    [SerializeField] Transform cardRoot;

    [Header("상세")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text pointText;
    [SerializeField] TrainingStatRowView[] statRows;

    [Header("훈련")]
    [SerializeField] GameObject trainGroup;
    [SerializeField] Button trainButton;
    [SerializeField] TMP_Text trainText;

    [Header("초기화")]
    [SerializeField] Button resetButton;
    [SerializeField] TMP_Text resetText;

    [Header("해금")]
    [SerializeField] GameObject unlockGroup;
    [SerializeField] TMP_Text conditionText;
    [SerializeField] Color metColor = new(0.59f, 0.67f, 0.43f, 1f);
    [SerializeField] Color unmetColor = new(0.69f, 0.21f, 0.16f, 1f);
    [SerializeField] Button unlockButton;
    [SerializeField] TMP_Text unlockText;

    private readonly List<ClassCardView> cards = new();
    private ClassTraining training;
    private Wallet wallet;
    private ClassCardView selected;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        trainButton.onClick.AddListener(Train);
        unlockButton.onClick.AddListener(Unlock);
        resetButton.onClick.AddListener(ResetPoints);

        foreach(var row in statRows)
        {
            row.PlusClicked += AddPoint;
        }
    }

    private void OnEnable()
    {
        EventManager.instance.AddListener<OnChangeGold>(HandleGoldChanged);
    }

    private void OnDisable()
    {
        if(!EventManager.HasInstance)
            return;

        EventManager.instance.RemoveListener<OnChangeGold>(HandleGoldChanged);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void Open()
    {
        GameSession session = GameSession.instance;
        wallet = session.Wallet;

        Unsubscribe();
        training = session.Training;
        training.Changed += HandleTrainingChanged;

        PopupManager.instance.Open(popup, Close);
        BuildCards();
        RefreshCards();
        Select(Find(session.CurrentClass));
    }

    public void Close()
    {
        Unsubscribe();
        PopupManager.instance.Close(popup);
    }

    private void Unsubscribe()
    {
        if(training == null)
            return;

        training.Changed -= HandleTrainingChanged;
        training = null;
    }

    #region 카드
    private void BuildCards()
    {
        if(cards.Count > 0)
            return;

        var classes = new List<ClassInfo>(DataManager.instance.Classes);
        classes.Sort((a, b) => a.Id.CompareTo(b.Id));

        foreach(ClassInfo info in classes)
        {
            ClassCardView card = Instantiate(cardPrefab, cardRoot);
            card.Bind(info);
            card.Clicked += Select;
            cards.Add(card);
        }
    }

    private void RefreshCards()
    {
        TowerProgress towers = GameSession.instance.Towers;

        foreach(var card in cards)
        {
            ClassInfo info = card.Info;
            bool fallen = towers.IsFallen(info);
            card.gameObject.SetActive(!fallen);

            if(fallen)
                continue;

            if(training.IsUnlocked(info))
                card.SetState(ClassAvailability.Available, $"Lv.{training.GetProgress(info).Level}");
            else
                card.SetState(ClassAvailability.Locked, $"{info.UnlockFloor}층 도달");
        }
    }

    private void Select(ClassCardView card)
    {
        if(card == null)
            return;

        if(selected != null)
            selected.SetSelected(false);

        selected = card;
        selected.SetSelected(true);
        RefreshDetail();
    }

    private ClassCardView Find(ClassInfo info)
    {
        ClassCardView first = null;

        foreach(var card in cards)
        {
            if(!card.gameObject.activeSelf)
                continue;

            if(card.Info == info)
                return card;

            if(first == null)
                first = card;
        }

        return first;
    }
    #endregion

    #region 상세
    private void RefreshDetail()
    {
        if(selected == null || training == null)
            return;

        ClassInfo info = selected.Info;
        ClassProgress progress = training.GetProgress(info);
        bool unlocked = training.IsUnlocked(info);

        nameText.text = info.Name;
        levelText.text = $"Lv.{progress.Level}";
        pointText.text = unlocked ? $"남은 포인트 {progress.FreePoints}" : string.Empty;

        RefreshStats(info, progress);

        trainGroup.SetActive(unlocked);
        unlockGroup.SetActive(!unlocked);

        if(unlocked)
            RefreshTrain(info);
        else
            RefreshUnlock(info);
    }

    private void RefreshStats(ClassInfo info, ClassProgress progress)
    {
        foreach(var row in statRows)
        {
            if(!DataManager.instance.TryGetTrainingStat(row.Stat, out TrainingStatInfo statInfo))
            {
                row.Hide();
                continue;
            }

            row.Show(statInfo, progress.GetPoints(row.Stat), training.CanAddPoint(info, row.Stat));
        }
    }

    private void RefreshTrain(ClassInfo info)
    {
        if(!training.TryGetTrainCost(info, out int cost))
        {
            trainText.text = "최대 레벨";
            trainButton.interactable = false;
            return;
        }

        trainText.text = $"훈련하기 {cost}G";
        trainButton.interactable = wallet.Gold >= cost;
        RefreshReset(info);
    }

    private void RefreshUnlock(ClassInfo info)
    {
        conditionText.text = $"조건 : {info.UnlockFloor}층 도달 (최고 {training.BestFloor}층)";
        conditionText.color = training.MeetsFloor(info) ? metColor : unmetColor;
        unlockText.text = $"해금 {info.UnlockCost}G";
        unlockButton.interactable = training.CanUnlock(info, wallet);
    }

    private void RefreshReset(ClassInfo info)
    {
        int scrolls = GameSession.instance.CountConsumable(ConsumeType.ResetPoints);
        bool spent = training.GetProgress(info).SpentPoints > 0;

        resetText.text = $"초기화 (두루마리 {scrolls}개)";
        resetButton.interactable = spent && scrolls > 0;
    }

    private void ResetPoints()
    {
        if(selected == null)
            return;

        ClassInfo info = selected.Info;

        if(!training.IsUnlocked(info) || training.GetProgress(info).SpentPoints <= 0)
            return;

        GameSession session = GameSession.instance;

        if(!session.TryTakeConsumable(ConsumeType.ResetPoints, out _))
            return;

        training.ResetPoints(info);
        session.SaveInVillage();
    }
    #endregion

    #region 버튼
    private void Train()
    {
        if(selected == null || !training.TryTrain(selected.Info, wallet))
            return;

        GameSession.instance.SaveInVillage();
    }

    private void Unlock()
    {
        if(selected == null || !training.TryUnlock(selected.Info, wallet))
            return;

        GameSession.instance.SaveInVillage();
    }

    private void AddPoint(TrainingStat stat)
    {
        if(selected == null || !training.TryAddPoint(selected.Info, stat))
            return;

        GameSession.instance.SaveInVillage();
    }
    #endregion

    private void HandleTrainingChanged(ClassInfo changed)
    {
        RefreshCards();
        RefreshDetail();
    }

    private void HandleGoldChanged(OnChangeGold e)
    {
        if(!popup.activeSelf)
            return;

        RefreshDetail();
    }
}