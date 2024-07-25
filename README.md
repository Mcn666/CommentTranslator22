# CommentTranslator22

在 VS Code 中有个名为 Comment Translate 的扩展，Visual Studio 没有此扩展，于是我尝试实现类似的效果。

## 参考扩展源：
- [CommentTranslator](https://github.com/LoveHikari/comment-translator)
- [EnTranslate](https://github.com/Entity-Now/EnTranslate)
- [VsTranslator](https://github.com/Kerwin1202/VsTranslator)
- [Codist](https://github.com/wmjordan/Codist)
- [VSIntelliSenseTweaks](https://github.com/cfognom/VSIntelliSenseTweaks)
- [IntelliSenseExtender](https://github.com/Dreamescaper/IntelliSenseExtender)
- ...(有些我忘了)

## ...
- 参考的这些扩展源中很多不错的设计，但这个项目目的是解决语言阅读的一些障碍，因此不打算增加太多不符合本质的功能。
- 对于翻译语言的方法注释这块，我比较倾向于将这个翻译后的内容存储在本地，以避免经常使用网络翻译，理想情况下是将常用的方法压缩为缓存包。
- 在开发中经常遇到问题，导致进度会卡很久，因为部分接口没有或者只有少量的相关示例文档，不然就是在最新的VS版本中，部分接口被弃用或改用其它新接口了。
- 在这个项目中仍然有许多待解决的问题，时间精力有限，也许以后会继续完善，也许在以后的某天就不再更新了