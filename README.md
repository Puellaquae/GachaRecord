# 二游抽卡记录工具

支持原、崩、绝三款游戏的抽卡记录，WinUI3 制作，支持导入导出 [UIGF](https://uigf.org/)

- UI 设计参考自 [Snap.Hutao](https://github.com/DGP-Studio/Snap.Hutao)
- 抽卡记录获取参考自 [Snap.Hutao](https://github.com/DGP-Studio/Snap.Hutao), [star-rail-warp-export](https://github.com/biuuu/star-rail-warp-export), [zzz-signal-search-export](https://github.com/earthjasonlin/zzz-signal-search-export)
- WinUI3 组件使用参考自 [WinUI3 Gallery](https://github.com/microsoft/WinUI-Gallery)
- WinUI3 感觉又是 M$ 的烂尾项目，稍微拓展点功能就得自己来，AI api 都胡编的，网上资料太少了
- 写 UI 还得是 Web 爽，但 WebView 是不能接受的，要 native
- 不支持多语言，原始抽卡数据抓取到的是什么，显示的就是什么
- 虽然支持 [UIGF](https://uigf.org/)，但有些工具导出的数据就只有 `ItemId`，会有问题
- 设计上支持多账户，但我用不到就没做
- Release 模式下代码优化会导致 JSON 用不了，所以目前必须 Debug 模式编译
- 注意数据备份，建议多用几个不同程序多重保障呢
