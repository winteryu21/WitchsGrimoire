using UnityEngine;
using System;

public abstract class MagicBase : MonoBehaviour
{
    [SerializeField] protected string magicName;
    [SerializeField] protected string description;
    [SerializeField] protected Sprite icon;
    [SerializeField] protected int level = 1;
    [SerializeField] protected int maxLevel = 5;
    
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float baseArea;
    [SerializeField] protected float baseCooldown;
    
    protected PlayerStats playerStats;
    protected float lastActivationTime;
    protected bool isActive = true;
    
    // 이벤트
    public event Action OnMagicActivated;
    public event Action OnLevelUp;
    
    // 프로퍼티
    public string MagicName => magicName;
    public string Description => description;
    public Sprite Icon => icon;
    public int Level => level;
    public int MaxLevel => maxLevel;
    public bool IsMaxLevel => level >= maxLevel;
    public bool IsActive => isActive;
    
    protected virtual void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError($"Magic {magicName} could not find PlayerStats component");
        }
    }
    
    protected virtual void Start()
    {
        // 자식 클래스에서 오버라이드 가능
    }
    
    protected virtual void Update()
    {
        if (!isActive || GameManager.Instance.IsPaused) return;
        
        // 쿨다운 확인 및 마법 발동
        if (Time.time >= lastActivationTime + GetCurrentCooldown())
        {
            ActivateMagic();
            lastActivationTime = Time.time;
        }
    }
    
    public abstract void ActivateMagic();
    
    public virtual bool LevelUp()
    {
        if (level >= maxLevel) return false;
        
        level++;
        OnLevelUp?.Invoke();
        return true;
    }
    
    public virtual void SetActive(bool state)
    {
        isActive = state;
    }
    
    public virtual float GetCurrentDamage()
    {
        return baseDamage * (1 + (level - 1) * 0.2f) * playerStats.GetTotalDamageMultiplier();
    }
    
    public virtual float GetCurrentArea()
    {
        return baseArea * (1 + (level - 1) * 0.1f) * playerStats.GetTotalAreaMultiplier();
    }
    
    public virtual float GetCurrentCooldown()
    {
        return baseCooldown * (1 - (level - 1) * 0.05f) * playerStats.GetTotalCooldownReduction();
    }
    
    public virtual string GetNextLevelDescription()
    {
        if (IsMaxLevel) return "최대 레벨";
        
        return $"다음 레벨: 데미지 +20%, 범위 +10%, 쿨다운 -5%";
    }
}