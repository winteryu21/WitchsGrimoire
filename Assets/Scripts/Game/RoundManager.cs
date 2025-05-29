using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundManager : MonoBehaviour
{
    public TMP_Text roundText;      // 현재 라운드 UI
    public TMP_Text timerText;      // 남은 시간 UI
    public EnemySpawner enemySpawner; // 적 스포너

    public int maxRounds = 20;
    public float roundTime = 60f;

    private int currentRound = 1;
    private float timeRemaining;
    private bool isRunning = false;

    void Start()
    {
        StartRound();
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            NextRound();
        }

        UpdateUI();
    }

    void StartRound()
    {
        // 라운드 관리
        timeRemaining = roundTime;
        isRunning = true;
        UpdateUI();
        Debug.Log($"Round {currentRound} 시작!");

        int enemyCount = 5 + (currentRound - 1) * 2; // 라운드에 따라 적 수 증가
        enemySpawner.SpawnEnemies(enemyCount);
    }

    void NextRound()
    {
        if (currentRound >= maxRounds)
        {
            EndRounds();
            return;
        }

        currentRound++;
        StartRound();
    }

    void EndRounds()
    {
        isRunning = false;
        Debug.Log("모든 라운드가 종료되었습니다.");
        // GameManager와 연동하거나 결과 UI 표시 가능
    }

    void UpdateUI()
    {
        roundText.text = $"Round {currentRound}";
        timerText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
    }
}
