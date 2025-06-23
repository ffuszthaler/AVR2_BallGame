using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private BallLogic activeBallLogic;

    [SerializeField] private GameObject winPanel;

    [SerializeField] private GameObject lossPanel;

    [SerializeField] private TextMeshProUGUI winCountText;

    [SerializeField] private TextMeshProUGUI lossCountText;

    private const string WIN_COUNT_KEY = "WinCount";
    private const string LOSS_COUNT_KEY = "LossCount";

    private int winCount = 0;
    private int lossCount = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(false);

        LoadScores();
        UpdateScoreDisplay();
    }

    public void SetBallLogic(BallLogic ballLogic)
    {
        // unsubscribe from previous ball logic events
        if (activeBallLogic != null)
        {
            activeBallLogic.OnWin.RemoveListener(HandleWin);
            activeBallLogic.OnLoss.RemoveListener(HandleLoss);
        }

        activeBallLogic = ballLogic;

        // subscribe to new ball logic events
        if (activeBallLogic != null)
        {
            activeBallLogic.OnWin.AddListener(HandleWin);
            activeBallLogic.OnLoss.AddListener(HandleLoss);

            activeBallLogic.ResetBallState();
        }
        else
        {
            Debug.LogError("BallLogic is null.");
        }

        if (winPanel != null) winPanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(false);
    }

    private void LoadScores()
    {
        winCount = PlayerPrefs.GetInt(WIN_COUNT_KEY, 0);
        lossCount = PlayerPrefs.GetInt(LOSS_COUNT_KEY, 0);
        Debug.Log($"Scores loaded - Wins: {winCount}, Losses: {lossCount}");
    }

    private void SaveScores()
    {
        PlayerPrefs.SetInt(WIN_COUNT_KEY, winCount);
        PlayerPrefs.SetInt(LOSS_COUNT_KEY, lossCount);
        PlayerPrefs.Save();
        Debug.Log($"Scores saved - Wins: {winCount}, Losses: {lossCount}");
    }

    private void UpdateScoreDisplay()
    {
        if (winCountText != null)
        {
            winCountText.text = "Wins: " + winCount;
        }

        if (lossCountText != null)
        {
            lossCountText.text = "Losses: " + lossCount;
        }
    }

    // public methods to be called by BallLogic's Unity Events
    public void HandleWin()
    {
        winCount++;
        SaveScores();
        UpdateScoreDisplay();
        Debug.Log("Player Won!");

        if (winPanel != null) winPanel.SetActive(true);
        if (lossPanel != null) lossPanel.SetActive(false);
    }

    public void HandleLoss()
    {
        lossCount++;
        SaveScores();
        UpdateScoreDisplay();
        Debug.Log("Player Lost!");

        if (lossPanel != null) lossPanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Debug.Log("Restarting Game...");

        if (winPanel != null) winPanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(false);

        if (activeBallLogic != null)
        {
            activeBallLogic.ResetBallState();
        }
        else
        {
            Debug.LogWarning("no active labyrinth/ball found, ensure marker is tracked to spawn a new game.");
        }
    }

    // maybe call this from main menu?
    // public void ResetAllScores()
    // {
    //     winCount = 0;
    //     lossCount = 0;
    //     PlayerPrefs.DeleteKey(WIN_COUNT_KEY);
    //     PlayerPrefs.DeleteKey(LOSS_COUNT_KEY);
    //     PlayerPrefs.Save();
    //     UpdateScoreDisplay();
    //     Debug.Log("GameManager: All scores reset.");
    // }
}