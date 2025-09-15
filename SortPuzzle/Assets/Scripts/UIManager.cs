using UnityEngine;
using UnityEngine.UI;
using TMPro; 
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Button restartButton;
    public Button nextLevelButton;


    void Start()
    {
        nextLevelButton.onClick.AddListener(GameManager.Instance.LoadNextLevel);
        restartButton.onClick.AddListener(GameManager.Instance.ReloadLevel);
        nextLevelButton.gameObject.SetActive(false);
    }

    public void UpdateLevelText(int level)
    {
        levelText.text = "Level " + level;
    }

    public void ShowNextLevelButton()
    {
        nextLevelButton.gameObject.SetActive(true);
    }
}