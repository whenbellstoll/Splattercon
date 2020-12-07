using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    private GameManager _gm;
    [SerializeField]
    private GameObject _impactParticles;

    // Start is called before the first frame update
    void Start()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = _gm.GetNearestEnemy(transform.position);
        transform.position = Vector2.MoveTowards(transform.position, pos, 10f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            _gm.ApplyDamage(transform.position, 20);
            Instantiate(_impactParticles, transform.position, _impactParticles.transform.rotation);
            Destroy(gameObject);
        }
    }
}
