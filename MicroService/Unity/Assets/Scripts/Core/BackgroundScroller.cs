using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("滚动设置")]
    public float scrollSpeed = 2f; // 向下滚动速度
    public Transform otherBackground; // 另一个背景的 Transform（Inspector 拖拽）

    [Header("屏幕边界")]
    public float backgroundHeight = 10f; // 单个背景高度（根据 Sprite 调整）

    private void Update()
    {
        // 向下滚动
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

        // 当当前背景完全移出屏幕下方时，重置到另一个背景上方
        if (transform.position.y <= -backgroundHeight / 2)
        {
            Vector3 resetPos = otherBackground.position;
            resetPos.y += backgroundHeight; // 放到另一个上方
            transform.position = resetPos;
        }
    }

    // 可选：暂停滚动（Game Over 时）
    public void Pause() => enabled = false;
    public void Resume() => enabled = true;
}