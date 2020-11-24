using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BearTrap : MonoBehaviour
{
    private GameManager _gm;
    float time;

    public SpriteRenderer spriteRenderer;
    public Sprite closedSprite;
    public Sprite openSprite;

    enum State
    {
        TRIGGERED,
        OPEN,
        WAITING

    }

    State current;

    // Start is called before the first frame update
    void Start()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        current = State.OPEN;
        time = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (current)
        {
            //Bear Trap is open an ready to stop enemy
            case State.OPEN:
                if ( _gm.BearTrap(transform.position, 0))
                {
                    spriteRenderer.sprite = closedSprite;
                    time = 1;//Change for how long enemy is in trap
                    current = State.TRIGGERED;
                    _gm.ApplyDamage(transform.position, 50);
                }
                break;
                //Enemy is in bear trap 
            case State.TRIGGERED:
                time -= Time.deltaTime;
                _gm.BearTrap(transform.position, 0);
                if (time < 0)
                {                    
                    spriteRenderer.sprite = openSprite;
                    time = 5;//Change for how long until trap triggers again
                    Destroy(gameObject); // Makes bear traps one time use
                    current = State.WAITING;
                }
                break;
                //Trap opens and waits for enemy to leave
            case State.WAITING:
                time -= Time.deltaTime;
                if (time < 0)
                {
                    _gm.BearTrap(transform.position, 1);
                    time = 1;
                    current = State.OPEN;
                }
                break;
        }
    }
}
