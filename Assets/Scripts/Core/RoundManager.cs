using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [System.Serializable]
    public class Round
    {
        public int roundNumber;
        public int enemyCount;
        public float spawnInterval = 2f;
        public float roundDuration = 120f; // 라운드 제한 시간 (초)
        public GameObject[] enemyPrefabs;
        public float enemyHealthMultiplier = 1f;
        public float enemySpeedMultiplier = 1f;
    }

    [Header("Round Settings")]
    public List<Round> rounds = new List<Round>();
    public Transform[] spawnPoints;
    public float timeBetweenRounds = 3f; // 라운드 사이 대기 시간
    
    [Header("UI References")]
    public TextMeshProUGUI roundText;     // 라운드 번호만 표시
    public TextMeshProUGUI enemyCountText; // 남은 적 수 표시
    public TextMeshProUGUI timerText;     // 타이머 표시 텍스트
    
    [Header("Events")]
    public UnityEvent onRoundStart;
    public UnityEvent onRoundComplete;
    public UnityEvent onAllRoundsComplete;
    
    private int currentRound = 0;
    private int remainingEnemies = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float roundTimer = 0f;
    private bool isRoundActive = false;
    
    void Start()
    {
        // 첫 라운드 시작
        StartNextRound();
    }
    
    void Update()
    {
        if (isRoundActive)
        {
            // 라운드 타이머 업데이트
            UpdateRoundTimer();
            
            // UI 업데이트
            UpdateUI();
        }
    }
    
    private void UpdateRoundTimer()
    {
        if (roundTimer > 0)
        {
            roundTimer -= Time.deltaTime;
            
            // 시간이 다 되면 라운드 종료
            if (roundTimer <= 0)
            {
                CompleteRound();
            }
        }
    }
    
    private void UpdateUI()
    {
        // 라운드 텍스트 업데이트
        if (roundText != null && currentRound < rounds.Count)
        {
            roundText.text = $"Round {rounds[currentRound].roundNumber}";
        }
        
        // 적 수 텍스트 업데이트
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemies: {remainingEnemies}";
        }
        
        // 타이머 텍스트 업데이트
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(roundTimer / 60);
            int seconds = Mathf.FloorToInt(roundTimer % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    public void StartNextRound()
    {
        if (currentRound < rounds.Count)
        {
            isRoundActive = true;
            roundTimer = rounds[currentRound].roundDuration;
            
            StartCoroutine(SpawnEnemiesForRound(rounds[currentRound]));
            onRoundStart?.Invoke();
            
            // UI 초기화
            UpdateUI();
            
            Debug.Log($"Round {rounds[currentRound].roundNumber} Started");
        }
        else
        {
            // 모든 라운드 완료
            isRoundActive = false;
            onAllRoundsComplete?.Invoke();
            
            // UI 업데이트
            if (roundText != null)
                roundText.text = "All Rounds Complete!";
            if (enemyCountText != null)
                enemyCountText.text = "";
            if (timerText != null)
                timerText.text = "00:00";
                
            Debug.Log("All rounds complete!");
        }
    }
    
    IEnumerator SpawnEnemiesForRound(Round round)
    {
        remainingEnemies = round.enemyCount;
        
        for (int i = 0; i < round.enemyCount; i++)
        {
            // 라운드가 여전히 활성화 상태인지 확인
            if (!isRoundActive)
                break;
                
            // 랜덤 적 프리팹 선택
            GameObject enemyPrefab = round.enemyPrefabs[Random.Range(0, round.enemyPrefabs.Length)];
            
            // 랜덤 스폰 포인트
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // 적 생성
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            
            // 적 설정
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.SetMultipliers(round.enemyHealthMultiplier, round.enemySpeedMultiplier);
                enemyController.OnEnemyDeath += HandleEnemyDeath;
            }
            
            activeEnemies.Add(enemy);
            
            // 스폰 간격만큼 대기
            yield return new WaitForSeconds(round.spawnInterval);
        }
    }
    
    void HandleEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        remainingEnemies--;
        
        if (remainingEnemies <= 0 && isRoundActive)
        {
            // 적이 모두 쓰러지면 라운드 완료
            CompleteRound();
        }
    }
    
    void CompleteRound()
    {
        isRoundActive = false;
        currentRound++;
        onRoundComplete?.Invoke();
        
        if (currentRound < rounds.Count)
        {
            Debug.Log($"Round {rounds[currentRound-1].roundNumber} Complete!");
        }
        
        // 다음 라운드 시작 전 잠시 대기
        StartCoroutine(WaitBeforeNextRound());
    }
    
    IEnumerator WaitBeforeNextRound()
    {
        // 다음 라운드 메시지 표시
        if (roundText != null && currentRound < rounds.Count)
        {
            roundText.text = $"Preparing Round {rounds[currentRound].roundNumber}";
        }
        
        // 적 수 텍스트 비우기
        if (enemyCountText != null)
        {
            enemyCountText.text = "";
        }
        
        if (timerText != null)
        {
            timerText.text = $"Next round in...";
        }
        
        // 카운트다운 표시
        for (int i = Mathf.FloorToInt(timeBetweenRounds); i > 0; i--)
        {
            if (timerText != null)
            {
                timerText.text = i.ToString();
            }
            yield return new WaitForSeconds(1f);
        }
        
        StartNextRound();
    }
    
    // 게임 중단/일시정지시 사용할 수 있는 공용 메서드
    public void PauseRound()
    {
        isRoundActive = false;
    }
    
    public void ResumeRound()
    {
        isRoundActive = true;
    }
    
    // 디버그용 - 현재 라운드 강제 종료
    public void ForceCompleteRound()
    {
        if (isRoundActive)
        {
            CompleteRound();
        }
    }
    
    // 게임 재시작을 위한 리셋 함수
    public void ResetGame()
    {
        StopAllCoroutines();
        
        // 모든 적 제거
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        
        // 상태 초기화
        currentRound = 0;
        remainingEnemies = 0;
        isRoundActive = false;
        
        // 첫 라운드부터 다시 시작
        StartNextRound();
    }
}