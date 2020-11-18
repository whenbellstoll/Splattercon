using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSpell : MonoBehaviour
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
        if(time > 0.05f)
        {
            _gm.ApplyDamage(transform.position, 2);
            time = 0;
        }
        
    }
}
