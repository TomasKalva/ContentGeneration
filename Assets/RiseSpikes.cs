using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiseSpikes : MonoBehaviour
{
    [SerializeField]
    private Animator spikes;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit");
        if (other.tag == "Player")
        {
            spikes.SetBool("riseSpikes", true);
        }
    }
}
