using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 全局暂停菜单管理器：跨场景存在。
// 纯绑定模式：不创建、不销毁任何 UI，所有按钮/滑条都在场景 Hierarchy 里摆好。
// 这个脚本只负责找现有 UI、绑事件、读写数值。
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("退回到哪个主场景")]
    public string homeSceneName = "Home";

    private bool _paused;
    private GameObject _pausePanel;
    private Button _resumeButton;
    private Button _quitButton;
    private Button _pauseButton;
    private Slider _bgmSlider;
    private Slider _sfxSlider;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start() => BindCurrentScene();

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Resume();
        BindCurrentScene();
    }

    // 每次切场景后，重新找一遍当前场景里的暂停 UI 并绑事件
    private void BindCurrentScene()
    {
        AudioManager.Ensure();

        _pausePanel = FindInactive("PausePanel");
        if (_pausePanel != null) _pausePanel.SetActive(false);

        _resumeButton = FindButton("ResumeButton");
        if (_resumeButton != null)
        {
            _resumeButton.onClick.RemoveListener(Resume);
            _resumeButton.onClick.AddListener(Resume);
        }

        _quitButton = FindButton("QuitButton");
        if (_quitButton != null)
        {
            _quitButton.onClick.RemoveListener(QuitToHome);
            _quitButton.onClick.AddListener(QuitToHome);
        }

        // 音量滑条：在 BGMRow / SFXRow 下找名为 Slider 的子物体
        _bgmSlider = FindSliderUnder("BGMRow");
        _sfxSlider = FindSliderUnder("SFXRow");

        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.RemoveAllListeners();
            _bgmSlider.onValueChanged.AddListener(v =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetBgmVolume(Mathf.RoundToInt(v));
            });
            if (AudioManager.Instance != null)
                _bgmSlider.value = AudioManager.Instance.GetBgmVolume01() * 10f;
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveAllListeners();
            _sfxSlider.onValueChanged.AddListener(v =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetSfxVolume(Mathf.RoundToInt(v));
            });
            if (AudioManager.Instance != null)
                _sfxSlider.value = AudioManager.Instance.GetSfxVolume01() * 10f;
        }

        // 右上角暂停按钮（只在 Level 场景显示）
        _pauseButton = FindButton("PauseButton");
        bool inLevel = SceneManager.GetActiveScene().name.StartsWith("Level");
        if (_pauseButton != null)
        {
            _pauseButton.gameObject.SetActive(inLevel);
            _pauseButton.onClick.RemoveListener(TogglePause);
            _pauseButton.onClick.AddListener(TogglePause);
        }
    }

    private Slider FindSliderUnder(string rowName)
    {
        GameObject row = FindInactive(rowName);
        if (row == null) return null;
        Transform sliderT = row.transform.Find("Slider");
        return sliderT != null ? sliderT.GetComponent<Slider>() : null;
    }

    private GameObject FindInactive(string objectName)
    {
        Transform[] all = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all)
        {
            if (t.name == objectName) return t.gameObject;
        }
        return null;
    }

    private Button FindButton(string objectName)
    {
        GameObject go = FindInactive(objectName);
        return go != null ? go.GetComponent<Button>() : null;
    }

    public void TogglePause()
    {
        AudioManager.Instance?.PlayClick();
        _paused = !_paused;
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(_paused);
            if (_paused) RefreshSliderValues();
        }
        Time.timeScale = _paused ? 0f : 1f;
    }

    private void RefreshSliderValues()
    {
        if (AudioManager.Instance == null) return;
        if (_bgmSlider != null) _bgmSlider.value = AudioManager.Instance.GetBgmVolume01() * 10f;
        if (_sfxSlider != null) _sfxSlider.value = AudioManager.Instance.GetSfxVolume01() * 10f;
    }

    public void Resume()
    {
        _paused = false;
        if (_pausePanel != null) _pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitToHome()
    {
        AudioManager.Instance?.PlayClick();
        _paused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }
}
