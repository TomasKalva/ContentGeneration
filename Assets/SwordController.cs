using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovingSphere;

[RequireComponent(typeof(Collider)),RequireComponent(typeof(Animator))]
public class SwordController : MonoBehaviour
{
    Animator animator;
    [SerializeField]
    MovingSphere owner;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            animator.SetTrigger("AttackDown");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            owner.PerformAbility(new JumpAbility(15f));// JumpNoChecks(20f);
        }
    }
}
