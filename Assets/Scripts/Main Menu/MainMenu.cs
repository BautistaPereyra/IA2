using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void MM()
    {
        LoadLevelMainMenu();
    }
    public void Jugar()
    {
        LoadLevelPlay();
    }
    public void Quit()
    {
        LoadLevelQuit();
    }
    void LoadLevelMainMenu()
    {
        Debug.Log("Menu principal");
        SceneManager.LoadScene("MainMenu");
    }
    void LoadLevelPlay()
    {
        Debug.Log("Gameplay");
        SceneManager.LoadScene(1);
    }
    void LoadLevelQuit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
