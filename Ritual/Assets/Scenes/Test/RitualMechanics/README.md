# Ritual 五个方块测试场景

在 `Assets/Scenes/Test` 打开任意一个场景，点击 Unity 的 Play 即可独立测试。画面中的英文操作说明避免默认 TMP 字体缺少中文字形。

| 场景 | 操作与目标 |
| --- | --- |
| 01_PlayerControls | A/D 横向移动，靠近黄色方块按 E 交互；Tab 打开/关闭笔记页面；Esc 设置；按住 Shift 疾跑，Ctrl 下蹲。 |
| 02_PicturePuzzle | 九块图像随机打乱；点击两块交换位置，参照右侧原图恢复。此版本是交换式拼图，不留空格。 |
| 03_CrouchScanner | A/D 前进，红色扫描来临时按住 Ctrl 隐藏身形；站立被扫到会回起点，抵达绿色出口成功。 |
| 04_MorseCode | Play 播放四位随机数字的摩斯声音，右侧密码本提供 0–9 对照；点击数字键输入，Submit 提交。可重播、停止、退格或换新密码。 |
| 05_HiddenKey | 鼠标左键拖开五个叠放方块；所有方块完全离开钥匙区域后，点击底层黄色 KEY 收集。 |

疾跑消耗耐力，松开后恢复；耐力耗尽后需恢复至 25% 才能再次疾跑。下蹲速度较慢，疾跑与下蹲不同时生效。

Esc 设置提供音量调节与继续。打开设置或笔记时，测试玩法暂停；关闭后继续。每个场景可通过自己的 Reset / Shuffle / New code 按钮重试。

测试脚本兼容两种键盘输入后端；当前项目优先沿用现有玩家脚本使用的旧输入方式，uGUI 事件使用项目已安装的 Input System。界面使用 TextMeshPro，不依赖原有角色、动画、Yarn 对话或美术资产。场景静态方块、相机和引用保存于 .unity，运行时生成测试界面与程序拼图/音频，不会修改原有正式场景或 Build Settings。

编辑器菜单 `Ritual Tests/Create missing five test scenes` 只生成缺少的测试场景，不覆盖已有场景。`Ritual Tests/Run five-scene smoke tests` 在已打开场景均保存后可运行行为检查，结束后恢复之前打开的场景。结果与截图保存在项目 `Logs/RitualTests`，该检查不代替人工试听与完整游戏测试。
