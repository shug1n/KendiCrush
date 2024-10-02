using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string sceneName;

    public void StartGameFonx()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGameFonx()
    {
        Application.Quit();
    }
}
