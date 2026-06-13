## DecomposeBenchmark

Engine version - 2.0.0

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```
| Method                | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Default               | 1.670 μs | 0.0092 μs | 0.0082 μs |  1.00 |    0.01 | 0.1259 |      - |   6.35 KB |        1.00 |
| IncludeRoot           | 1.879 μs | 0.0078 μs | 0.0069 μs |  1.12 |    0.01 | 0.1545 | 0.0038 |   7.61 KB |        1.20 |
| IncludeFields         | 1.828 μs | 0.0104 μs | 0.0092 μs |  1.09 |    0.01 | 0.1373 | 0.0019 |   6.77 KB |        1.07 |
| IncludeEvents         | 1.763 μs | 0.0075 μs | 0.0070 μs |  1.06 |    0.01 | 0.1373 | 0.0019 |   6.74 KB |        1.06 |
| IncludeUnsupported    | 1.641 μs | 0.0067 μs | 0.0059 μs |  0.98 |    0.01 | 0.1259 |      - |   6.35 KB |        1.00 |
| IncludePrivateMembers | 1.750 μs | 0.0101 μs | 0.0090 μs |  1.05 |    0.01 | 0.1354 | 0.0019 |   6.73 KB |        1.06 |
| IncludeStaticMembers  | 1.755 μs | 0.0105 μs | 0.0093 μs |  1.05 |    0.01 | 0.1354 | 0.0019 |   6.73 KB |        1.06 |
| EvaluateMethods       | 1.803 μs | 0.0306 μs | 0.0286 μs |  1.08 |    0.02 | 0.1259 |      - |    6.3 KB |        0.99 |
| EnableExtensions      | 1.636 μs | 0.0077 μs | 0.0068 μs |  0.98 |    0.01 | 0.1259 |      - |   6.35 KB |        1.00 |
| EnableRedirection     | 1.686 μs | 0.0095 μs | 0.0089 μs |  1.01 |    0.01 | 0.1278 | 0.0019 |   6.35 KB |        1.00 |
| AllEnabled            | 4.603 μs | 0.0146 μs | 0.0129 μs |  2.76 |    0.02 | 0.2747 |      - |  14.74 KB |        2.32 |
