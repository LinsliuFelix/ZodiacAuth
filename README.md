# 在VR中基于知识的三维图形身份认证

## 简介
在Unity中构建2×2×3立方体阵列，通过点击十二生肖图片形成序列密码进行认证。
包含注册、登录、随机化、高亮反馈、数据记录等功能。

## 运行环境
- Unity 2022.3.53f1
- Windows 10/11

## 快速开始
1. 克隆仓库
2. 用Unity Hub打开项目
3. 打开 `Scenes/LoginRegister` 场景并运行

## 核心脚本
- `ZodiacCube.cs`：立方体交互与高亮
- `RegisterManager.cs`：注册逻辑
- `LoginManager.cs`：登录认证
- `CameraOrbit.cs`：WASD旋转视角

## 文件说明
- 项目根目录下会生成 `txt/zodiac_password.txt`（注册密码备份）
- 登录成功后生成 `txt/statistics.csv`（统计数据）
