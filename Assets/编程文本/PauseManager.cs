using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("把暂停面板拖进来")]
    public GameObject pausePanel;

    [Header("退回到哪个主场景")]
    public string homeSceneName = "Home";

    private bool _paused;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        _paused = !_paused;

        if (pausePanel != null)
            pausePanel.SetActive(_paused);

        Time.timeScale = _paused ? 0f : 1f;
    }

    public void Resume()
    {
        _paused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void QuitToHome()
    {
        _paused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }
}
