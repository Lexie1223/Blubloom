using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;

    private AudioClip _bgmClip;
    private AudioClip _eatClip;
    private AudioClip _impactClip;
    private AudioClip _levelUpClip;
    private AudioClip _clickClip;

    [Header("音量 0-10")]
    public int bgmVolume = 8;
    public int sfxVolume = 8;

    public static void Ensure()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _sfxSource = gameObject.AddComponent<AudioSource>();

        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 0f;

        _bgmClip = Resources.Load<AudioClip>("Audio/bgm");
        _eatClip = Resources.Load<AudioClip>("Audio/eat");
        _impactClip = Resources.Load<AudioClip>("Audio/impact");
        _levelUpClip = Resources.Load<AudioClip>("Audio/levelup");
        _clickClip = Resources.Load<AudioClip>("Audio/uiclick");

        ApplyVolumes();
    }

    void Start()
    {
        PlayBGM();
    }

    private void ApplyVolumes()
    {
        if (_bgmSource != null) _bgmSource.volume = bgmVolume / 10f;
        if (_sfxSource != null) _sfxSource.volume = sfxVolume / 10f;
    }

    public void BgmUp() { bgmVolume = Mathf.Min(10, bgmVolume + 1); ApplyVolumes(); PlayClick(); }
    public void BgmDown() { bgmVolume = Mathf.Max(0, bgmVolume - 1); ApplyVolumes(); PlayClick(); }
    public void SfxUp() { sfxVolume = Mathf.Min(10, sfxVolume + 1); ApplyVolumes(); PlayClick(); }
    public void SfxDown() { sfxVolume = Mathf.Max(0, sfxVolume - 1); ApplyVolumes(); PlayClick(); }

    // 滑动条直接设置音量（0-10）
    public void SetBgmVolume(int v) { bgmVolume = Mathf.Clamp(v, 0, 10); ApplyVolumes(); }
    public void SetSfxVolume(int v) { sfxVolume = Mathf.Clamp(v, 0, 10); ApplyVolumes(); }

    // 给滑动条用的 0~1 音量值
    public float GetBgmVolume01() => bgmVolume / 10f;
    public float GetSfxVolume01() => sfxVolume / 10f;

    public string GetBgmText() => bgmVolume.ToString();
    public string GetSfxText() => sfxVolume.ToString();

    public void PlayBGM()
    {
        if (_bgmSource == null || _bgmClip == null) return;
        if (_bgmSource.clip == _bgmClip && _bgmSource.isPlaying) return;
        _bgmSource.clip = _bgmClip;
        _bgmSource.Play();
    }

    public void PlayEat() => PlayOneShot(_eatClip);
    public void PlayImpact() => PlayOneShot(_impactClip);
    public void PlayLevelUp() => PlayOneShot(_levelUpClip);
    public void PlayClick() => PlayOneShot(_clickClip);

    private void PlayOneShot(AudioClip clip)
    {
        if (_sfxSource == null || clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }
}
