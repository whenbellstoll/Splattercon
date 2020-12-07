using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonTrap : MonoBehaviour
{

    public GameObject projectile;

    private GameManager _gm;
    private float timer;

    private AudioSource _audioSource;
    public AudioClip _shoot;

    // Start is called before the first frame update
    void Start()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _audioSource = GetComponent<AudioSource>();
        timer = 5;
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 lookAt = _gm.GetNearestEnemy(transform.position);

        if (lookAt != Vector3.zero && Vector2.Distance(lookAt, transform.position) < 10f)
        {
            float AngleRad = Mathf.Atan2(lookAt.y - this.transform.position.y, lookAt.x - this.transform.position.x);

            float AngleDeg = (180 / Mathf.PI) * AngleRad + 270;

            transform.rotation = Quaternion.Euler(0, 0, AngleDeg);

            timer += Time.deltaTime;
            if (timer > 1.5f)
            {
                _audioSource.PlayOneShot(_shoot);
                Destroy(Instantiate(projectile, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation), 2.0f);
                timer = 0;
            }
        }
    }
}
