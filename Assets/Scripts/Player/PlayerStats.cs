using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // 플레이어 기본 스탯
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float healthRegen = 0.5f;
    [SerializeField] private float baseSpeedMultiplier = 1f;
    [SerializeField] private float baseDamageMultiplier = 1f;
    [SerializeField] private float baseAreaMultiplier = 1f;
    [SerializeField] private float baseCooldownReduction = 1f;
    [SerializeField] private int level = 1;
    [SerializeField] private int experience = 0;
    [SerializeField] private int experienceToNextLevel = 10;
    
    // 피격 관련 설정G
    [SerializeField] private float invincibilityDuration = 1.5f;

    // 플레이어 상태 플래그
    private bool isInvulnerable = false;
    private bool canMove = true;
    private bool isDead = false;

    // 애니메이션 관련 컴포넌트
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // 버프 시스템 추가
    private Dictionary<string, float> speedBuffs = new Dictionary<string, float>();
    private Dictionary<string, float> damageBuffs = new Dictionary<string, float>();
    private Dictionary<string, float> areaBuffs = new Dictionary<string, float>();
    private Dictionary<string, float> cooldownBuffs = new Dictionary<string, float>();

    // 이벤트
    public event Action OnHealthChanged;
    public event Action OnLevelUp;
    public event Action OnDeath;
    public event Action<int> OnExperienceGained;
    public event Action<string, float> OnBuffAdded;
    public event Action<string> OnBuffRemoved;
    public event Action OnPlayerHit;  // 피격 시 발생하는 이벤트 추가

    // 프로퍼티
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public int Level => level;
    public int Experience => experience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public bool CanMove => canMove;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        
        // 애니메이션 관련 컴포넌트 가져오기
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // 체력 자동 회복
        if (currentHealth < maxHealth && !isDead)
        {
            RegenerateHealth();
        }
    }

    private void RegenerateHealth()
    {
        currentHealth += healthRegen * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        // 무적 상태나 사망 상태면 데미지를 받지 않음
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke();
        OnPlayerHit?.Invoke();
        
        // 피격 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // 무적 시간 시작 (깜빡임 효과 제거)
        StartCoroutine(InvincibilityFrames());

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // 무적 시간 처리 코루틴 (깜빡임 효과 제거)
    IEnumerator InvincibilityFrames()
    {
        isInvulnerable = true;
        
        // 깜빡임 효과 제거 (애니메이션으로 처리)
        yield return new WaitForSeconds(invincibilityDuration);
        
        isInvulnerable = false;
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void GainExperience(int amount)
    {
        experience += amount;
        OnExperienceGained?.Invoke(amount);

        while (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        experience -= experienceToNextLevel;
        experienceToNextLevel = CalculateNextLevelExperience();
        
        // 레벨업 이벤트 발생
        OnLevelUp?.Invoke();
        
        // 레벨업 시 체력 회복
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
        
        // 최고 레벨 업데이트
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHighestLevel(level);
        }
    }

    private int CalculateNextLevelExperience()
    {
        // 간단한 레벨업 경험치 공식 예시
        return experienceToNextLevel + level * 5;
    }

    private void Die()
    {
        isDead = true;
        canMove = false;
        
        // 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 사망 이벤트 발생
        OnDeath?.Invoke();
        
        // 게임 오버 처리
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    // 스탯 수정자 메서드
    public void IncreaseMaxHealth(float amount)
    {
        float healthPercent = currentHealth / maxHealth;
        maxHealth += amount;
        currentHealth = healthPercent * maxHealth;
        OnHealthChanged?.Invoke();
    }

    // 버프 관련 메서드
    public void AddSpeedBuff(string buffId, float multiplier)
    {
        speedBuffs[buffId] = multiplier;
        OnBuffAdded?.Invoke("speed", multiplier);
    }
    
    public void AddDamageBuff(string buffId, float multiplier)
    {
        damageBuffs[buffId] = multiplier;
        OnBuffAdded?.Invoke("damage", multiplier);
    }
    
    public void AddAreaBuff(string buffId, float multiplier)
    {
        areaBuffs[buffId] = multiplier;
        OnBuffAdded?.Invoke("area", multiplier);
    }
    
    public void AddCooldownBuff(string buffId, float multiplier)
    {
        cooldownBuffs[buffId] = multiplier;
        OnBuffAdded?.Invoke("cooldown", multiplier);
    }
    
    public void RemoveBuff(string buffId)
    {
        bool wasRemoved = false;
        
        if (speedBuffs.Remove(buffId))
        {
            wasRemoved = true;
            OnBuffRemoved?.Invoke("speed");
        }
        
        if (damageBuffs.Remove(buffId))
        {
            wasRemoved = true;
            OnBuffRemoved?.Invoke("damage");
        }
        
        if (areaBuffs.Remove(buffId))
        {
            wasRemoved = true;
            OnBuffRemoved?.Invoke("area");
        }
        
        if (cooldownBuffs.Remove(buffId))
        {
            wasRemoved = true;
            OnBuffRemoved?.Invoke("cooldown");
        }
    }

    public float GetTotalSpeedMultiplier()
    {
        float totalMultiplier = baseSpeedMultiplier;
        foreach (float buff in speedBuffs.Values)
        {
            totalMultiplier += buff;
        }
        return Mathf.Max(0.1f, totalMultiplier); // 최소 속도 보장
    }
    
    public float GetTotalDamageMultiplier()
    {
        float totalMultiplier = baseDamageMultiplier;
        foreach (float buff in damageBuffs.Values)
        {
            totalMultiplier += buff;
        }
        return totalMultiplier;
    }
    
    public float GetTotalAreaMultiplier()
    {
        float totalMultiplier = baseAreaMultiplier;
        foreach (float buff in areaBuffs.Values)
        {
            totalMultiplier += buff;
        }
        return totalMultiplier;
    }
    
    public float GetTotalCooldownReduction()
    {
        float totalReduction = baseCooldownReduction;
        foreach (float buff in cooldownBuffs.Values)
        {
            totalReduction += buff;
        }
        // 쿨다운 감소는 0.1보다 작으면 안됨 (90% 감소 한계)
        return Mathf.Max(0.1f, totalReduction);
    }

    // 상태 변경 메서드
    public void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }
    
    // 게임 저장/로드를 위한 데이터 직렬화 (옵션)
    [Serializable]
    public class PlayerData
    {
        public float maxHealth;
        public float currentHealth;
        public int level;
        public int experience;
        public int experienceToNextLevel;
    }
    
    public PlayerData GetSaveData()
    {
        return new PlayerData
        {
            maxHealth = this.maxHealth,
            currentHealth = this.currentHealth,
            level = this.level,
            experience = this.experience,
            experienceToNextLevel = this.experienceToNextLevel
        };
    }
    
    public void LoadSaveData(PlayerData data)
    {
        if (data == null) return;
        
        maxHealth = data.maxHealth;
        currentHealth = data.currentHealth;
        level = data.level;
        experience = data.experience;
        experienceToNextLevel = data.experienceToNextLevel;
        
        OnHealthChanged?.Invoke();
    }
}