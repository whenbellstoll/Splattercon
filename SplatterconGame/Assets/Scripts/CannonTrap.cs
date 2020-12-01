﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonTrap : MonoBehaviour
{

    public GameObject projectile;

    private GameManager _gm;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        timer = 5;
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 lookAt = _gm.GetNearestEnemy(transform.position);

        if (lookAt != Vector3.zero && Vector2.Distance(lookAt, transform.position) < 5f)
        {
            float AngleRad = Mathf.Atan2(lookAt.y - this.transform.position.y, lookAt.x - this.transform.position.x);

            float AngleDeg = (180 / Mathf.PI) * AngleRad;

            transform.rotation = Quaternion.Euler(0, 0, AngleDeg);

            timer += Time.deltaTime;
            if (timer > 1.5f)
            {
                Destroy(Instantiate(projectile), 5.0f);
                timer = 0;
            }
        }
    }
}