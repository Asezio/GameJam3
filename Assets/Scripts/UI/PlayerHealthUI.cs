using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    private Text levelText;
    private Image healthSlider;
    private Image expSlider;

    void Awake()
    {
        levelText = transform.GetChild(2).GetComponent<Text>();
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
    }

    private void Update()
    {
        levelText.text = "Level: " + GameManager.Instance.playerStats.characterData.currentLevel;
        UpdateHealth();
        UpdateExp();
    }

    void UpdateHealth()
    {
        float sliderPresent = (float) GameManager.Instance.playerStats.CurrentHealth /
                              GameManager.Instance.playerStats.MaxHealth;
        healthSlider.fillAmount = sliderPresent;
    }
    void UpdateExp()
    {
        float sliderPresent = (float) GameManager.Instance.playerStats.characterData.currentExp /
                              GameManager.Instance.playerStats.characterData.baseExp;
        expSlider.fillAmount = sliderPresent;
    }
}
