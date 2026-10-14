using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // 全局单例，其他脚本自动调用
    public static UIManager Instance;

    [Header("UI文本组件：把对应物体拖进来")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 更新分数显示
    public void RefreshScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    // 更新血量显示
    public void RefreshHealth(int health)
    {
        healthText.text = "Health: " + health;
    }
}
