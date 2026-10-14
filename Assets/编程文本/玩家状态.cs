using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHealth = 3;
    public int currentHealth;
    public int currentScore = 0;

    [Header("无敌帧设置")]
    public float invincibilityTime = 1.5f;
    private bool _isInvincible;

    [Header("障碍物击退力度")]
    public float knockbackForce = 8f;

    private Rigidbody2D _rb;

    void Start()
    {
        currentHealth = maxHealth;
        _rb = GetComponent<Rigidbody2D>();

        // 自动同步UI初始值
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshScore(currentScore);
            UIManager.Instance.RefreshHealth(currentHealth);
        }
    }

    // 触发类物体：食物、有毒物质（可穿过、吃掉消失）
    void OnTriggerEnter2D(Collider2D other)
    {
        // 吃到食物：加分 + 销毁物体
        if (other.CompareTag("Food"))
        {
            currentScore += 10;
            if (UIManager.Instance != null)
                UIManager.Instance.RefreshScore(currentScore);
            Destroy(other.gameObject);
        }

        // 吃到有毒物质：扣血 + 销毁物体（吃掉消失）
        if (other.CompareTag("Hazard") && !_isInvincible)
        {
            TakeDamage();
            Destroy(other.gameObject);
        }
    }

    // 实体碰撞：障碍物（挡路 + 击退 + 扣血）
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Obstacle") && !_isInvincible)
        {
            TakeDamage();

            // 先清空当前速度，保证击退力度稳定
            _rb.linearVelocity = Vector2.zero;
            // 沿碰撞面法线方向弹开
            Vector2 knockbackDir = other.contacts[0].normal;
            _rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    // 扣血统一逻辑
    void TakeDamage()
    {
        currentHealth--;
        _isInvincible = true;

        if (UIManager.Instance != null)
            UIManager.Instance.RefreshHealth(currentHealth);

        // 无敌时间结束后恢复
        Invoke(nameof(EndInvincible), invincibilityTime);

        // 血量归零重置当前关卡
        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void EndInvincible()
    {
        _isInvincible = false;
    }
}
