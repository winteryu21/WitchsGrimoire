using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // 씬 이름 또는 인덱스를 통해 전환
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}