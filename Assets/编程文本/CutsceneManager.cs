using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("显示多少秒后自动进下一个场景")]
    public float showTime = 12f;

    [Header("看完进入哪个场景")]
    public string nextSceneName = "Level2";

    private float _timer;
    private bool _loaded;
    private Text _continueText;
    private float _baseY;

    void Start()
    {
        AudioManager.Instance?.PlayLevelUp();
        CreateContinueUI();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        // 底部文字轻轻跳动
        if (_continueText != null)
        {
            float bounce = Mathf.Sin(Time.time * 4f) * 8f;
            ((RectTransform)_continueText.transform).anchoredPosition =
                new Vector2(0, _baseY + bounce);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            LoadNext();
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            LoadNext();

        if (_timer >= showTime)
            LoadNext();
    }

    private void CreateContinueUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 底部跳动提示
        GameObject textGo = new GameObject("ContinueHint", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(canvas.transform, false);
        RectTransform rt = (RectTransform)textGo.transform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 40f);
        rt.sizeDelta = new Vector2(800f, 50f);

        _continueText = textGo.GetComponent<Text>();
        _continueText.text = "Click anywhere to continue to the next stage";
        _continueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _continueText.fontSize = 30;
        _continueText.alignment = TextAnchor.MiddleCenter;
        _continueText.color = Color.white;
        _baseY = 40f;

        // 右下角 Next 按钮
        GameObject btnGo = new GameObject("NextButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(canvas.transform, false);
        RectTransform brt = (RectTransform)btnGo.transform;
        brt.anchorMin = new Vector2(1f, 0f);
        brt.anchorMax = new Vector2(1f, 0f);
        brt.pivot = new Vector2(1f, 0f);
        brt.anchoredPosition = new Vector2(-50f, 50f);
        brt.sizeDelta = new Vector2(220f, 110f);

        Image img = btnGo.GetComponent<Image>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("UI/next");
        if (sprites != null && sprites.Length > 0) img.sprite = sprites[0];
        img.color = Color.white;

        Button btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(LoadNext);
    }

    private void LoadNext()
    {
        if (_loaded) return;
        _loaded = true;
        AudioManager.Instance?.PlayClick();
        SceneManager.LoadScene(nextSceneName);
    }
}
