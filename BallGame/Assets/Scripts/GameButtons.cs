using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameButtons : MonoBehaviour
{
    private string currentSceneName;

    private void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Get Active Scene");
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync("GameScene");
        Debug.Log("Start Game");
    }

    public void BackToMenu()
    {
        SceneManager.LoadSceneAsync("MenuScene");
        Debug.Log("Back to Menu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(currentSceneName);
        Debug.Log("Restart Game");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Reset Stats");
    }
}