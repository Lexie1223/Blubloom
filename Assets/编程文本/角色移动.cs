using UnityEngine;
using UnityEngine.InputSystem;

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
        float moveX = 0;
        float moveY = 0;

        // 左右：A/D + 左右方向键
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveX -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveX += 1;

        // 上下：W/S + 上下方向键
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveY += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveY -= 1;

        // 归一化，斜着走速度不会变快
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // 用物理方式移动
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
