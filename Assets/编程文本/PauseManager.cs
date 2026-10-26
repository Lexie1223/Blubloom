using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// 全局暂停菜单管理器：跨场景存在，负责右上角暂停按钮、暂停面板、音量加减
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
    public Vector2 resumeButtonSize = new Vector2(280f, 90f);
    public Vector2 quitButtonSize = new Vector2(280f, 90f);

    private bool _paused;
    private GameObject _pausePanel;
    private Button _pauseButton;

    private Text _bgmValueText;
    private Text _sfxValueText;

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
        Button resume = FindOrCreateChildButton("ResumeButton", "resume", new Vector2(0, 110));
        resume.onClick.RemoveListener(Resume);
        resume.onClick.AddListener(Resume);
        ClearButtonText(resume);

        // 接线 Quit to Home 按钮
        Button quit = FindOrCreateChildButton("QuitButton", "exit", new Vector2(0, 0));
        quit.onClick.RemoveListener(QuitToHome);
        quit.onClick.AddListener(QuitToHome);
        ClearButtonText(quit);

        // 创建 BGM / SFX 加减音量行
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

    // 创建/找回 BGM 和 SFX 两行音量控制
    private void EnsureVolumeRows()
    {
        Transform row = _pausePanel.transform.Find("BGMRow");
        if (row == null)
        {
            CreateVolumeRow("BGMRow", "music", -90,
                () => AudioManager.Instance?.BgmDown(),
                () => AudioManager.Instance?.BgmUp(),
                out _bgmValueText);
        }
        else
        {
            _bgmValueText = row.Find("Value")?.GetComponent<Text>();
            WireExistingVolumeRow(row, true);
        }

        row = _pausePanel.transform.Find("SFXRow");
        if (row == null)
        {
            CreateVolumeRow("SFXRow", "sound", -160,
                () => AudioManager.Instance?.SfxDown(),
                () => AudioManager.Instance?.SfxUp(),
                out _sfxValueText);
        }
        else
        {
            _sfxValueText = row.Find("Value")?.GetComponent<Text>();
            WireExistingVolumeRow(row, false);
        }

        RefreshVolumeLabels();
    }

    // 生成一行：music/sound 图标 + [-] + 数字 + [+]
    private void CreateVolumeRow(string rowName, string iconName, float y,
        System.Action minus, System.Action plus, out Text valueText)
    {
        GameObject row = new GameObject(rowName, typeof(RectTransform));
        row.transform.SetParent(_pausePanel.transform, false);
        ((RectTransform)row.transform).anchoredPosition = new Vector2(0, y);

        // 左边图标
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(row.transform, false);
        RectTransform irt = (RectTransform)iconGo.transform;
        irt.anchoredPosition = new Vector2(-170, 0);
        irt.sizeDelta = new Vector2(50f, 50f);
        Image iconImg = iconGo.GetComponent<Image>();
        iconImg.color = Color.white;
        ApplySprite(iconImg, iconName);

        Button minusBtn = CreateSmallButton(row.transform, "Minus", "-", new Vector2(-70, 0));
        minusBtn.onClick.AddListener(() => { minus(); RefreshVolumeLabels(); });

        valueText = CreateLabel(row.transform, "8", new Vector2(0, 0), 60, 40);
        valueText.gameObject.name = "Value";

        Button plusBtn = CreateSmallButton(row.transform, "Plus", "+", new Vector2(70, 0));
        plusBtn.onClick.AddListener(() => { plus(); RefreshVolumeLabels(); });
    }

    // 生成纯文字标签
    private Text CreateLabel(Transform parent, string content, Vector2 pos, float w, float h)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(w, h);

        Text t = go.GetComponent<Text>();
        t.text = content;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 28;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        return t;
    }

    // 生成小方块按钮（- / +）
    private Button CreateSmallButton(Transform parent, string name, string text, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(55f, 45f);

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f, 1f);

        Text t = CreateLabel(go.transform, text, Vector2.zero, 55, 45);
        t.fontSize = 28;

        return go.GetComponent<Button>();
    }

    // 给菜单生成的 Minus / Plus 按钮补文字和事件
    private void WireExistingVolumeRow(Transform row, bool isBgm)
    {
        Button minus = row.Find("Minus")?.GetComponent<Button>();
        Button plus = row.Find("Plus")?.GetComponent<Button>();

        if (minus != null)
        {
            EnsureButtonText(minus, "-");
            minus.onClick.RemoveAllListeners();
            minus.onClick.AddListener(() =>
            {
                if (isBgm) AudioManager.Instance?.BgmDown();
                else AudioManager.Instance?.SfxDown();
                RefreshVolumeLabels();
            });
        }

        if (plus != null)
        {
            EnsureButtonText(plus, "+");
            plus.onClick.RemoveAllListeners();
            plus.onClick.AddListener(() =>
            {
                if (isBgm) AudioManager.Instance?.BgmUp();
                else AudioManager.Instance?.SfxUp();
                RefreshVolumeLabels();
            });
        }
    }

    private void EnsureButtonText(Button btn, string content)
    {
        Text t = btn.GetComponentInChildren<Text>();
        if (t == null)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(btn.transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            t = go.GetComponent<Text>();
        }
        t.text = content;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 28;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;

        Image img = btn.GetComponent<Image>();
        if (img != null && img.sprite == null)
            img.color = new Color(0.2f, 0.5f, 0.8f, 1f);
    }

    // 点音量按钮后刷新数字显示
    private void RefreshVolumeLabels()
    {
        if (AudioManager.Instance == null) return;
        if (_bgmValueText != null) _bgmValueText.text = AudioManager.Instance.GetBgmText();
        if (_sfxValueText != null) _sfxValueText.text = AudioManager.Instance.GetSfxText();
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
            ((RectTransform)btn.transform).sizeDelta =
                buttonName == "ResumeButton" ? resumeButtonSize : quitButtonSize;
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
            _pausePanel.SetActive(_paused);

        if (_paused) RefreshVolumeLabels();

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
