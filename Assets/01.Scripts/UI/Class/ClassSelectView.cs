using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClassSelectView : MonoBehaviour
{
    [SerializeField] GameObject popup;
    [SerializeField] ClassCardView cardPrefab;
    [SerializeField] Transform cardRoot;
    [SerializeField] Button departButton;
    [SerializeField] Button backButton;

    private readonly List<ClassCardView> cards = new();
    private ClassCardView selected;

    private void Awake()
    {
        departButton.onClick.AddListener(Depart);
        backButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        PopupManager.instance.Open(popup, Close);
        BuildCards();
        RefreshLocks();
        Select(Find(GameSession.instance.CurrentClass));
    }

    public void Close()
    {
        PopupManager.instance.Close(popup);
    }

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
    private void RefreshLocks()
    {
        ClassTraining training = GameSession.instance.Training;

        foreach(var card in cards)
        {
            bool locked = !training.IsUnlocked(card.Info);
            string status = locked ? "훈련장에서 해금" : $"Lv.{training.GetProgress(card.Info).Level}";
            card.SetStatus(status, locked);
        }
    }
    private void Select(ClassCardView card)
    {
        if(card != null && card.IsLocked)
            return;

        if (selected != null)
            selected.SetSelected(false);

        selected = card;

        if(selected != null)
            selected.SetSelected(true);

        departButton.interactable = selected != null;
    }

    private ClassCardView Find(ClassInfo info)
    {
        foreach(var card in cards)
        {
            if(card.Info == info)
                return card;
        }

        return null;
    }

    private void Depart()
    {
        departButton.interactable = false;
        GameSession.instance.SelectClass(selected.Info);
        GameSession.instance.BeginRun();
        SceneManager.LoadScene(SceneNames.Dungeon);
    }
}