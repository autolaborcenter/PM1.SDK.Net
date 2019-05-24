# PM1 SDK

## 介绍

[.Net](https://dotnet.microsoft.com/) 是微软开发应用程序平台，旨在为桌面、移动端、网络和游戏开发提供一致的体验。Autolabor PM1 通过 [Autolabor.PM1.SDK.Net 库](https://github.com/autolaborcenter/Autolabor.PM1.SDK.Net)（此后简称 SDK）支持在 .Net 平台的二次开发。微软在 .Net 平台上实现了多种编程语言，SDK 完全使用 C# 开发，但可以使用 C#、F# 或 VB.Net 正常调用。

## 环境要求

支持 .Net 平台的目的是加速 Windows 桌面端的开发，而非完全拥抱 .Net 生态，因此 .Net SDK 依赖于 Native SDK。

.Net 具有平台无关性，开发 .Net 应用程序依赖于 .Net SDK 的版本而非系统的版本。PM1 .Net SDK 基于 .Net Standard 2.0，对应关系见下表：

| 平台            | 最低版本   |
| -------------- | ---------- |
| .Net Standard  | 2.0        |
| .Net Core      | 2.0        |
| .Net Framework | 4.6.1      |
| UWP            | 10.0.16299 |
| Unity          | 2018.1     |

> 查看[文档](https://docs.microsoft.com/zh-cn/dotnet/standard/net-standard)以确认其支持性。

依赖要求基于 SDK 构建的其他库或应用程序必须高于上表所示的版本，这需要 **Visual Studio 2017** 或更高版本开发，需要 **Windows 7** 或更高版本运行。

> 如果您无法升级到 VS 2017，则可使用 VS 2015 + [.Net Core Tools](https://github.com/dotnet/core/blob/master/release-notes/download-archive.md)。
