using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ZodiacService
{
    public static IZodiacClickHandler Handler { get; set; }
    public static string Password { get; set; }
    public static DateTime? RegisterTime { get; set; }    // 注册成功的时间点
    public static int LoginFailCount { get; set; }        // 失败次数

    /// <summary>
    /// 重置跟踪状态（开始新一轮计时）
    /// </summary>
    public static void ResetTracking()
    {
        RegisterTime = null;
        LoginFailCount = 0;
    }
}