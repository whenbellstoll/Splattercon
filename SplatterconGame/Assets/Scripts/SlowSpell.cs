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
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _gm.ApplySlow(transform.position, 0.98f);
        
    }

    void OnDestroy()
    {
        _gm.ApplySlow(transform.position, 1f);
    }
}
