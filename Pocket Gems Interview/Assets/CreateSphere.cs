using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSphere : MonoBehaviour
{
    [SerializeField]
    private GameObject _spherePrefab;
    [SerializeField]
    private Vector3 _spherePos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeSphere()
    {
        GameObject.Instantiate(_spherePrefab, _spherePos, Quaternion.identity);
    }
}
