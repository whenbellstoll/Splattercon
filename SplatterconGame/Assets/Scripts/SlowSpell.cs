using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowSpell : MonoBehaviour
{
    private GameManager _gm;
    float time;
    // Start is called before the first frame update
    void Start()
    {
        time = 0;
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 0.5f)
        {
            _gm.ApplySlow(transform.position, 0.1f, 10f);
            time = 0;
        }

    }
}
