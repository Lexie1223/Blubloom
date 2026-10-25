海底开始菜单动态背景 / Ocean Start Background

效果：四张原图，默认 1→2→3→4→3→2→1 往返循环。
每张停留6秒，再用2秒交叉淡化到下一张；画面缓慢左右移动。
图片原样保留，没有把鱼、海草和气泡拆成独立动画。

最快使用方法
1. 解压 ZIP，把里面 Assets 文件夹中的 OceanStartBackground 文件夹放进你项目的 Assets。
   最终路径必须是 Assets/OceanStartBackground。
2. 等 Unity 编译完成，打开你的开始菜单场景。
3. 顶部菜单 Tools → Ocean Background → Create Menu Background，点击一次。
4. 点击 Play，即可看到循环背景。保存场景。
5. 标题和 Start 按钮放在你自己的 Screen Space - Overlay Canvas 上，Sort Order 设为0或更高。
   去掉原菜单中遮住新背景的不透明全屏 Panel / 背景图。

参数：选中 Ocean Background Canvas 下的 Animated Ocean。
Hold Seconds：每张停留多久；Fade Seconds：切换时间。
Ping Pong：勾选为往返；取消为1→2→3→4→1。
Pan Amount：移动幅度，0为静止；Pan Period：一次左右往返需要的秒数。
Backgrounds：可以拖入新 Texture2D，调整顺序。

适配与注意
原图2172×724（3:1），会保持比例铺满屏幕。16:9屏幕会裁掉部分左右区域，慢移展示。
3:1屏幕显示完整图片，因没有额外宽度，不会横移。
背景使用RawImage，不需要把图片设为Sprite；不会拦截按钮点击。
使用unscaledDeltaTime，Time.timeScale=0时仍然播放。
只用于开始菜单；不要把此Canvas设为DontDestroyOnLoad，以免覆盖游戏场景。
需要Unity UI（uGUI）。脚本使用常规UnityEngine.UI API，面向Unity 2022 LTS / Unity 6。
此环境未运行Unity编辑器，导入后请在你们项目中按Play确认效果。
不含Start按钮行为、音乐、游戏逻辑，也不需要外部插件。

素材：用户提供的四张原始JPEG。ZIP里的Preview.html可用浏览器打开预览相同切换节奏。
