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

    public void AboutButton(GameObject aboutMenu)
    {

		gameObject.transform.parent.gameObject.SetActive(false);
		aboutMenu.SetActive(true);

    }
	
	public void BackButton(GameObject mainMenu)
    {

		gameObject.transform.parent.gameObject.SetActive(false);
		mainMenu.SetActive(true);

    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void MenuButton()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);

    }

    public void DestroySceneData()
    {
        if (GameObject.Find("SceneData") != null)
        {
            SceneData sceneData = GameObject.Find("SceneData").GetComponent<SceneData>();
            Destroy(sceneData.gameObject);
        }
    }

}
