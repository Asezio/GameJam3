using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Attack", menuName = "Charater Stats/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;
    public float skillRange;
    public float coolDown;
    public float minDamage;
    public float MaxDamage;

    public float criticleMultiplier;
    public float criticalChance;
}
