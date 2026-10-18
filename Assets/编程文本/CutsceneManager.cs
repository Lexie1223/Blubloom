using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CutsceneManager : MonoBehaviour
{
    [Header("显示多少秒后自动进下一个场景")]
    public float showTime = 12f;

    [Header("看完进入哪个场景")]
    public string nextSceneName = "Level2";

    private float _timer;
    private bool _loaded;

    void Update()
    {
        _timer += Time.deltaTime;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            LoadNext();
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            LoadNext();

        if (_timer >= showTime)
            LoadNext();
    }

    void LoadNext()
    {
        if (_loaded) return;
        _loaded = true;
        SceneManager.LoadScene(nextSceneName);
    }
}
