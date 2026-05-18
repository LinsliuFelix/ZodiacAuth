using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public enum ZodiacType
{
    Rat, Ox, Tiger, Rabbit, Dragon, Snake,
    Horse, Goat, Monkey, Rooster, Dog, Pig
}

public class ZodiacCube : MonoBehaviour
{
    public ZodiacType zodiacType;               // 在 Inspector 中设置对应的生肖
    public Color highlightColor = Color.cyan; // 高亮的自发光颜色
    public float highlightDuration = 0.5f;      // 高亮持续时间

    private IZodiacClickHandler handler;        // 场景中的管理器接口
    private Material myMaterial;                // 当前立方体的材质实例
    private Color originalEmissionColor;        // 原始自发光颜色
    private Coroutine highlightCoroutine;       // 高亮协程

    void Start()
    {
        // 获取管理器
        handler = ZodiacService.Handler;
        if (handler == null)
            Debug.LogError("当前场景没有注册任何 Zodiac 管理器！");

        // 获取材质实例并记录原始自发光颜色
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            myMaterial = rend.material;   // 创建独立实例，避免影响其他立方体
            // 确保材质启用了 _EMISSION 关键字
            myMaterial.EnableKeyword("_EMISSION");
            // 记录原始的自发光颜色
            originalEmissionColor = myMaterial.GetColor("_EmissionColor");
        }
    }

    void OnMouseDown()
    {
        // 1. 触发高亮视觉反馈
        TriggerHighlight();
        // 2. 通知管理器处理点击
        handler?.OnZodiacClicked(zodiacType);
    }

    /// <summary>
    /// 瞬间切换到高亮自发光颜色，并在指定时间后恢复
    /// </summary>
    void TriggerHighlight()
    {
        if (myMaterial == null) return;

        // 停止之前未完成的恢复协程
        if (highlightCoroutine != null)
            StopCoroutine(highlightCoroutine);

        // 设置自发光颜色为高亮色
        myMaterial.SetColor("_EmissionColor", highlightColor);
        highlightCoroutine = StartCoroutine(RestoreColor());
    }

    IEnumerator RestoreColor()
    {
        yield return new WaitForSeconds(highlightDuration);
        // 恢复原始自发光颜色
        myMaterial.SetColor("_EmissionColor", originalEmissionColor);
        highlightCoroutine = null;
    }

    void OnDestroy()
    {
        // 销毁动态创建的材质实例
        if (myMaterial != null)
            Destroy(myMaterial);
    }

    public void RefreshMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            myMaterial = rend.material;
            if (myMaterial != null)
            {
                myMaterial.EnableKeyword("_EMISSION");
                originalEmissionColor = myMaterial.GetColor("_EmissionColor");
            }
        }
    }
}

// 定义点击回调接口
public interface IZodiacClickHandler
{
    void OnZodiacClicked(ZodiacType type);
}
