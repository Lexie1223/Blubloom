using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 用法：打开场景后，点顶部菜单 Tools -> Blubloom -> Setup Current Scene UI
// 会把 Home/Level 需要的 UI 物体真正建到 Hierarchy 里，可手动拖动改大小
public static class UISetupMenu
{
    [MenuItem("Tools/Blubloom/Setup Current Scene UI")]
    public static void SetupCurrentScene()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("UI Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        string scene = SceneManager.GetActiveScene().name;

        // Level 场景：暂停按钮 + 顶部关卡图标 + 暂停面板
        if (scene.StartsWith("Level"))
        {
            CreateButton(canvas.transform, "PauseButton", "pause_button",
                new Vector2(1, 1), new Vector2(-75, -75), new Vector2(120, 120));

            string levelIcon = scene == "Level1" ? "level_1" : "level_2";
            CreateImage(canvas.transform, "LevelBadge", levelIcon,
                new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(140, 140));

            CreatePausePanel(canvas.transform);
        }

        // Home：设置按钮 + 音量面板
        if (scene == "Home")
        {
            CreateButton(canvas.transform, "SettingsButton", "settings",
                new Vector2(1, 0), new Vector2(-50, 50), new Vector2(110, 110));

            CreateHomeVolumePanel(canvas.transform);
        }

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[Blubloom] UI 已生成，记得 Ctrl+S 保存场景。");
    }

    private static void CreatePausePanel(Transform parent)
    {
        // 如果 PausePanel 已经存在，也继续往里面补 Resume/Quit/音量行
        GameObject panel = FindExisting("PausePanel");
        if (panel == null)
        {
            panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)panel.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);
            panel.SetActive(false);
        }

        CreateButton(panel.transform, "ResumeButton", "resume",
            new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(400, 130));
        CreateButton(panel.transform, "QuitButton", "exit",
            new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(400, 130));

        CreateSliderVolumeRow(panel.transform, "BGMRow", "music", new Vector2(0, -130), "背景音乐");
        CreateSliderVolumeRow(panel.transform, "SFXRow", "sound", new Vector2(0, -240), "音效");
    }

    private static void CreateHomeVolumePanel(Transform parent)
    {
        if (FindExisting("HomeVolumePanel") != null) return;

        GameObject panel = new GameObject("HomeVolumePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)panel.transform;
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-180, 180);
        rt.sizeDelta = new Vector2(340, 200);
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        panel.SetActive(false);

        CreateVolumeRow(panel.transform, "BGMRow", "music", new Vector2(0, 50));
        CreateVolumeRow(panel.transform, "SFXRow", "sound", new Vector2(0, -50));
    }

    private static void CreateVolumeRow(Transform parent, string rowName, string iconName, Vector2 pos)
    {
        if (FindExisting(rowName) != null) return;

        GameObject row = new GameObject(rowName, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        ((RectTransform)row.transform).anchoredPosition = pos;

        CreateImage(row.transform, "Icon", iconName,
            new Vector2(0.5f, 0.5f), new Vector2(-110, 0), new Vector2(70, 70));

        CreateButton(row.transform, "Minus", "",
            new Vector2(0.5f, 0.5f), new Vector2(-40, 0), new Vector2(55, 45));
        CreateText(row.transform, "Value", "8", new Vector2(0.5f, 0.5f), new Vector2(20, 0), new Vector2(60, 45));
        CreateButton(row.transform, "Plus", "",
            new Vector2(0.5f, 0.5f), new Vector2(80, 0), new Vector2(55, 45));
    }

    // 新版音量行：图标 + 文字标注 + Slider 滑动条（暂停面板用）
    private static void CreateSliderVolumeRow(Transform parent, string rowName, string iconName,
        Vector2 pos, string label)
    {
        if (FindExisting(rowName) != null) return;

        GameObject row = new GameObject(rowName, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        RectTransform rowRt = (RectTransform)row.transform;
        rowRt.anchoredPosition = pos;
        rowRt.sizeDelta = new Vector2(560, 60);

        CreateImage(row.transform, "Icon", iconName,
            new Vector2(0.5f, 0.5f), new Vector2(-230, 0), new Vector2(64, 64));

        // 文字标注
        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(row.transform, false);
        RectTransform lrt = (RectTransform)labelGo.transform;
        lrt.anchoredPosition = new Vector2(-120, 0);
        lrt.sizeDelta = new Vector2(130, 44);
        Text lt = labelGo.GetComponent<Text>();
        lt.text = label;
        lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lt.fontSize = 26;
        lt.alignment = TextAnchor.MiddleLeft;
        lt.color = Color.white;

        // Slider 本体
        GameObject sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(row.transform, false);
        RectTransform srt = (RectTransform)sliderGo.transform;
        srt.anchoredPosition = new Vector2(40, 0);
        srt.sizeDelta = new Vector2(320, 44);

        // 轨道背景
        GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(sliderGo.transform, false);
        RectTransform bgRt = (RectTransform)bgGo.transform;
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgGo.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.45f, 1f);

        // 填充
        GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(sliderGo.transform, false);
        RectTransform fillRt = (RectTransform)fillGo.transform;
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 1);
        fillRt.offsetMin = new Vector2(6, 0);
        fillRt.offsetMax = new Vector2(-6, 0);
        fillGo.GetComponent<Image>().color = new Color(0.2f, 0.7f, 1f, 1f);

        // 手柄
        GameObject handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGo.transform.SetParent(sliderGo.transform, false);
        RectTransform hrt = (RectTransform)handleGo.transform;
        hrt.sizeDelta = new Vector2(40, 40);
        handleGo.GetComponent<Image>().color = Color.white;

        Slider slider = sliderGo.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 10;
        slider.wholeNumbers = true;
        slider.value = 8;
        slider.targetGraphic = handleGo.GetComponent<Image>();
        slider.fillRect = fillRt;
        slider.handleRect = hrt;
        slider.direction = Slider.Direction.LeftToRight;
    }

    private static GameObject FindExisting(string name)
    {
        return GameObject.Find(name);
    }

    private static Button CreateButton(Transform parent, string name, string spriteName,
        Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject existing = FindExisting(name);
        if (existing != null) return existing.GetComponent<Button>();

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        Sprite s = LoadSprite(spriteName);
        if (s != null)
        {
            img.sprite = s;
            img.color = Color.white;
        }
        else
        {
            // 没有贴图的按钮（- / +）用蓝色底
            img.color = new Color(0.2f, 0.5f, 0.8f, 1f);
        }

        return go.GetComponent<Button>();
    }

    private static void CreateImage(Transform parent, string name, string spriteName,
        Vector2 anchor, Vector2 pos, Vector2 size)
    {
        if (FindExisting(name) != null) return;

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.color = Color.white;
        Sprite s = LoadSprite(spriteName);
        if (s != null) img.sprite = s;
    }

    private static void CreateText(Transform parent, string name, string content,
        Vector2 anchor, Vector2 pos, Vector2 size)
    {
        if (FindExisting(name) != null) return;

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text t = go.GetComponent<Text>();
        t.text = content;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 28;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
    }

    private static Sprite LoadSprite(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        Sprite[] sprites = Resources.LoadAll<Sprite>("UI/" + name);
        return sprites != null && sprites.Length > 0 ? sprites[0] : null;
    }
}
