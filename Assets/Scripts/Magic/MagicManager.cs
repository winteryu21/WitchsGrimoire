using System.Collections.Generic;
using UnityEngine;
using System;

public class MagicManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> startingMagicPrefabs;
    [SerializeField] private List<GameObject> availableMagicPrefabs;
    
    private List<MagicBase> acquiredMagics = new List<MagicBase>();
    private Dictionary<string, GameObject> magicPrefabLookup = new Dictionary<string, GameObject>();
    
    // 이벤트
    public event Action<MagicBase> OnMagicAcquired;
    public event Action<MagicBase> OnMagicLevelUp;
    
    private void Awake()
    {
        // 마법 프리팹 캐싱
        foreach (GameObject prefab in availableMagicPrefabs)
        {
            MagicBase magic = prefab.GetComponent<MagicBase>();
            if (magic != null)
            {
                magicPrefabLookup[magic.MagicName] = prefab;
            }
        }
    }
    
    private void Start()
    {
        // 시작 마법 부여
        foreach (GameObject magicPrefab in startingMagicPrefabs)
        {
            AcquireMagic(magicPrefab);
        }
    }
    
    public MagicBase AcquireMagic(GameObject magicPrefab)
    {
        if (magicPrefab == null) return null;
        
        GameObject magicInstance = Instantiate(magicPrefab, transform);
        MagicBase magic = magicInstance.GetComponent<MagicBase>();
        
        if (magic != null)
        {
            acquiredMagics.Add(magic);
            OnMagicAcquired?.Invoke(magic);
        }
        
        return magic;
    }
    
    public MagicBase AcquireMagicByName(string magicName)
    {
        if (magicPrefabLookup.TryGetValue(magicName, out GameObject magicPrefab))
        {
            return AcquireMagic(magicPrefab);
        }
        
        Debug.LogWarning($"Magic with name {magicName} not found");
        return null;
    }
    
    public bool UpgradeMagic(MagicBase magic)
    {
        if (magic == null || magic.IsMaxLevel) return false;
        
        bool success = magic.LevelUp();
        if (success)
        {
            OnMagicLevelUp?.Invoke(magic);
        }
        
        return success;
    }
    
    public List<MagicBase> GetAcquiredMagics()
    {
        return new List<MagicBase>(acquiredMagics);
    }
    
    // 새로운 업그레이드 옵션 제공
    public List<UpgradeOption> GetUpgradeOptions(int count)
    {
        List<UpgradeOption> options = new List<UpgradeOption>();
        
        // 현재 보유한 마법 업그레이드 옵션
        foreach (MagicBase magic in acquiredMagics)
        {
            if (!magic.IsMaxLevel)
            {
                options.Add(new UpgradeOption
                {
                    Type = UpgradeOptionType.LevelUp,
                    Magic = magic,
                    MagicName = magic.MagicName,
                    Description = $"{magic.MagicName} (Lv {magic.Level}) - {magic.GetNextLevelDescription()}"
                });
            }
        }
        
        // 새 마법 획득 옵션
        List<GameObject> unacquiredMagics = GetUnacquiredMagics();
        foreach (GameObject magicPrefab in unacquiredMagics)
        {
            MagicBase magicComponent = magicPrefab.GetComponent<MagicBase>();
            if (magicComponent != null)
            {
                options.Add(new UpgradeOption
                {
                    Type = UpgradeOptionType.NewMagic,
                    MagicPrefab = magicPrefab,
                    MagicName = magicComponent.MagicName,
                    Description = $"새 마법: {magicComponent.MagicName} - {magicComponent.Description}"
                });
            }
        }
        
        // 옵션 셔플 및 선택
        ShuffleList(options);
        return options.GetRange(0, Mathf.Min(count, options.Count));
    }
    
    private List<GameObject> GetUnacquiredMagics()
    {
        List<GameObject> unacquired = new List<GameObject>();
        
        foreach (GameObject magicPrefab in availableMagicPrefabs)
        {
            MagicBase magicComponent = magicPrefab.GetComponent<MagicBase>();
            if (magicComponent != null)
            {
                bool isAcquired = acquiredMagics.Exists(m => m.MagicName == magicComponent.MagicName);
                if (!isAcquired)
                {
                    unacquired.Add(magicPrefab);
                }
            }
        }
        
        return unacquired;
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public enum UpgradeOptionType
{
    NewMagic,
    LevelUp
}

public class UpgradeOption
{
    public UpgradeOptionType Type;
    public MagicBase Magic;
    public GameObject MagicPrefab;
    public string MagicName;
    public string Description;
}