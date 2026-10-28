using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// 全局暂停菜单管理器：跨场景存在，负责右上角暂停按钮、暂停面板、音量滑动条
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("退回到哪个主场景")]
    public string homeSceneName = "Home";

    [Header("右上角暂停按钮大小，想改大小直接改这里")]
    public float pauseButtonSize = 200f;

    [Header("暂停按钮离右上角的距离")]
    public float pauseButtonOffset = 75f;

    [Header("Resume / Exit 按钮大小，被拉伸就改这里")]
    public Vector2 resumeButtonSize = new Vector2(400f, 130f);
    public Vector2 quitButtonSize = new Vector2(400f, 130f);

    [Header("音量滑动条宽度")]
    public float volumeSliderWidth = 320f;

    private bool _paused;
    private GameObject _pausePanel;
    private Button _pauseButton;
    private Slider _bgmSlider;
    private Slider _sfxSlider;

    // 单例 + 跨场景保留，防止切场景后暂停菜单消失
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

    // 监听场景切换：每次进新场景都重新绑定暂停 UI
    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start()
    {
        EnsurePersistentPauseButton();
        RebindCurrentScene();
    }

    // 按 Esc 或 P 也能暂停/继续
    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // 场景刚加载完：先恢复时间，再重新找当前场景的暂停面板
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Resume();
        RebindCurrentScene();
    }

    // 每个场景重新找 PausePanel；找不到就自动建一个
    private void RebindCurrentScene()
    {
        AudioManager.Ensure();

        _pausePanel = FindInactive("PausePanel");
        if (_pausePanel == null)
            _pausePanel = CreatePanel();

        Image panelImg = _pausePanel.GetComponent<Image>();
        if (panelImg != null) panelImg.color = new Color(0f, 0f, 0f, 0.65f);
        _pausePanel.SetActive(false);

        // 接线 Resume 按钮
        Button resume = FindOrCreateChildButton("ResumeButton", "resume", new Vector2(0, 160));
        resume.onClick.RemoveListener(Resume);
        resume.onClick.AddListener(Resume);
        ClearButtonText(resume);

        // 接线 Quit to Home 按钮
        Button quit = FindOrCreateChildButton("QuitButton", "exit", new Vector2(0, 20));
        quit.onClick.RemoveListener(QuitToHome);
        quit.onClick.AddListener(QuitToHome);
        ClearButtonText(quit);

        // 创建 BGM / SFX 音量滑动条行
        EnsureVolumeRows();

        // 确保右上角暂停按钮存在，再按场景决定显隐
        EnsurePersistentPauseButton();
        bool inLevel = SceneManager.GetActiveScene().name.StartsWith("Level");
        if (_pauseButton != null)
            _pauseButton.gameObject.SetActive(inLevel);

        // 顶部显示第几关图标
        EnsureLevelBadge();
    }

    // 在每个 Level 顶部显示 level_1 / level_2 图标
    private void EnsureLevelBadge()
    {
        Canvas canvas = FindOrCreateOverlayCanvas();
        GameObject badge = FindInactive("LevelBadge");
        if (badge == null)
            badge = new GameObject("LevelBadge", typeof(RectTransform), typeof(Image));

        badge.transform.SetParent(canvas.transform, false);
        RectTransform rt = (RectTransform)badge.transform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -50f);
        rt.sizeDelta = new Vector2(120f, 120f);

        string scene = SceneManager.GetActiveScene().name;
        string iconName = scene == "Level1" ? "level_1" : scene == "Level2" ? "level_2" : "";

        Image img = badge.GetComponent<Image>();
        if (img == null) img = badge.AddComponent<Image>();
        img.color = Color.white;

        if (!string.IsNullOrEmpty(iconName))
        {
            ApplySprite(img, iconName);
            badge.SetActive(true);
        }
        else
        {
            badge.SetActive(false);
        }
    }

    // 创建/找回 BGM 和 SFX 两行音量滑动条
    private void EnsureVolumeRows()
    {
        _bgmSlider = EnsureVolumeRow("BGMRow", "music", -130, true, "背景音乐");
        _sfxSlider = EnsureVolumeRow("SFXRow", "sound", -240, false, "音效");
        RefreshSliders();
    }

    // 确保某一行是 [图标 + 文字标注 + 滑动条] 结构；旧版 [- 数字 +] 结构会被自动替换
    private Slider EnsureVolumeRow(string rowName, string iconName, float y, bool isBgm, string label)
    {
        Transform row = _pausePanel.transform.Find(rowName);
        if (row == null)
        {
            row = CreateVolumeRow(rowName, iconName, y, isBgm, label);
        }
        else
        {
            row.SetAsLastSibling();
            // 保留的 Icon 重新摆到最左边，避免和文字/滑条重叠
            Transform icon = row.Find("Icon");
            if (icon != null)
            {
                RectTransform irt = (RectTransform)icon;
                irt.anchoredPosition = new Vector2(-230, 0);
                irt.sizeDelta = new Vector2(64, 64);
            }
            Slider slider = row.Find("Slider")?.GetComponent<Slider>();
            if (slider == null)
            {
                // 旧结构（Minus / Plus / Value）直接清掉重建
                for (int i = row.childCount - 1; i >= 0; i--)
                {
                    Transform child = row.GetChild(i);
                    if (child.name != "Icon") Destroy(child.gameObject);
                }
                slider = BuildVolumeSlider(row, isBgm, label);
            }
            else
            {
                WireSlider(slider, isBgm);
            }
        }
        return row.Find("Slider")?.GetComponent<Slider>();
    }

    // 新建一行：Icon + 文字标注 + Slider
    private Transform CreateVolumeRow(string rowName, string iconName, float y, bool isBgm, string label)
    {
        GameObject row = new GameObject(rowName, typeof(RectTransform));
        row.transform.SetParent(_pausePanel.transform, false);
        RectTransform rowRt = (RectTransform)row.transform;
        rowRt.anchoredPosition = new Vector2(0, y);
        rowRt.sizeDelta = new Vector2(560, 60);

        // 左边图标
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(row.transform, false);
        RectTransform irt = (RectTransform)iconGo.transform;
        irt.anchoredPosition = new Vector2(-230, 0);
        irt.sizeDelta = new Vector2(64f, 64f);
        Image iconImg = iconGo.GetComponent<Image>();
        iconImg.color = Color.white;
        ApplySprite(iconImg, iconName);

        BuildVolumeSlider(row.transform, isBgm, label);
        return row.transform;
    }

    // 在一行内生成文字标注 + 滑动条本体（背景/填充/手柄）
    private Slider BuildVolumeSlider(Transform row, bool isBgm, string label)
    {
        // 文字标注：背景音乐 / 音效
        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(row, false);
        RectTransform lrt = (RectTransform)labelGo.transform;
        lrt.anchoredPosition = new Vector2(-120, 0);
        lrt.sizeDelta = new Vector2(130f, 44f);
        Text labelText = labelGo.GetComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 26;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;

        // 滑动条
        GameObject sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(row, false);
        RectTransform srt = (RectTransform)sliderGo.transform;
        srt.anchoredPosition = new Vector2(40, 0);
        srt.sizeDelta = new Vector2(volumeSliderWidth, 44f);

        // 轨道背景
        GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(sliderGo.transform, false);
        RectTransform bgRt = (RectTransform)bgGo.transform;
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgGo.GetComponent<Image>();
        bgImg.color = new Color(0.2f, 0.3f, 0.45f, 1f);

        // 填充
        GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(sliderGo.transform, false);
        RectTransform fillRt = (RectTransform)fillGo.transform;
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 1);
        fillRt.offsetMin = new Vector2(6, 0);
        fillRt.offsetMax = new Vector2(-6, 0);
        Image fillImg = fillGo.GetComponent<Image>();
        fillImg.color = new Color(0.2f, 0.7f, 1f, 1f);

        // 手柄
        GameObject handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGo.transform.SetParent(sliderGo.transform, false);
        RectTransform hrt = (RectTransform)handleGo.transform;
        hrt.sizeDelta = new Vector2(40f, 40f);
        Image handleImg = handleGo.GetComponent<Image>();
        handleImg.color = Color.white;

        Slider slider = sliderGo.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 10;
        slider.wholeNumbers = true;
        slider.targetGraphic = handleImg;
        slider.fillRect = fillRt;
        slider.handleRect = hrt;
        slider.direction = Slider.Direction.LeftToRight;
        WireSlider(slider, isBgm);
        return slider;
    }

    // 给滑动条接上音量逻辑
    private void WireSlider(Slider slider, bool isBgm)
    {
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(v =>
        {
            if (AudioManager.Instance == null) return;
            if (isBgm) AudioManager.Instance.SetBgmVolume(Mathf.RoundToInt(v));
            else AudioManager.Instance.SetSfxVolume(Mathf.RoundToInt(v));
        });
    }

    // 打开暂停面板时把滑条位置刷成当前音量
    private void RefreshSliders()
    {
        if (AudioManager.Instance == null) return;
        if (_bgmSlider != null) _bgmSlider.value = AudioManager.Instance.GetBgmVolume01() * 10f;
        if (_sfxSlider != null) _sfxSlider.value = AudioManager.Instance.GetSfxVolume01() * 10f;
    }

    // 找或创建右上角暂停按钮
    private void EnsurePersistentPauseButton()
    {
        _pauseButton = FindButton("PauseButton");
        if (_pauseButton == null)
        {
            Canvas canvas = FindOrCreateOverlayCanvas();
            _pauseButton = CreateCornerButton(canvas, "PauseButton", "pause_button");
        }
        else
        {
            ApplySprite(_pauseButton.GetComponent<Image>(), "pause_button");
        }

        _pauseButton.onClick.RemoveListener(TogglePause);
        _pauseButton.onClick.AddListener(TogglePause);
    }

    // 连隐藏物体一起找，不然 PausePanel 关着时代码会误以为没有、又建一个
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

    // 找现有按钮，找不到就自动补一个
    private Button FindOrCreateChildButton(string buttonName, string spriteName, Vector2 anchoredPos)
    {
        Button btn = FindButton(buttonName);
        if (btn == null)
        {
            btn = CreateChildButton(buttonName, spriteName, _pausePanel.transform, anchoredPos);
        }
        else
        {
            ApplySprite(btn.GetComponent<Image>(), spriteName);
            RectTransform rt = (RectTransform)btn.transform;
            rt.sizeDelta = buttonName == "ResumeButton" ? resumeButtonSize : quitButtonSize;
            rt.localScale = Vector3.one;
        }
        return btn;
    }

    // 创建全屏半透明黑底面板
    private GameObject CreatePanel()
    {
        Canvas canvas = FindOrCreateOverlayCanvas();
        GameObject panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = (RectTransform)panel.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.65f);
        return panel;
    }

    // 创建面板里的大按钮（Resume / Exit）
    private Button CreateChildButton(string buttonName, string spriteName, Transform parent, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta =
            buttonName == "ResumeButton" ? resumeButtonSize : quitButtonSize;

        Image img = go.GetComponent<Image>();
        img.color = Color.white;
        ApplySprite(img, spriteName);

        return go.GetComponent<Button>();
    }

    // 创建右上角暂停按钮，大小由上面 public 变量控制
    private Button CreateCornerButton(Canvas canvas, string buttonName, string spriteName)
    {
        GameObject go = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-pauseButtonOffset, -pauseButtonOffset);
        rt.sizeDelta = new Vector2(pauseButtonSize, pauseButtonSize);

        Image img = go.GetComponent<Image>();
        img.color = Color.white;
        ApplySprite(img, spriteName);

        return go.GetComponent<Button>();
    }

    // 给按钮贴图
    private void ApplySprite(Image img, string spriteName)
    {
        if (img == null) return;
        Sprite sprite = LoadSprite(spriteName);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
        }
    }

    // 清空按钮上自带的文字
    private void ClearButtonText(Button btn)
    {
        foreach (Text t in btn.GetComponentsInChildren<Text>(true)) t.text = "";
        foreach (TMP_Text t in btn.GetComponentsInChildren<TMP_Text>(true)) t.text = "";
    }

    // 从 Resources 加载多图 Sprite
    private Sprite LoadSprite(string name)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("UI/" + name);
        return sprites != null && sprites.Length > 0 ? sprites[0] : null;
    }

    // 找场景里的 Canvas，没有就新建一个
    private Canvas FindOrCreateOverlayCanvas()
    {
        Canvas existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null) return existing;

        GameObject canvasGo = new GameObject("PersistentUICanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas c = canvasGo.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        return c;
    }

    // 暂停/继续切换
    public void TogglePause()
    {
        AudioManager.Instance?.PlayClick();

        _paused = !_paused;
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(_paused);
            if (_paused) RefreshSliders();
        }

        Time.timeScale = _paused ? 0f : 1f;
    }

    // 继续游戏：关面板、恢复时间
    public void Resume()
    {
        _paused = false;
        if (_pausePanel != null)
            _pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // 退回主菜单
    public void QuitToHome()
    {
        AudioManager.Instance?.PlayClick();
        _paused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }
}
