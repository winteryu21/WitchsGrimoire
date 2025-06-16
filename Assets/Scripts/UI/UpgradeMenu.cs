using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeMenu : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject optionButtonPrefab;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private int upgradeOptionsCount = 3;
    
    private MagicManager magicManager;
    private PlayerStats playerStats;
    private List<GameObject> optionButtons = new List<GameObject>();
    
    private void Awake()
    {
        magicManager = FindAnyObjectByType<MagicManager>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        
        if (magicManager == null || playerStats == null)
        {
            Debug.LogError("Required components not found");
        }
        
        // 초기에는 업그레이드 패널 비활성화
        upgradePanel.SetActive(false);
    }
    
    private void OnEnable()
    {
        playerStats.OnLevelUp += ShowUpgradeOptions;
    }
    
    private void OnDisable()
    {
        playerStats.OnLevelUp -= ShowUpgradeOptions;
    }
    
    private void ShowUpgradeOptions()
    {
        // 게임 일시 정지
        GameManager.Instance.PauseGame();
        
        // 업그레이드 패널 활성화
        upgradePanel.SetActive(true);
        
        // 이전 옵션 버튼 정리
        ClearOptionButtons();
        
        // 업그레이드 옵션 가져오기
        List<UpgradeOption> options = magicManager.GetUpgradeOptions(upgradeOptionsCount);
        
        // 옵션 버튼 생성
        foreach (UpgradeOption option in options)
        {
            GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            
            // 버튼 텍스트 설정
            if (buttonText != null)
            {
                buttonText.text = option.Description;
            }
            
            // 아이콘 설정
            if (iconImage != null)
            {
                Sprite icon = null;
                
                if (option.Type == UpgradeOptionType.NewMagic && option.MagicPrefab != null)
                {
                    MagicBase magicComponent = option.MagicPrefab.GetComponent<MagicBase>();
                    if (magicComponent != null)
                    {
                        icon = magicComponent.Icon;
                    }
                }
                else if (option.Type == UpgradeOptionType.LevelUp && option.Magic != null)
                {
                    icon = option.Magic.Icon;
                }
                
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
            }
            
            // 버튼 이벤트 설정
            if (button != null)
            {
                button.onClick.AddListener(() => SelectUpgrade(option));
            }
            
            optionButtons.Add(buttonObj);
        }
    }
    
    private void SelectUpgrade(UpgradeOption option)
    {
        switch (option.Type)
        {
            case UpgradeOptionType.NewMagic:
                magicManager.AcquireMagic(option.MagicPrefab);
                break;
                
            case UpgradeOptionType.LevelUp:
                magicManager.UpgradeMagic(option.Magic);
                break;
        }
        
        // 업그레이드 패널 닫기
        CloseUpgradePanel();
    }
    
    private void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        
        // 게임 재개
        GameManager.Instance.ResumeGame();
    }
    
    private void ClearOptionButtons()
    {
        foreach (GameObject button in optionButtons)
        {
            Destroy(button);
        }
        
        optionButtons.Clear();
    }
}