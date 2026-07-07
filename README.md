# SnackStealth

SnackStealth 是一个 Unity 6 3D 潜行喜剧游戏原型。玩家扮演随堂测验中的学生，在课堂里偷偷寻找零食、占座、偷吃，同时躲避老师巡逻和台上同学的视线。

## 当前内容

- 3D 教室场景与可替换模型结构
- 第一人称移动、坐下、起立和跳跃
- 桌内有限零食与饱腹值系统
- 老师巡逻、视野检测和被抓惩罚
- 被踢走同学上讲台、随机观察玩家
- 中文 HUD、开场提示、胜负状态和基础特效
- Kenney Furniture Kit 轻量家具素材与程序生成材质

## 环境

- Unity Editor: `6000.3.19f1`
- 推荐打开目录: `unity_project`
- 主场景: `Assets/_SnackStealth/Scenes/Classroom_Blockout.unity`

## 项目结构

```text
SnackStealth/
  unity_project/                 Unity 工程
    Assets/_SnackStealth/         游戏代码、场景、材质、编辑器工具
    Assets/ExternalAssets/        第三方素材，保留许可证文件
    Packages/                    Unity 包配置
    ProjectSettings/             Unity 项目设置
  README.md
  .gitignore
```

## 本地运行

1. 使用 Unity Hub 打开 `E:\A_project\game\SnackStealth\unity_project`。
2. 打开场景 `Assets/_SnackStealth/Scenes/Classroom_Blockout.unity`。
3. 等待 Unity 导入完成，确认 Console 没有红色编译错误。
4. 点击 Play 运行。

## 操作

- `WASD`: 移动
- `鼠标`: 转头
- `Space`: 跳跃
- `F`: 坐下或起立
- `E`: 偷吃当前桌内零食



