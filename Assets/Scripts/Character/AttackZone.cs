using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);
        if (other.CompareTag("Enemy") && GetComponentInParent<Rigidbody>().velocity.y < 0)
        {
            var targetStats = other.GetComponent<CharacterStats>();
            targetStats.TakeDamage(1, targetStats);
            //Debug.Log("hit");
            GetComponentInParent<Rigidbody>().AddForce(0,100,0);
        }
    }
}
