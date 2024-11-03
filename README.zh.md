## 贡献与改进
我的中文不太好，和检查问题有点难，所以要是你能帮我改进翻译，我会非常感激！请随时提交 Pull Request 或提供反馈，让这个项目更好地服务中文社区。谢谢你的帮助！

<p align="center">
  <img width="65%" src="https://github.com/user-attachments/assets/56e921ff-e463-4ab3-b687-92f248dc727e">
</p>
<p align="center">
  <a href="https://github.com/unchihugo/FluentFlyout/blob/master/README.md">English</a> | <strong>简体中文</strong> | <a href="https://github.com/unchihugo/FluentFlyout/blob/master/README.nl.md">Nederlands</a>
</p>

---
FluentFlyout 是一个简单且现代的 Windows 音量控制弹窗，基于 Fluent 2 设计原则构建。它的用户界面与 Windows 10/11 无缝融合，为您在控制媒体时提供一个干净、原生般的无干扰体验。  

FluentFlyout 具有流畅的动画效果，能够与系统的颜色主题相融合，提供多种布局位置和个性化设置选项，同时在一个美观现代的弹窗中显示媒体控制和信息。

<a href="https://apps.microsoft.com/detail/9N45NSM4TNBP?mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

## 功能 ✨
- 原生 Windows 风格设计
- 使用 Fluent 2 组件
- 采用 Windows Mica 模糊效果
- 支持明暗模式
- 与设备主题颜色匹配
- 流畅的动画效果
- 可自定义弹窗位置
- 包含“全部循环”、“单曲循环”和“随机播放”功能
- 同时监听音量和媒体输入
- 无干扰地驻留在系统托盘
- **音乐弹窗：显示封面、标题、艺术家和媒体控制**
- **“即将播放”弹窗：在歌曲结束时显示下一首曲目**

## 音乐弹窗 🎵
<div align="center">
	<img height="205px" width="auto" src="https://github.com/user-attachments/assets/4dab1c12-594a-4785-bddc-0da1783bf1c8"> <img height="205px" src="https://github.com/user-attachments/assets/b4306026-b274-418b-a39e-78877e7610a7"> 	<img height="190px" src="https://github.com/user-attachments/assets/39de69fe-54c8-4b22-880c-7f0370b8dd9c"> <img height="190px" src="https://github.com/user-attachments/assets/a25adb0e-963a-49a5-8abb-d9a288c2ad9a">
</div>

## 怎么下载？
### 通过 Microsoft Store 安装（如果可以）
<a href="https://apps.microsoft.com/detail/9N45NSM4TNBP?mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="300"/>
</a>

> 在寻找 FluentFlyout 设置吗？您可以通过点击系统托盘图标来访问设置。
### 用 .msixbundle 安装程序（如果 Microsoft Store 不可用）
> [!Important]
> 如果可能，建议从 Microsoft Store 下载 FluentFlyout，这更方便并支持自动更新。
1. 前往 [最新发布](https://github.com/unchihugo/FluentFlyout/releases/latest) 页面
2. 下载 **"*.cer"** 文件
3. 打开证书，并使用管理员权限进行安装
4. 将证书放入 **"受信任的根证书颁发机构"** 中
5. 下载 **"*.msixbundle"**
6. 应用安装程序将弹出，点击 **"安装"**，如果之前已安装 FluentFlyout，点击 **"更新"**
7. 完成！试试播放音乐，并使用您的媒体或音量键

## 即将推出的功能 📝
- [x] 设置
- [x] 可编辑的弹窗超时
- [x] 实现紧凑布局
- [x] 移除 Windows Forms 依赖
- [ ] 增加更多媒体控制（循环✅，随机播放✅，进度滑块）
- [ ] 更多动画效果
- [ ] 从 `alt+tab` 中移除窗口
### 问题
- Windows 10 界面可能无法如预期显示

## 贡献 💖
欢迎以任何方式贡献！请查看 [CONTRIBUTING.md](https://github.com/unchihugo/FluentFlyout/blob/master/.github/CONTRIBUTING.md) 开始贡献。

## 致谢 🙌
[Hugo Li](https://unchihugo.github.io) - 原始开发者、Microsoft Store 发布者、中文和荷兰翻译
### 依赖
- [Dubya.WindowsMediaController](https://github.com/DubyaDude/WindowsMediaController)
- [MicaWPF](https://github.com/Simnico99/MicaWPF)
- [WPF-UI](https://github.com/lepoco/wpfui)