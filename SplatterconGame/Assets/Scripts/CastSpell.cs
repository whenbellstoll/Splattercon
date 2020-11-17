using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CastSpell : MonoBehaviour
{
    private Camera _mainCamera;

    private GameObject current;

    private float time;

    public GameObject spellOne;
    public GameObject spellTwo;
    


    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        current = spellOne;
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(1) && time > 0)
        {
            Destroy(Instantiate(current, mousePos, Quaternion.identity), 3.0f);
            time = 0;
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
