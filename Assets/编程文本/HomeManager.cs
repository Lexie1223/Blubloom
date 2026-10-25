using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
    [Header("点Start后进入的场景名，必须和文件名一致")]
    public string startSceneName = "Level1";

    // 绑给 Start 按钮
    public void OnStartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }
}
