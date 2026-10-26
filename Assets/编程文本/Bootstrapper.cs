using UnityEngine;

// 不管从哪个场景直接按 Play，都自动生成音频和暂停系统
public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        AudioManager.Ensure();

        if (PauseManager.Instance == null)
        {
            new GameObject("PauseManager").AddComponent<PauseManager>();
        }
    }
}
