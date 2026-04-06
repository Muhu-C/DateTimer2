# DateTimer 2
![DateTimer2](https://socialify.git.ci/Muhu-C/DateTimer2/image?font=KoHo&language=1&name=1&owner=1&pattern=Floating+Cogs&stargazers=1&theme=Dark)
-------  
#### 本项目使用 GPL 3.0 License，完全开源免费，禁止倒卖！
  
---

## 更新日志（从新到旧）

### 2.0.4 更新说明  
- 基础性更新  
  - 更新 iNKORE.UI.WPF.Modern 至 0.10.2.1  
  - 版本号开始遵循语义化版本号规则
- 问题修复
  - 修复设置内软件信息与 .csproj 不同的问题 （[920e856](https://github.com/Muhu-C/DateTimer2/commit/920e85646a5905030f86d431dc20ab4015c940bc)）  
  - 修复设置 UI 错位问题和 ToggleSwitch 提示文本为英文的问题 （[920e856](https://github.com/Muhu-C/DateTimer2/commit/920e85646a5905030f86d431dc20ab4015c940bc),  [5f1105c](https://github.com/Muhu-C/DateTimer2/commit/5f1105cf4b1e41467eab6c7b0f65b1404680f9a1)）  
  - 修复打开设置页面报错的问题  （[920e856](https://github.com/Muhu-C/DateTimer2/commit/920e85646a5905030f86d431dc20ab4015c940bc),  [5f1105c](https://github.com/Muhu-C/DateTimer2/commit/5f1105cf4b1e41467eab6c7b0f65b1404680f9a1)）  
- 留存问题
  - 打开过设置页面，时间过长有概率会出现主窗口崩溃的情况（[920e856](https://github.com/Muhu-C/DateTimer2/commit/920e85646a5905030f86d431dc20ab4015c940bc)），请留意此问题并上报

---

### 2.0.2.0 更新说明
- 应用环境： 由 .NET Framework 4.7.2 转移至 .NET 8.0
- 软件包：iNKORE.UI.WPF.Modern 由 0.9.30 更新到 0.10.0
- 功能更新：
  - “编辑时间表”页面时间表过期提醒
  - “编辑时间表”页面编辑窗口日期或星期日只可以选择其中一个
- 应用优化：
  - 气球提示加入至控制台“最近消息”中
  - 修复了文件资源管理器中应用版本不正确的问题
  - 更改背景效果时，消息框背景效果包括在内
  - 优化部分排版
  - 优化“编辑时间表”页面中下拉列表中时间表名称的显示
  - 优化“设置”页面中“更新时间表”功能，添加5秒冷却时间，修复快速刷新时间表导致时间表混乱的问题
  - 优化“时间表”窗口高亮当前时间段的功能
  - 修复提前提醒时间更改后不生效的问题
  - 修复“编辑时间表“页面中”新建时间表”窗口显示原先选中时间表的问题
  - 清空待办页，准备更新待办功能

---

### 2.0.0.0 更新说明（相比 DateTimer 1.2.0）  
  
- UI 控件：从 HandyControl 更换为 iNKORE.UI.WPF.Modern。用WinUI 3，使控制台与时间表窗口操作更便捷  
- 功能更新：  
  - 复刻时间表显示、时间表管理的功能  
  - 新增数项功能  
    - 选择时间表  
    - 倒计时显示设置  
    - 提前提醒功能  
    - 控制台显示设置  
    - 云母或亚克力效果设置  
    - 判断时间段设置是否有误  
  - **待办功能仍在开发中**  
- 优化  
  - 优化“编辑时间表”的保存功能，减少保存功能的代码量  
  - 优化“设置”功能，取消“保存设置”按钮，减少用户设置繁杂度  
  - 加入“最近消息”功能，使用户更易看见消息  
  - 优化“关于"功能  
