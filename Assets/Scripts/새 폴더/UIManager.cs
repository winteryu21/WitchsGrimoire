using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UIManager Instance { get; private set; }
    
    [Header("Player Stats UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;
    
    // 참조
    private PlayerStats playerStats;
    
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 플레이어 스탯 참조 가져오기
        playerStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        
        if (playerStats != null)
        {
            // 이벤트 구독
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnExperienceGained += UpdateExperienceBar;
            playerStats.OnLevelUp += UpdateLevelText;
        }
        else
        {
            Debug.LogError("UIManager couldn't find PlayerStats component");
        }
        
        // 초기 UI 업데이트
        UpdateAllUI();
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 취소
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnExperienceGained -= UpdateExperienceBar;
            playerStats.OnLevelUp -= UpdateLevelText;
        }
    }
    
    public void UpdateAllUI()
    {
        UpdateHealthBar();
        UpdateExperienceBar(0); // 0은 이벤트를 위한 더미 값
        UpdateLevelText();
    }
    
    private void UpdateHealthBar()
    {
        if (healthBar != null && playerStats != null)
        {
            healthBar.value = playerStats.CurrentHealth / playerStats.MaxHealth;
        }
    }
    
    private void UpdateExperienceBar(int amount)
    {
        if (experienceBar != null && playerStats != null)
        {
            experienceBar.value = (float)playerStats.Experience / playerStats.ExperienceToNextLevel;
        }
    }
    
    private void UpdateLevelText()
    {
        if (levelText != null && playerStats != null)
        {
            levelText.text = $"Lv. {playerStats.Level}";
        }
    }
}