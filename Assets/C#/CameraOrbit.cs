using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("旋转中心")]
    public Transform centerPoint;          // 拖入 CenterPoint

    [Header("旋转速度")]
    public float rotationSpeed = 90f;      // 度/秒

    [Header("俯仰限制")]
    public float minPitch = -30f;          // 最低角度
    public float maxPitch = 60f;           // 最高角度

    [Header("初始随机范围")]
    public float randomYawMin = 0f;        // 水平角随机最小值
    public float randomYawMax = 360f;      // 水平角随机最大值
    public float randomPitchMin = -20f;    // 俯仰角随机最小值
    public float randomPitchMax = 40f;     // 俯仰角随机最大值

    private float currentYaw = 0f;
    private float currentPitch = 0f;

    void Start()
    {
        if (centerPoint == null)
        {
            Debug.LogError("请将 CenterPoint 拖入 CameraOrbit 组件！");
            return;
        }

        // 随机初始角度
        currentYaw = Random.Range(randomYawMin, randomYawMax);
        currentPitch = Random.Range(randomPitchMin, randomPitchMax);

        // 应用初始位置
        UpdateCameraPosition();
    }

    void Update()
    {
        if (centerPoint == null) return;

        // WASD 输入
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;

        // 更新角度
        currentYaw += horizontal * rotationSpeed * Time.deltaTime;
        currentPitch += vertical * rotationSpeed * Time.deltaTime;

        // 限制俯仰角
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // 更新摄像机位置
        UpdateCameraPosition();
    }

    /// <summary>
    /// 根据当前水平和俯仰角，将摄像机放在以 centerPoint 为中心的球面上，并看向中心
    /// </summary>
    void UpdateCameraPosition()
    {
        // 计算方向
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        // 摄像机与中心的距离可取固定值（例如 8），或根据初始距离动态保留
        float distance = Vector3.Distance(transform.position, centerPoint.position);
        // 如果距离为 0，设定一个默认距离
        if (distance < 0.1f) distance = 8f;

        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = centerPoint.position + offset;
        transform.LookAt(centerPoint.position);
    }
}