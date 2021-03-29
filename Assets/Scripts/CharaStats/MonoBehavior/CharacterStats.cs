using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterStats : MonoBehaviour
{
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;
    public AttackData_SO attackData;

    public event Action<int, int> UpdateHealthBarOnAttack;
    
    [HideInInspector]
    public bool isCritical;

    private void Awake()
    {
        if (templateData != null)
            characterData = Instantiate(templateData);
    }

    #region Read From Data_SO
    public int MaxHealth
    {
        get { if (characterData != null) return characterData.maxHealth; else return 0; }
        set { characterData.maxHealth = value; }
    }
    public int CurrentHealth
    {
        get { if (characterData != null) return characterData.currentHealth; else return 0; }
        set { characterData.currentHealth = value; }
    }
    public int BaseDefence
    {
        get { if (characterData != null) return characterData.baseDefence; else return 0; }
        set { characterData.baseDefence = value; }
    }
    public int CurrentDefence
    {
        get { if (characterData != null) return characterData.CurrentDefence; else return 0; }
        set { characterData.CurrentDefence = value; }
    }
    #endregion

    #region Character Combat

    public void TakeDamage(CharacterStats attacker,CharacterStats defender)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence,0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        if(attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("hit");
        }
        //update UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth,MaxHealth);
        
        //经验update
        if (CurrentHealth <= 0)
        {
            attacker.characterData.UpdateExp(characterData.killPoint);
        }
    }

    public void TakeDamage(int damage,CharacterStats defender)
    {
        int currentDamage = Mathf.Max(damage - defender.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage,0);
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth,MaxHealth);
        
        if(CurrentHealth <= 0)
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.MaxDamage);
        if(isCritical)
        {
            coreDamage *= attackData.criticleMultiplier;
        }
        return (int)coreDamage;
    }

    #endregion
}
