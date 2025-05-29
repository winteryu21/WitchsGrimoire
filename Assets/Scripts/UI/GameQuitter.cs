using UnityEngine;

public class GameQuitter : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("게임 종료 시도됨");

#if UNITY_EDITOR
        // 유니티 에디터에서 실행 중이라면 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 실행 파일에서 게임 종료
        Application.Quit();
#endif
    }
}
