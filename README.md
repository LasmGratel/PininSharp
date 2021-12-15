# PininSharp

C# 的拼音搜索支持，目标框架为 .NET Standard 2.1。

## 性能

相对 Java 版大幅提升

部分匹配
| 匹配逻辑 | 构建耗时 | 搜索耗时 |
| ------- | -------- | ------- |
| TreeSearcher  |  156ms  |  0.01ms |
| SimpleSearcher  |  6.43ms  |  49ms |
| CachedSearcher  |  9.26ms  |  0.281ms |

前缀匹配
| 匹配逻辑 | 构建耗时 | 搜索耗时 |
| ------- | -------- | ------- |
| TreeSearcher  |  15.7ms  |  0.0009ms |
| SimpleSearcher  |  6.52ms  |  4.1ms |
| CachedSearcher  |  9.39ms  |  0.0006ms |

测试环境

``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22518
Processor=Intel i7-11800H
  [Host]     : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT
```

## 使用

```csharp
var tree = new TreeSearcher<int>(SearcherLogic.Contain, PinIn.CreateDefault());
tree.Put("测试文本", 1);
tree.Put("测试切分", 5);
tree.Put("测试切分文本", 6);

List<int> s;
s = tree.Search("ceshi");
s // [1, 5, 6]
```

## Credits

<https://github.com/Towdium/PinIn>

