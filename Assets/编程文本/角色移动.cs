using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动速度，可以在Inspector里直接调")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        // 自动获取身上的刚体组件
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 读取WASD和方向键输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 归一化，斜着走速度不会变快
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 用物理方式移动
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
