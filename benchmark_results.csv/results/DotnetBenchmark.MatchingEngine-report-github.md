``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 26.04
AMD EPYC 4464P, 1 CPU, 8 logical and 8 physical cores
.NET SDK=10.0.302
  [Host]     : .NET 10.0.10 (10.0.1026.32716), X64 RyuJIT
  DefaultJob : .NET 10.0.10 (10.0.1026.32716), X64 RyuJIT


```
|                   Method |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------- |-----------:|----------:|----------:|-----------:|------:|--------:|-------:|-------:|------:|----------:|
|        DivisionSwitchHit |  10.348 ns | 0.0563 ns | 0.0527 ns |  10.326 ns |  1.00 |    0.00 | 0.0258 | 0.0000 |     - |     216 B |
|          DivisionDataHit |  54.658 ns | 0.9803 ns | 0.9169 ns |  54.760 ns |  5.28 |    0.09 | 0.0401 |      - |     - |     336 B |
|       DivisionSwitchMiss |   2.676 ns | 0.0519 ns | 0.0434 ns |   2.656 ns |  0.26 |    0.00 |      - |      - |     - |         - |
|         DivisionDataMiss |  13.483 ns | 0.0217 ns | 0.0203 ns |  13.485 ns |  1.30 |    0.01 |      - |      - |     - |         - |
| PythagorasSwitchAdjacent |  12.890 ns | 0.0712 ns | 0.0594 ns |  12.868 ns |  1.25 |    0.01 |      - |      - |     - |         - |
|   PythagorasDataAdjacent | 560.113 ns | 1.2901 ns | 1.1436 ns | 560.098 ns | 54.13 |    0.33 | 0.3290 | 0.0010 |     - |   2,752 B |
|     PythagorasDataBuried | 720.904 ns | 3.1204 ns | 2.7662 ns | 721.446 ns | 69.67 |    0.39 | 0.4177 | 0.0029 |     - |   3,496 B |
|   PythagorasDataLongMiss | 356.262 ns | 1.5205 ns | 1.4223 ns | 356.346 ns | 34.43 |    0.20 | 0.1354 |      - |     - |   1,136 B |
|               PassSwitch | 144.928 ns | 2.8374 ns | 4.3330 ns | 142.462 ns | 14.29 |    0.38 | 0.0772 | 0.0002 |     - |     648 B |
|                 PassData | 659.927 ns | 0.8450 ns | 0.7491 ns | 660.107 ns | 63.77 |    0.32 | 0.1106 |      - |     - |     928 B |
