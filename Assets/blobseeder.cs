using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blobseeder : MonoBehaviour
{

    public uint seed;
    public GameObject blobs1, blobs2, blobs3;
    ParticleSystem part1, part2, part3;

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

        part1 = blobs1.GetComponent<ParticleSystem>();
        part2 = blobs2.GetComponent<ParticleSystem>();
        part3 = blobs3.GetComponent<ParticleSystem>();

    }

    private void OnEnable()
    {
        seed += 1;
        part1.randomSeed = seed;
        part2.randomSeed = seed;
        part3.randomSeed = seed;


    }
}
