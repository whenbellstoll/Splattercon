using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetScore : MonoBehaviour
{


    private SceneData sceneData;
    private Text text;


    // Start is called before the first frame update
    void Start()
    {

        sceneData = GameObject.Find("SceneData").GetComponent<SceneData>();
        text = this.gameObject.GetComponent<Text>();

        Debug.Log(sceneData.GetScore().ToString());

        if (sceneData != null)
        {
            text.text = "Survived " + sceneData.GetScore().ToString() + " Rounds!";
        }

        Destroy(sceneData.gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
