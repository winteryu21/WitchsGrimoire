using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    // 게임 상태
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    // 게임 설정
    [Header("Game Settings")]
    [SerializeField] private float gameTime = 1200f; // 기본 게임 시간 (20분)
    [SerializeField] private float timeScale = 1f;   // 게임 속도 배율

    // 내부 상태
    private GameState currentState;
    private float currentGameTime;
    private int enemiesKilled;
    private int highestLevel;
    private bool isInitialized = false;

    // 이벤트
    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnGameTimeChanged;
    public event Action<int> OnEnemyKilled;
    public event Action OnGameOver;
    public event Action OnVictory;

    // 프로퍼티
    public GameState CurrentState => currentState;
    public float CurrentGameTime => currentGameTime;
    public int EnemiesKilled => enemiesKilled;
    public int HighestLevel => highestLevel;
    public bool IsPaused => currentState == GameState.Paused;
    public float TimeScale => timeScale;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            // 게임 시간 업데이트
            UpdateGameTime();

            // 일시정지 입력 처리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }
        else if (currentState == GameState.Paused)
        {
            // 일시정지 해제 입력 처리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeGame();
            }
        }
    }

    private void InitializeGame()
    {
        if (isInitialized) return;

        currentState = GameState.MainMenu;
        currentGameTime = 0f;
        enemiesKilled = 0;
        highestLevel = 1;
        Time.timeScale = timeScale;

        isInitialized = true;
    }

    // 게임 시작
    public void StartGame()
    {
        if (currentState == GameState.MainMenu || currentState == GameState.GameOver || currentState == GameState.Victory)
        {
            // 게임 상태 초기화
            currentGameTime = 0f;
            enemiesKilled = 0;
            highestLevel = 1;
            
            // 게임 상태 변경
            SetGameState(GameState.Playing);
            
            // 게임 시간 스케일 설정
            Time.timeScale = timeScale;
        }
    }

    // 게임 일시정지
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
        }
    }

    // 게임 재개
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            Time.timeScale = timeScale;
        }
    }

    // 게임 오버
    public void GameOver()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.GameOver);
            Time.timeScale = 0f;
            OnGameOver?.Invoke();
        }
    }

    // 게임 승리
    public void Victory()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Victory);
            Time.timeScale = 0f;
            OnVictory?.Invoke();
        }
    }

    // 메인 메뉴로 돌아가기
    public void ReturnToMainMenu()
    {
        StartCoroutine(LoadMainMenuScene());
    }

    // 적 처치 카운트 증가
    public void EnemyKilled()
    {
        enemiesKilled++;
        OnEnemyKilled?.Invoke(enemiesKilled);
    }

    // 최고 레벨 업데이트
    public void UpdateHighestLevel(int level)
    {
        if (level > highestLevel)
        {
            highestLevel = level;
        }
    }

    // 게임 시간 업데이트
    private void UpdateGameTime()
    {
        currentGameTime += Time.deltaTime;
        OnGameTimeChanged?.Invoke(currentGameTime);

        // 게임 시간이 다 되면 승리
        if (currentGameTime >= gameTime)
        {
            Victory();
        }
    }

    // 게임 상태 변경
    private void SetGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
    }

    // 게임 종료
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // 게임 저장
    public void SaveGame()
    {
        // 게임 저장 로직 구현 (선택적)
        Debug.Log("Game saved");
    }

    // 메인 메뉴 씬 로드
    private IEnumerator LoadMainMenuScene()
    {
        SetGameState(GameState.MainMenu);
        Time.timeScale = 1f;

        // 씬 로드
        SceneManager.LoadScene("MainMenu");
        yield return null;
    }

    // 게임 씬 로드
    public void LoadGameScene()
    {
        StartCoroutine(LoadGameSceneCoroutine());
    }

    private IEnumerator LoadGameSceneCoroutine()
    {
        // 씬 로드
        SceneManager.LoadScene("GameScene");
        yield return null;
        
        // 게임 시작
        StartGame();
    }

    // 게임 설정 변경
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Clamp(scale, 0.5f, 2f);
        
        if (currentState == GameState.Playing)
        {
            Time.timeScale = timeScale;
        }
    }

    // 디버그 모드 토글 (개발 중 유용)
    private bool debugMode = false;
    
    public void ToggleDebugMode()
    {
        debugMode = !debugMode;
        Debug.Log($"Debug mode: {debugMode}");
    }
    
    public bool IsDebugMode()
    {
        return debugMode;
    }
}