using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonHandler : MonoBehaviour
{

    public void ShowSplatter(GameObject splatter)
    {
        splatter.SetActive(true);
    }

    public void HideSplatter(GameObject splatter)
    {
        splatter.SetActive(false);
    }

    public void StartButton()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void AboutButton()
    {

    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
