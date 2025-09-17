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
    [SerializeField] Button SettingButton;
    [SerializeField] Button nextLevelButton;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject nextLevelPanel;
    [SerializeField] TMP_Text SettingPaneltext;


    void Start()
    {
        HideNextLevelPanel();
        nextLevelButton.onClick.AddListener(()=>
        {
            HideNextLevelPanel();
            GameManager.Instance.LoadNextLevel();
        });
        restartButton.onClick.AddListener(GameManager.Instance.ReloadLevel);
        SettingButton.onClick.AddListener(()=>ShowNextLevelPanel("Option"));
        closeButton.onClick.AddListener(HideNextLevelPanel);
    }

    public void UpdateLevelText(int level)
    {
        levelText.text = "Level " + level;
    }
    void HideNextLevelPanel()
    {
        nextLevelButton.gameObject.SetActive(false);
        nextLevelPanel.SetActive(false);
    }    
    public void ShowNextLevelPanel(String text)
    {
        SettingPaneltext.text = text;
        nextLevelPanel.SetActive(true);
        nextLevelButton.transform.DOPunchScale(Vector3.one, .5f, 10, 1);
        nextLevelButton.gameObject.SetActive(true);
    }
}