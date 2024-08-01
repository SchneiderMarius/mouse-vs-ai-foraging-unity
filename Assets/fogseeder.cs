using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fogseeder : MonoBehaviour
{

    public uint seed;
    ParticleSystem fog;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        seed = 0;

        fog = this.GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        seed += 1;
        fog.randomSeed = seed;

    }
}
