using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("吃一个食物加的分")]
    public int scorePerFood = 10;

    [Header("无敌帧时长")]
    public float invincibilityTime = 1.5f;

    [Header("危险障碍击退力度（Obstacle）")]
    public float knockbackForce = 8f;

    [Header("下一关场景名；最后一关留空字符串")]
    public string nextSceneName = "Level2";

    [Header("进化动画名 / 时长（秒）")]
    public string evolveAnimName = "Cell_Evolve";
    public float evolveAnimLength = 1.5f;

    private bool _isInvincible;
    private bool _hasEvolved;
    private Rigidbody2D _rb;
    private Animator _anim;
    private PlayerMovement _movement;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _movement = GetComponent<PlayerMovement>();
        RefreshUI();
    }

    // 从 GameManager 把数值同步到屏幕
    void RefreshUI()
    {
        if (GameManager.Instance != null && UIManager.Instance != null)
        {
            UIManager.Instance.RefreshScore(GameManager.Instance.CurrentScore);
            UIManager.Instance.RefreshHealth(GameManager.Instance.CurrentHealth);
        }
    }

    // 触发体：食物 / 有毒物
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(scorePerFood);
            RefreshUI();
            Destroy(other.gameObject);
            CheckEvolve();
        }

        if (other.CompareTag("Hazard") && !_isInvincible)
        {
            TakeDamage();
            Destroy(other.gameObject);
        }
    }

    // 实体碰撞：危险障碍（挡+扣血+击退）；Blocker 只挡路，这里不写处理
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Obstacle") && !_isInvincible)
        {
            TakeDamage();
            _rb.linearVelocity = Vector2.zero;
            Vector2 knockbackDir = other.contacts[0].normal;
            _rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    void CheckEvolve()
    {
        if (_hasEvolved || GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentScore >= GameManager.Instance.GetEvolveScore())
            StartCoroutine(EvolveRoutine());
    }

    // 进化：停移动 → 无敌 → 播动画 → 切关
    IEnumerator EvolveRoutine()
    {
        _hasEvolved = true;
        if (_movement != null) _movement.enabled = false;
        _rb.linearVelocity = Vector2.zero;
        _isInvincible = true;

        if (_anim != null) _anim.Play(evolveAnimName);
        yield return new WaitForSeconds(evolveAnimLength);

        // 最后一关：留空就重置整局
        if (string.IsNullOrEmpty(nextSceneName))
        {
            GameManager.Instance.RestartRun();
            yield break;
        }

        GameManager.Instance.AdvanceStage();
        SceneManager.LoadScene(nextSceneName);
    }

    void TakeDamage()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.Damage();
        _isInvincible = true;
        RefreshUI();

        Invoke(nameof(EndInvincible), invincibilityTime);

        if (GameManager.Instance.CurrentHealth <= 0)
            GameManager.Instance.RestartRun();
    }

    void EndInvincible()
    {
        _isInvincible = false;
    }
}
