# 自由开局 / Free Start

为《深空当铺：可能是偷的》（Probably Stolen）自定义新开局天赋点数和基础槽位。

**作者：Cryingcatcat**  
**贡献者：Cryingcatcat、Codex（OpenAI AI 编程助手）**

## 功能

- 默认 **99 天赋点、99 个基础槽位**，在游戏模组设置中直接输入其他整数。
- 独立的无限天赋点数、无限槽位开关。
- 可允许首局选择天赋，默认开启。
- 保留原生天赋互斥、负面天赋返还点数和额外槽位加成。
- 保留原生已选天赋分页。

只影响新游戏的天赋选择，不会给已有存档补加天赋。

## 安装与设置

1. 下载本仓库的发行包并解压。
2. 将其中的 `Mods` 文件夹复制到 `Probably Stolen.exe` 所在目录。
3. 启动游戏，在 **Mods / Modding** 中启用 **自由开局：天赋点数与槽位**，按提示重启。
4. 点击模组旁的齿轮，调整设置并点击 **Apply / 应用**。
5. 重新打开新游戏的天赋选择界面。

正确目录：

```text
Probably Stolen/
└─ Mods/
   └─ FreeStart/
      ├─ manifest.xml
      └─ FreeStart.dll
```

使用游戏自带的 Mono 模组加载器，无额外前置。不需要 MelonLoader 或 BepInEx。
不要把源码、编译脚本或 ZIP 放进 `Mods/FreeStart`。

| 设置 | 默认值 | 说明 |
| --- | --- | --- |
| 初始天赋点数 | 99 | 0～2147483647 的整数；负数按零处理 |
| 基础天赋槽位数 | 99 | 天赋自身的槽位加成继续叠加 |
| 无限天赋点数 | 关闭 | 开启后忽略点数限制 |
| 无限天赋槽位 | 关闭 | 开启后忽略槽位限制 |
| 首局也可选择天赋 | 开启 | 不修改已游玩次数 |

**0 表示零，不代表无限。** 无限模式显示 `∞`。
恢复原版数值：点数设为 **3**、槽位设为 **4**，关闭两个无限开关；首局规则也可单独恢复。

## 兼容性与验证范围

针对 Windows Steam 正式版 Mono 程序集编译，验证基于 **Steam build 26337366**，内部版本字符串为 **0.46D**。
不适用于旧 IL2CPP Demo / Playtest。游戏更新后可能需要适配。

已完成编译、原版 `ModScanner` 扫描和离线逻辑检查：扫描无额外权限、无硬性拦截；限制逻辑、负数与大数值处理、原生分页和停用恢复通过。
首局补丁已核对真实游戏指令。**尚未完成游戏内界面及完整开局实测。**

不要同时启用其他修改相同开局限制或界面的模组。完整记录见 [验证记录](验证记录.txt)。

## 卸载

在游戏中停用，退出后删除 `Mods/FreeStart`。之后的新开局恢复原版。
已选的原版天赋继续由游戏自身保存；本模组不删除存档。

配置保存在游戏用户数据目录的 `ModConfigs/custom_free_start.json`，卸载时可以保留。

## 从源码编译

需要 Windows 自带的 .NET Framework C# 编译器及合法安装的游戏。

```powershell
.\Source\build.ps1 -GamePath 'C:\Steam\steamapps\common\Probably Stolen'
```

输出位于 `Mods/FreeStart/FreeStart.dll`。仓库不包含游戏 DLL、反编译游戏代码或游戏素材。

参考：[官方模组文档](https://github.com/dragonpan2/probably-stolen-modding)。

## Credits

- **Cryingcatcat** — 作者、功能需求与项目维护。
- **Codex** — AI 辅助实现、离线验证及发布准备。
