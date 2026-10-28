using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // 全局单例，其他脚本自动调用
    public static UIManager Instance;

    [Header("HUD 数值文本：拖图片旁边显示数字的 TMP 文本")]
    public TextMeshProUGUI healthValueText;
    public TextMeshProUGUI scoreValueText;

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

    // 更新分数显示（图片标牌旁边只写数字）
    public void RefreshScore(int score)
    {
        if (scoreValueText != null) scoreValueText.text = score.ToString();
    }

    // 更新血量显示（图片标牌旁边只写数字）
    public void RefreshHealth(int health)
    {
        if (healthValueText != null) healthValueText.text = health.ToString();
    }
}
