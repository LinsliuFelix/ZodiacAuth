using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour, IZodiacClickHandler
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
    public Text sequenceText;        // 显示已选序列
    public Button registerButton;   // 注册按钮
    public Button resetButton;      // 重置按钮
    public Button backButton;       // 返回入口场景

    [Header("随机分配配置")]
    public List<ZodiacCube> cubeScripts = new List<ZodiacCube>(); // 拖入12个ZodiacCube组件
    public List<Material> zodiacMaterials = new List<Material>(); // 按顺序放12个生肖材质（鼠0,牛1,...猪11）

    private List<ZodiacType> selectedSequence = new List<ZodiacType>();

    void Start()
    {
        // ★ 启动时随机打乱立方体的生肖布局
        RandomizeZodiacs();

        // 按钮监听
        registerButton.onClick.AddListener(OnRegister);
        resetButton.onClick.AddListener(OnReset);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("lr"));

        UpdateSequenceUI();
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

            var rend = cubeScripts[i].GetComponent<Renderer>();
            if (rend != null)
            {
                var newMat = shuffledMaterials[i];
                rend.material = newMat;
                newMat.EnableKeyword("_EMISSION");       // 修复点1
            }
            // 根据材质的索引找到对应的生肖枚举
            int originalIndex = zodiacMaterials.IndexOf(shuffledMaterials[i]);
            cubeScripts[i].zodiacType = (ZodiacType)originalIndex;
            cubeScripts[i].RefreshMaterial();            // 修复点2
        }

        Debug.Log("已随机分配十二生肖位置（注册场景）");
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

    /// <summary>
    /// 点击生肖立方体的回调
    /// </summary>
    public void OnZodiacClicked(ZodiacType type)
    {
        if (selectedSequence.Count >= 5)
        {
            Debug.LogWarning("序列最多只能包含 5 位！");
            return;
        }
        selectedSequence.Add(type);
        UpdateSequenceUI();
    }

    /// <summary>
    /// 点击注册按钮，存储密码
    /// </summary>
    void OnRegister()
    {
        if (selectedSequence.Count < 3)
        {
            Debug.LogWarning("序列至少需要 3 位！");
            return;
        }

        // 1. 将生肖序列转为字符串
        string password = "";
        for (int i = 0; i < selectedSequence.Count; i++)
        {
            password += (int)selectedSequence[i];
            if (i < selectedSequence.Count - 1) password += ",";
        }

        // 2. 将密码存入内存
        ZodiacService.Password = password;
        // 开始新的跟踪周期
        ZodiacService.ResetTracking();
        ZodiacService.RegisterTime = System.DateTime.Now;
        // 3. 写入文本文件
        string dir;
#if UNITY_EDITOR
        dir = System.IO.Path.Combine(Application.dataPath, "C:/Users/23972/bs/Assets/txt");
#else
        dir = Application.persistentDataPath;
#endif
        // 确保文件夹存在
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string filePath = System.IO.Path.Combine(dir, "zodiac_password.txt");
        System.IO.File.WriteAllText(filePath, password);
        Debug.Log($"密码已存入内存，并备份到文件：{filePath}");

        // 4. 跳转回入口场景
        SceneManager.LoadScene("lr");
    }

    void OnReset()
    {
        selectedSequence.Clear();
        UpdateSequenceUI();
    }

    void UpdateSequenceUI()
    {
        string display = "当前序列：";
        foreach (var type in selectedSequence)
            display += ZodiacToChinese(type) + " ";
        sequenceText.text = display;
    }

    string ZodiacToChinese(ZodiacType type) => type switch
    {
        ZodiacType.Rat => "鼠",
        ZodiacType.Ox => "牛",
        ZodiacType.Tiger => "虎",
        ZodiacType.Rabbit => "兔",
        ZodiacType.Dragon => "龙",
        ZodiacType.Snake => "蛇",
        ZodiacType.Horse => "马",
        ZodiacType.Goat => "羊",
        ZodiacType.Monkey => "猴",
        ZodiacType.Rooster => "鸡",
        ZodiacType.Dog => "狗",
        ZodiacType.Pig => "猪",
        _ => "？"
    };
}