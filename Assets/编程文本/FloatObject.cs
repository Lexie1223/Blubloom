using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [Header("浮动高度")]
    public float amplitude = 0.3f;
    [Header("浮动速度")]
    public float speed = 1.5f;

    private float _startY;
    private float _randomOffset;

    void Start()
    {
        _startY = transform.position.y;
        // 随机相位，多个物体不会同步上下
        _randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float newY = _startY + Mathf.Sin(Time.time * speed + _randomOffset) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
