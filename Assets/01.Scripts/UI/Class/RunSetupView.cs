using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunSetupView : MonoBehaviour
{
    [SerializeField] GameObject popup;
    [SerializeField] Button backButton;
    [SerializeField] ClassListView classList;
    [SerializeField] TowerFrontView front;
    [SerializeField] TowerMapView map;
    [SerializeField] TMP_Text hintText;
    private TowerInfo gateTower;
    private bool departing;

    private void Awake()
    {
        backButton.onClick.AddListener(Close);
        classList.SelectionChanged += HandleClassChanged;
        front.DoorClicked += HandleDoorClicked;
        map.TowerClicked += HandleTowerClicked;
    }

    public void Open()
    {
        departing = false;
        backButton.interactable = true;
        PopupManager.instance.Open(popup, Close);

        GameSession session = GameSession.instance;
        classList.Refresh(session);
        ShowTowers(session.Towers);
        RefreshInteractable();
    }

    public void Close()
    {
        if(departing)
            return;

        PopupManager.instance.Close(popup);
    }

    private void ShowTowers(TowerProgress progress)
    {
        gateTower = GetGateTower(progress);

        if(gateTower == null)
        {
            front.Hide();
            map.Show(progress);
            hintText.text = "오를 탑을 선택하세요";
            return;
        }

        map.Hide();
        front.Show(gateTower);
        hintText.text = "문을 눌러 출발";
    }
    private static TowerInfo GetGateTower(TowerProgress progress)
    {
        if(!progress.IsPrologueCollapsed)
            return progress.Prologue;

        if(progress.IsOriginRisen)
            return progress.Origin;

        return null;
    }
    private void RefreshInteractable()
    {
        bool ready = !departing && classList.Selected != null;
        front.SetInteractable(ready);
        map.SetInteractable(ready);
    }

    private void HandleClassChanged(ClassInfo info)
    {
        RefreshInteractable();
    }

    private void HandleDoorClicked()
    {
        TryDepart(gateTower, front.Open());
    }

    private void HandleTowerClicked(TowerInfo tower)
    {
        TryDepart(tower, map.Enter(tower));
    }

    private void TryDepart(TowerInfo tower, IEnumerator effect)
    {
        GameSession session = GameSession.instance;

        if(departing || classList.Selected == null || tower == null || !session.Towers.CanEnter(tower))
            return;

        departing = true;
        backButton.interactable = false;
        RefreshInteractable();

        session.SelectClass(classList.Selected);
        session.SelectTower(tower);
        session.BeginRun();

        StartCoroutine(DepartRoutine(effect));
    }

    private IEnumerator DepartRoutine(IEnumerator effect)
    {
        yield return effect;

        SceneLoader.instance.Load(SceneNames.Dungeon);
    }
}