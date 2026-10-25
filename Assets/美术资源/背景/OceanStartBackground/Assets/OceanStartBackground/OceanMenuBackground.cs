using UnityEngine;
using UnityEngine.UI;

// UI-only background. Uses unscaled time so menus animate while gameplay is paused.
[RequireComponent(typeof(RectTransform))]
public class OceanMenuBackground : MonoBehaviour
{
    public Texture2D[] backgrounds;
    [Min(0.1f)] public float holdSeconds = 6f;
    [Min(0.1f)] public float fadeSeconds = 2f;
    public bool pingPong = true;
    [Range(0f, 1f)] public float panAmount = 0.35f;
    [Min(1f)] public float panPeriod = 28f;
    private RawImage back, front;
    private int current, next, direction = 1;
    private float elapsed, motion;

    void OnEnable()
    {
        if (backgrounds == null || backgrounds.Length == 0) return;
        if (back == null) back = MakeLayer("Background A");
        if (front == null) front = MakeLayer("Background B");
        current = 0; direction = 1; elapsed = 0f; motion = 0f;
        next = NextIndex();
        back.texture = backgrounds[current];
        front.texture = backgrounds[next];
        front.color = new Color(1, 1, 1, 0);
    }

    RawImage MakeLayer(string label)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<RawImage>();
        img.raycastTarget = false;
        return img;
    }

    int NextIndex()
    {
        if (backgrounds.Length < 2) return 0;
        if (!pingPong) return (current + 1) % backgrounds.Length;
        if (current == backgrounds.Length - 1) direction = -1;
        if (current == 0) direction = 1;
        return current + direction;
    }

    void Update()
    {
        if (back == null || backgrounds == null || backgrounds.Length == 0) return;
        float dt = Time.unscaledDeltaTime;
        motion += dt; elapsed += dt;
        float hold = Mathf.Max(0.1f, holdSeconds);
        float fade = Mathf.Max(0.1f, fadeSeconds);
        if (elapsed >= hold + fade)
        {
            elapsed %= hold + fade;
            current = next; next = NextIndex();
            back.texture = backgrounds[current]; front.texture = backgrounds[next];
        }
        float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - hold) / fade));
        front.color = new Color(1, 1, 1, alpha);
        FitAndPan(back); FitAndPan(front);
    }

    void FitAndPan(RawImage img)
    {
        if (img.texture == null) return;
        Rect rect = ((RectTransform)transform).rect;
        if (rect.width <= 0 || rect.height <= 0) return;
        float viewAspect = rect.width / rect.height;
        float imageAspect = (float)img.texture.width / img.texture.height;
        float w = Mathf.Min(1f, viewAspect / imageAspect);
        float h = Mathf.Min(1f, imageAspect / viewAspect);
        float pan = 0.5f + 0.5f * Mathf.Clamp01(panAmount) * Mathf.Sin(motion * 2f * Mathf.PI / Mathf.Max(1f, panPeriod));
        img.uvRect = new Rect((1f - w) * pan, (1f - h) * 0.5f, w, h);
    }

    void OnDisable()
    {
        if (back != null) Destroy(back.gameObject);
        if (front != null) Destroy(front.gameObject);
        back = null; front = null;
    }
}
