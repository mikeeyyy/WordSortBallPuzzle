using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Button restartButton;
    [SerializeField] Button settingButton;
    [SerializeField] Button nextLevelButton;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject nextLevelPanel;
    [SerializeField] TMP_Text SettingPaneltext;


    private void Awake()
    {
        GameEvents.OnLoadLevel += UpdateLevelText;
        GameEvents.OnLevelComplete += ShowVictoryPanel;

        restartButton.onClick.AddListener(() => GameEvents.OnReloadLevel?.Invoke());
        nextLevelButton.onClick.AddListener(() =>
        {
            HidePanel();
            GameEvents.OnLoadNextLevel?.Invoke();
        });
        settingButton.onClick.AddListener(() => ShowPanel("Options"));
        closeButton.onClick.AddListener(HidePanel);
    }
    private void OnDestroy()
    {
        GameEvents.OnLoadLevel -= UpdateLevelText;
        GameEvents.OnLevelComplete -= ShowVictoryPanel;
    }
    void Start()
    {
        HidePanel();
    }

    public void UpdateLevelText(int level)
    {
        levelText.text = "Level " + (level + 1);
    }

    private void ShowVictoryPanel()
    {
        ShowPanel("Victory!");
    }
    private void ShowPanel(string title)
    {
        SettingPaneltext.text = title;
        nextLevelPanel.SetActive(true);
        nextLevelButton.gameObject.SetActive(true);
        nextLevelButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 10, 1);
    }
    private void HidePanel()
    {
        nextLevelButton.gameObject.SetActive(false);
        nextLevelPanel.SetActive(false);
    }
}