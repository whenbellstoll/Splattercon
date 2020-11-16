using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CastSpell : MonoBehaviour
{
    private Camera _mainCamera;

    private GameObject current;

    public GameObject spellOne;
    public GameObject spellTwo;


    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        current = spellOne;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(1))
        {
            Destroy(Instantiate(current, mousePos, Quaternion.identity), 3.0f);
        }

    }

    public void DamageSpell()
    {
        current = spellOne;
    }

    public void SlowSpell()
    {
        current = spellTwo;
    }
}
