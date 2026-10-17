using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("每关进化所需分数（按关卡顺序：第1关、第2关、第3关...）")]
    public int[] evolveScores = new int[] { 100, 200, 300 };

    [Header("开局生命")]
    public int startHealth = 3;

    public int CurrentScore { get; private set; }
    public int CurrentHealth { get; private set; }
    public int StageIndex { get; private set; }

    void Awake()
    {
        // 单例：保证全程只有一个，切场景不销毁
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentHealth = startHealth;
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
    }

    public void Damage()
    {
        CurrentHealth--;
    }

    // 当前关卡该攒多少分进化
    public int GetEvolveScore()
    {
        if (StageIndex < evolveScores.Length)
            return evolveScores[StageIndex];
        return evolveScores[evolveScores.Length - 1];
    }

    public void AdvanceStage()
    {
        StageIndex++;
    }

    // 死亡重开：清掉所有进度
    public void RestartRun()
    {
        CurrentScore = 0;
        CurrentHealth = startHealth;
        StageIndex = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
