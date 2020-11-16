using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CastSpell : MonoBehaviour
{
    private Camera _mainCamera;

    private int current;

    public GameObject[] spells;


    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        current = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKey("up") && current < spells.Length)
        {
            //current++;

        }
        else if (Input.GetKey("down") && current > 0)
        {
            //current--;
        }

        if (Input.GetMouseButtonDown(1))
        {
            //Destroy(, 3.0f);
            Instantiate(spells[2], mousePos, Quaternion.identity);
        }

    }

    public void DamageSpell()
    {
        current = 0;
    }

    public void SlowSpell()
    {
        current = 1;
    }
}
