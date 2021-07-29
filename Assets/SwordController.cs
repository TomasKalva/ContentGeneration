using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Movement;

[RequireComponent(typeof(Collider)),RequireComponent(typeof(Animator))]
public class SwordController : MonoBehaviour
{
    Animator animator;
    [SerializeField]
    Movement owner;

    // Start is called before the first frame update
    void Awake()
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
            owner.Jump(15f);// JumpNoChecks(20f);
        }
    }
}
