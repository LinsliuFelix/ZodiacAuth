using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;   // 新增
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LoginManager : MonoBehaviour, IZodiacClickHandler
{
    public static IZodiacClickHandler Instance { get; private set; }

    void Awake()
    {
        ZodiacService.Handler = this;
    }

    void OnDestroy()
    {
        if (ZodiacService.Handler == (IZodiacClickHandler)this)
            ZodiacService.Handler = null;
    }

    [Header("UI 引用")]
    public Text feedbackText;
    public Button loginButton;
    public Button resetButton;
    public Button backButton;

    [Header("随机分配配置")]
    public List<ZodiacCube> cubeScripts = new List<ZodiacCube>(); // 拖入12个ZodiacCube组件
    public List<Material> zodiacMaterials = new List<Material>(); // 按顺序放12个生肖材质（鼠0,牛1,...猪11）

    private List<ZodiacType> inputSequence = new List<ZodiacType>();
    private Coroutine clearCoroutine;

    void Start()
    {
        // 随机分配图片与生肖类型
        RandomizeZodiacs();

        loginButton.onClick.AddListener(OnLogin);
        resetButton.onClick.AddListener(OnReset);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("lr"));
        feedbackText.text = "";
    }

    /// <summary>
    /// 随机打乱12个立方体的材质和对应生肖类型
    /// </summary>
    void RandomizeZodiacs()
    {
        if (cubeScripts.Count != 12 || zodiacMaterials.Count != 12)
        {
            Debug.LogError("请确保 cubeScripts 和 zodiacMaterials 各有12个元素！");
            return;
        }

        // 1. 创建一个材质列表的副本并随机打乱
        List<Material> shuffledMaterials = new List<Material>(zodiacMaterials);
        ShuffleList(shuffledMaterials);

        // 2. 为每个立方体设置打乱后的材质，并更新其ZodiacType
        for (int i = 0; i < cubeScripts.Count; i++)
        {
            if (cubeScripts[i] == null) continue;

            // 给立方体设置材质
            var rend = cubeScripts[i].GetComponent<Renderer>();
            if (rend != null)
            {
                var newMat = shuffledMaterials[i];
                rend.material = newMat;
                newMat.EnableKeyword("_EMISSION");       // 修复点1
            }
            // 根据材质的名称或顺序推断生肖类型
            // 因为 shuffledMaterials[i] 对应某个原始材质，我们可以反向查找它在 zodiacMaterials 中的索引
            int originalIndex = zodiacMaterials.IndexOf(shuffledMaterials[i]);
            cubeScripts[i].zodiacType = (ZodiacType)originalIndex;
            cubeScripts[i].RefreshMaterial();            // 修复点2
        }

        Debug.Log("已随机分配十二生肖位置");
    }

    /// <summary>
    /// Fisher–Yates 洗牌算法
    /// </summary>
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    public void OnZodiacClicked(ZodiacType type)
    {
        if (inputSequence.Count >= 5)
        {
            Debug.LogWarning("序列最多 5 位！");
            return;
        }
        inputSequence.Add(type);
    }

    void OnLogin()
    {
        // 检查序列长度
        if (inputSequence.Count < 3)
        {
            ShowErrorAndReset("序列至少需要 3 位！");
            return;
        }

        // 检查是否已注册密码
        string savedPassword = ZodiacService.Password;
        if (string.IsNullOrEmpty(savedPassword))
        {
            ShowErrorAndReset("尚未注册任何密码！");
            return;
        }

        // 将当前输入转为字符串
        string currentPassword = "";
        for (int i = 0; i < inputSequence.Count; i++)
        {
            currentPassword += (int)inputSequence[i];
            if (i < inputSequence.Count - 1) currentPassword += ",";
        }

        // 比对
        if (currentPassword == savedPassword)
        {
            // 计算耗时
            string result = "认证成功！";
            if (ZodiacService.RegisterTime.HasValue)
            {
                System.TimeSpan duration = System.DateTime.Now - ZodiacService.RegisterTime.Value;
                result += $" 耗时：{duration.TotalSeconds:F1} 秒";
                result += $" 失败次数：{ZodiacService.LoginFailCount}";
                WriteStatistics(duration, ZodiacService.LoginFailCount);   // 写入文件
            }
            feedbackText.text = result;
            feedbackText.color = Color.green;
        }
        else
        {
            //增加失败次数
            ZodiacService.LoginFailCount++; 
            ShowErrorAndReset("认证失败，序列不匹配。");
        }
    }

    void WriteStatistics(TimeSpan duration, int failCount)
    {
        string dir;
#if UNITY_EDITOR
        // 编辑器下：项目根目录/txt/
        dir = Path.Combine(Application.dataPath, "C:/Users/23972/bs/Assets/txt");
#else
    dir = Application.persistentDataPath;
#endif
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        string filePath = Path.Combine(dir, "statistics.csv");
        string line = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss},{duration.TotalSeconds:F1},{failCount}";

        // 如果文件不存在，先写入列名
        if (!System.IO.File.Exists(filePath))
        {
            System.IO.File.WriteAllText(filePath, "时间,耗时(秒),失败次数\n");
        }

        System.IO.File.AppendAllText(filePath, line + "\n");
    }
    void ShowErrorAndReset(string message)
    {
        feedbackText.text = $"{message} 已失败 {ZodiacService.LoginFailCount} 次";
        feedbackText.color = Color.red;

        inputSequence.Clear();

        StopClearCoroutine();
        clearCoroutine = StartCoroutine(ClearFeedbackAfterDelay(2f));
    }

    void StopClearCoroutine()
    {
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }
    }

    IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
        clearCoroutine = null;
    }

    void OnReset()
    {
        inputSequence.Clear();
        feedbackText.text = "";
        StopClearCoroutine();
    }
}