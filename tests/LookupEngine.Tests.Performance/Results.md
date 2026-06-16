## DecomposeBenchmark

Engine version - 2.0.0

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method                |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|-----------------------|---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Default               | 1.620 μs | 0.0099 μs | 0.0093 μs |  1.00 |    0.01 | 0.1278 | 0.0019 |   6.35 KB |        1.00 |
| IncludeRoot           | 1.880 μs | 0.0068 μs | 0.0060 μs |  1.16 |    0.01 | 0.1526 |      - |   7.61 KB |        1.20 |
| IncludeFields         | 1.812 μs | 0.0053 μs | 0.0041 μs |  1.12 |    0.01 | 0.1373 |      - |   6.77 KB |        1.07 |
| IncludeEvents         | 1.766 μs | 0.0047 μs | 0.0036 μs |  1.09 |    0.01 | 0.1373 | 0.0019 |   6.74 KB |        1.06 |
| IncludeUnsupported    | 1.640 μs | 0.0125 μs | 0.0117 μs |  1.01 |    0.01 | 0.1259 |      - |   6.35 KB |        1.00 |
| IncludePrivateMembers | 1.763 μs | 0.0074 μs | 0.0069 μs |  1.09 |    0.01 | 0.1335 |      - |   6.73 KB |        1.06 |
| IncludeStaticMembers  | 1.756 μs | 0.0061 μs | 0.0057 μs |  1.08 |    0.01 | 0.1354 | 0.0019 |   6.73 KB |        1.06 |
| EvaluateMethods       | 1.781 μs | 0.0068 μs | 0.0063 μs |  1.10 |    0.01 | 0.1278 | 0.0019 |    6.3 KB |        0.99 |
| EnableExtensions      | 1.637 μs | 0.0064 μs | 0.0060 μs |  1.01 |    0.01 | 0.1278 | 0.0019 |   6.35 KB |        1.00 |
| EnableRedirection     | 1.651 μs | 0.0110 μs | 0.0103 μs |  1.02 |    0.01 | 0.1278 | 0.0019 |   6.35 KB |        1.00 |
| AllEnabled            | 4.570 μs | 0.0126 μs | 0.0105 μs |  2.82 |    0.02 | 0.3052 |      - |  14.99 KB |        2.36 |

## ExtensionRegistrationBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method                              | Count   |            Mean |          Error |         StdDev |    Ratio |  RatioSD |       Gen0 |       Gen1 |   Allocated | Alloc Ratio |
|-------------------------------------|---------|----------------:|---------------:|---------------:|---------:|---------:|-----------:|-----------:|------------:|------------:|
| **Composer_DirectRegister**         | **1**   |    **20.82 ns** |   **0.186 ns** |   **0.174 ns** | **1.00** | **0.01** | **0.0030** |      **-** |   **152 B** |    **1.00** |
| Struct_DefineRegister               | 1       |        20.29 ns |       0.195 ns |       0.182 ns |     0.98 |     0.01 |     0.0030 |          - |       152 B |        1.00 |
| Class_DefineRegister                | 1       |        23.20 ns |       0.198 ns |       0.175 ns |     1.12 |     0.01 |     0.0038 |          - |       192 B |        1.26 |
| StructCachedDelegate_DefineRegister | 1       |        20.78 ns |       0.281 ns |       0.235 ns |     1.00 |     0.02 |     0.0030 |          - |       152 B |        1.00 |
| StructInterface_DefineRegister      | 1       |        23.63 ns |       0.182 ns |       0.170 ns |     1.14 |     0.01 |     0.0038 |          - |       192 B |        1.26 |
| Composer_NotSupported               | 1       |        16.40 ns |       0.207 ns |       0.194 ns |     0.79 |     0.01 |     0.0024 |          - |       120 B |        0.79 |
| Struct_AsNotSupported               | 1       |        16.21 ns |       0.257 ns |       0.241 ns |     0.78 |     0.01 |     0.0024 |          - |       120 B |        0.79 |
| Composer_MixedScenario              | 1       |        33.67 ns |       0.448 ns |       0.419 ns |     1.62 |     0.03 |     0.0054 |          - |       272 B |        1.79 |
| Struct_MixedScenario                | 1       |        33.91 ns |       0.393 ns |       0.368 ns |     1.63 |     0.02 |     0.0054 |          - |       272 B |        1.79 |
| Struct_WithMap                      | 1       |        20.76 ns |       0.255 ns |       0.238 ns |     1.00 |     0.02 |     0.0030 |          - |       152 B |        1.00 |
| Struct_AsStatic                     | 1       |        20.89 ns |       0.216 ns |       0.202 ns |     1.01 |     0.01 |     0.0030 |          - |       152 B |        1.00 |
|                                     |         |                 |                |                |          |          |            |            |             |             |
| **Composer_DirectRegister**         | **100** | **1,858.32 ns** |  **13.796 ns** |  **12.230 ns** | **1.00** | **0.01** | **0.3014** | **0.0076** | **15200 B** |    **1.00** |
| Struct_DefineRegister               | 100     |     1,880.26 ns |      24.786 ns |      20.697 ns |     1.01 |     0.02 |     0.3014 |     0.0076 |     15200 B |        1.00 |
| Class_DefineRegister                | 100     |     2,227.83 ns |      32.952 ns |      30.824 ns |     1.20 |     0.02 |     0.3815 |     0.0076 |     19200 B |        1.26 |
| StructCachedDelegate_DefineRegister | 100     |     1,855.84 ns |      22.776 ns |      21.304 ns |     1.00 |     0.02 |     0.3014 |     0.0076 |     15200 B |        1.00 |
| StructInterface_DefineRegister      | 100     |     2,232.41 ns |      35.664 ns |      29.781 ns |     1.20 |     0.02 |     0.3815 |     0.0076 |     19200 B |        1.26 |
| Composer_NotSupported               | 100     |     1,380.29 ns |      18.673 ns |      17.466 ns |     0.74 |     0.01 |     0.2384 |     0.0038 |     12000 B |        0.79 |
| Struct_AsNotSupported               | 100     |     1,379.77 ns |      19.666 ns |      18.396 ns |     0.74 |     0.01 |     0.2384 |     0.0038 |     12000 B |        0.79 |
| Composer_MixedScenario              | 100     |     3,396.97 ns |      41.481 ns |      38.801 ns |     1.83 |     0.03 |     0.5417 |     0.0267 |     27200 B |        1.79 |
| Struct_MixedScenario                | 100     |     3,307.76 ns |      36.224 ns |      32.112 ns |     1.78 |     0.03 |     0.5417 |     0.0267 |     27200 B |        1.79 |
| Struct_WithMap                      | 100     |     1,894.26 ns |      37.273 ns |      53.455 ns |     1.02 |     0.03 |     0.3014 |     0.0076 |     15200 B |        1.00 |
| Struct_AsStatic                     | 100     |     1,859.26 ns |      22.824 ns |      19.059 ns |     1.00 |     0.01 |     0.3014 |     0.0076 |     15200 B |        1.00 |
|                                     |         |                 |                |                |          |          |            |            |             |             |
| **Composer_DirectRegister**         | **500** | **9,361.22 ns** | **127.017 ns** | **112.597 ns** | **1.00** | **0.02** | **1.5106** | **0.1831** | **76000 B** |    **1.00** |
| Struct_DefineRegister               | 500     |     9,460.31 ns |     188.680 ns |     209.718 ns |     1.01 |     0.02 |     1.5106 |     0.1831 |     76000 B |        1.00 |
| Class_DefineRegister                | 500     |    10,997.28 ns |     123.419 ns |     109.408 ns |     1.17 |     0.02 |     1.9073 |     0.2289 |     96000 B |        1.26 |
| StructCachedDelegate_DefineRegister | 500     |     9,377.50 ns |     119.938 ns |     106.322 ns |     1.00 |     0.02 |     1.5106 |     0.1831 |     76000 B |        1.00 |
| StructInterface_DefineRegister      | 500     |    11,043.23 ns |     133.813 ns |     125.168 ns |     1.18 |     0.02 |     1.9073 |     0.2289 |     96000 B |        1.26 |
| Composer_NotSupported               | 500     |     6,705.41 ns |     108.464 ns |     101.457 ns |     0.72 |     0.01 |     1.1902 |     0.1221 |     60000 B |        0.79 |
| Struct_AsNotSupported               | 500     |     6,861.38 ns |      43.415 ns |      36.254 ns |     0.73 |     0.01 |     1.1902 |     0.1221 |     60000 B |        0.79 |
| Composer_MixedScenario              | 500     |    16,106.77 ns |     212.258 ns |     198.547 ns |     1.72 |     0.03 |     2.6855 |     0.3662 |    136000 B |        1.79 |
| Struct_MixedScenario                | 500     |    16,195.22 ns |     208.447 ns |     194.981 ns |     1.73 |     0.03 |     2.6855 |     0.3662 |    136000 B |        1.79 |
| Struct_WithMap                      | 500     |     9,474.01 ns |      69.208 ns |      64.737 ns |     1.01 |     0.01 |     1.5106 |     0.1984 |     76000 B |        1.00 |
| Struct_AsStatic                     | 500     |     9,457.25 ns |      96.908 ns |      90.648 ns |     1.01 |     0.01 |     1.5106 |     0.1831 |     76000 B |        1.00 |

## FormatMemberNameBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method                |     Mean |   Error |  StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|-----------------------|---------:|--------:|--------:|------:|-------:|----------:|------------:|
| LinqSelectJoin        | 140.0 ns | 1.05 ns | 0.93 ns |  1.00 | 0.0110 |     552 B |        1.00 |
| StringBuilderAppend   | 148.3 ns | 1.96 ns | 1.84 ns |  1.06 | 0.0138 |     696 B |        1.26 |
| StringBuilderSpanTrim | 130.4 ns | 0.97 ns | 0.91 ns |  0.93 | 0.0129 |     656 B |        1.19 |

## FormatTypeNameBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method                  | Type                                            |            Mean |         Error |        StdDev |    Ratio |  RatioSD |       Gen0 | Allocated | Alloc Ratio |
|-------------------------|-------------------------------------------------|----------------:|--------------:|--------------:|---------:|---------:|-----------:|----------:|------------:|
| **StringConcatenation** | **Dictionary&lt;String, List&lt;Int32&gt;&gt;** | **151.2045 ns** | **1.7855 ns** | **1.6702 ns** | **1.00** | **0.02** | **0.0124** | **624 B** |    **1.00** |
| StringBuilderRecursive  | Dictionary&lt;String, List&lt;Int32&gt;&gt;     |     131.4423 ns |     0.8563 ns |     0.7591 ns |     0.87 |     0.01 |     0.0072 |     368 B |        0.59 |
| SpanWithStringBuilder   | Dictionary&lt;String, List&lt;Int32&gt;&gt;     |     147.8973 ns |     1.2178 ns |     1.1391 ns |     0.98 |     0.01 |     0.0103 |     520 B |        0.83 |
|                         |                                                 |                 |               |               |          |          |            |           |             |
| **StringConcatenation** | **List&lt;Int32&gt;**                           |  **63.3988 ns** | **0.6336 ns** | **0.5927 ns** | **1.00** | **0.01** | **0.0038** | **192 B** |    **1.00** |
| StringBuilderRecursive  | List&lt;Int32&gt;                               |      61.5743 ns |     0.4973 ns |     0.4652 ns |     0.97 |     0.01 |     0.0036 |     184 B |        0.96 |
| SpanWithStringBuilder   | List&lt;Int32&gt;                               |      61.9677 ns |     0.4687 ns |     0.4384 ns |     0.98 |     0.01 |     0.0036 |     184 B |        0.96 |
|                         |                                                 |                 |               |               |          |          |            |           |             |
| **StringConcatenation** | **String**                                      |   **0.7939 ns** | **0.0392 ns** | **0.0347 ns** | **1.00** | **0.06** |      **-** |     **-** |      **NA** |
| StringBuilderRecursive  | String                                          |      13.8071 ns |     0.2443 ns |     0.2285 ns |    17.42 |     0.78 |     0.0029 |     144 B |          NA |
| SpanWithStringBuilder   | String                                          |       0.8875 ns |     0.0190 ns |     0.0168 ns |     1.12 |     0.05 |          - |         - |          NA |

## MemberEnumerationBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method                      |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-----------------------------|-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| PropertiesPublicOnly        |  47.833 ns | 0.5224 ns | 0.4631 ns |  1.00 |    0.01 | 0.0052 |     264 B |        1.00 |
| PropertiesPublicAndPrivate  |  49.022 ns | 0.4913 ns | 0.4596 ns |  1.02 |    0.01 | 0.0052 |     264 B |        1.00 |
| FieldsPublicOnly            |   7.245 ns | 0.0348 ns | 0.0325 ns |  0.15 |    0.00 |      - |         - |        0.00 |
| FieldsPublicAndPrivate      |  42.401 ns | 0.3032 ns | 0.2836 ns |  0.89 |    0.01 | 0.0048 |     240 B |        0.91 |
| MethodsPublicOnly           | 215.720 ns | 2.2602 ns | 2.0036 ns |  4.51 |    0.06 | 0.0288 |    1456 B |        5.52 |
| MethodsPublicAndPrivate     | 269.789 ns | 3.2646 ns | 3.0537 ns |  5.64 |    0.08 | 0.0315 |    1600 B |        6.06 |
| PropertiesFilterSpecialName |  47.868 ns | 0.3627 ns | 0.3393 ns |  1.00 |    0.01 | 0.0052 |     264 B |        1.00 |
| MethodsFilterSpecialName    | 177.102 ns | 2.7837 ns | 2.6039 ns |  3.70 |    0.06 | 0.0288 |    1456 B |        5.52 |
| ListWithCapacity32          |  76.724 ns | 0.7237 ns | 0.6415 ns |  1.60 |    0.02 | 0.0114 |     576 B |        2.18 |
| ListWithDefaultCapacity     | 100.465 ns | 1.8516 ns | 1.6414 ns |  2.10 |    0.04 | 0.0117 |     592 B |        2.24 |

## TypeEqualBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method                              | Type           |         Mean |         Error |        StdDev |    Ratio |  RatioSD | Allocated | Alloc Ratio |
|-------------------------------------|----------------|-------------:|--------------:|--------------:|---------:|---------:|----------:|------------:|
| **PatternMatching**                 | **?**          | **2.466 ns** | **0.0792 ns** | **0.0813 ns** | **1.80** | **0.09** |     **-** |      **NA** |
| PatternMatchingWithTypeEquality     | ?              |     1.373 ns |     0.0589 ns |     0.0578 ns |     1.00 |     0.06 |         - |          NA |
| PatternMatchingWithFullNameEquality | ?              |     1.756 ns |     0.0111 ns |     0.0098 ns |     1.28 |     0.05 |         - |          NA |
|                                     |                |              |               |               |          |          |           |             |
| **PatternMatching**                 | **ButtonBase** | **2.399 ns** | **0.0501 ns** | **0.0418 ns** | **1.26** | **0.02** |     **-** |      **NA** |
| PatternMatchingWithTypeEquality     | ButtonBase     |     1.905 ns |     0.0106 ns |     0.0099 ns |     1.00 |     0.01 |         - |          NA |
| PatternMatchingWithFullNameEquality | ButtonBase     |     3.966 ns |     0.0304 ns |     0.0284 ns |     2.08 |     0.02 |         - |          NA |

## TypeHierarchyBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method                       | Type                  |         Mean |        Error |       StdDev |    Ratio |  RatioSD |       Gen0 | Allocated | Alloc Ratio |
|------------------------------|-----------------------|-------------:|-------------:|-------------:|---------:|---------:|-----------:|----------:|------------:|
| **ListWithDynamicGrowth**    | **ArgumentException** | **36.67 ns** | **0.373 ns** | **0.349 ns** | **1.00** | **0.01** | **0.0017** |  **88 B** |    **1.00** |
| StackBasedReversal           | ArgumentException     |     58.55 ns |     0.484 ns |     0.453 ns |     1.60 |     0.02 |     0.0035 |     176 B |        2.00 |
| ListWithPreallocatedCapacity | ArgumentException     |     55.08 ns |     0.227 ns |     0.212 ns |     1.50 |     0.01 |     0.0017 |      88 B |        1.00 |
|                              |                       |              |              |              |          |          |            |           |             |
| **ListWithDynamicGrowth**    | **List&lt;Int32&gt;** | **21.49 ns** | **0.252 ns** | **0.235 ns** | **1.00** | **0.02** | **0.0017** |  **88 B** |    **1.00** |
| StackBasedReversal           | List&lt;Int32&gt;     |     37.67 ns |     0.621 ns |     0.581 ns |     1.75 |     0.03 |     0.0035 |     176 B |        2.00 |
| ListWithPreallocatedCapacity | List&lt;Int32&gt;     |     27.61 ns |     0.102 ns |     0.090 ns |     1.28 |     0.01 |     0.0014 |      72 B |        0.82 |
|                              |                       |              |              |              |          |          |            |           |             |
| **ListWithDynamicGrowth**    | **String**            | **22.21 ns** | **0.396 ns** | **0.371 ns** | **1.00** | **0.02** | **0.0017** |  **88 B** |    **1.00** |
| StackBasedReversal           | String                |     43.28 ns |     0.167 ns |     0.140 ns |     1.95 |     0.03 |     0.0035 |     176 B |        2.00 |
| ListWithPreallocatedCapacity | String                |     27.01 ns |     0.291 ns |     0.272 ns |     1.22 |     0.02 |     0.0014 |      72 B |        0.82 |

## WildcardMatchArrayBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method         | InputIndex |          Mean |         Error |        StdDev |    Ratio |  RatioSD | Allocated | Alloc Ratio |
|----------------|------------|--------------:|--------------:|--------------:|---------:|---------:|----------:|------------:|
| **TwoPointer** | **0**      | **35.146 ns** | **0.7208 ns** | **1.7269 ns** | **1.58** | **0.08** |     **-** |      **NA** |
| SegmentSplit   | 0          |     29.869 ns |     0.1757 ns |     0.1643 ns |     1.34 |     0.01 |         - |          NA |
| Precompiled    | 0          |     22.239 ns |     0.1307 ns |     0.1222 ns |     1.00 |     0.01 |         - |          NA |
|                |            |               |               |               |          |          |           |             |
| **TwoPointer** | **1**      |  **9.247 ns** | **0.1991 ns** | **0.3883 ns** | **4.50** | **0.19** |     **-** |      **NA** |
| SegmentSplit   | 1          |      5.697 ns |     0.0210 ns |     0.0186 ns |     2.77 |     0.01 |         - |          NA |
| Precompiled    | 1          |      2.055 ns |     0.0085 ns |     0.0079 ns |     1.00 |     0.01 |         - |          NA |
|                |            |               |               |               |          |          |           |             |
| **TwoPointer** | **2**      | **15.355 ns** | **0.2141 ns** | **0.2002 ns** | **0.67** | **0.01** |     **-** |      **NA** |
| SegmentSplit   | 2          |     27.639 ns |     0.1405 ns |     0.1314 ns |     1.21 |     0.01 |         - |          NA |
| Precompiled    | 2          |     22.880 ns |     0.0484 ns |     0.0429 ns |     1.00 |     0.00 |         - |          NA |

## WildcardMatchBenchmark

```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 9950X3D 0.62GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v4


```

| Method         | PatternIndex |           Mean |         Error |        StdDev |    Ratio |  RatioSD | Allocated | Alloc Ratio |
|----------------|--------------|---------------:|--------------:|--------------:|---------:|---------:|----------:|------------:|
| **TwoPointer** | **0**        |  **7.1264 ns** | **0.1512 ns** | **0.2264 ns** | **4.01** | **0.13** |     **-** |      **NA** |
| SegmentSplit   | 0            |      6.5015 ns |     0.1473 ns |     0.2249 ns |     3.66 |     0.13 |         - |          NA |
| Precompiled    | 0            |      1.7781 ns |     0.0116 ns |     0.0103 ns |     1.00 |     0.01 |         - |          NA |
|                |              |                |               |               |          |          |           |             |
| **TwoPointer** | **1**        |  **8.3419 ns** | **0.1549 ns** | **0.1373 ns** | **4.85** | **0.08** |     **-** |      **NA** |
| SegmentSplit   | 1            |      6.4500 ns |     0.1422 ns |     0.1637 ns |     3.75 |     0.09 |         - |          NA |
| Precompiled    | 1            |      1.7188 ns |     0.0058 ns |     0.0052 ns |     1.00 |     0.00 |         - |          NA |
|                |              |                |               |               |          |          |           |             |
| **TwoPointer** | **2**        | **12.5441 ns** | **0.2681 ns** | **0.4094 ns** | **1.77** | **0.08** |     **-** |      **NA** |
| SegmentSplit   | 2            |      6.1868 ns |     0.0262 ns |     0.0245 ns |     0.87 |     0.03 |         - |          NA |
| Precompiled    | 2            |      7.1006 ns |     0.1473 ns |     0.2421 ns |     1.00 |     0.05 |         - |          NA |
|                |              |                |               |               |          |          |           |             |
| **TwoPointer** | **3**        | **10.7294 ns** | **0.2073 ns** | **0.2696 ns** | **0.85** | **0.02** |     **-** |      **NA** |
| SegmentSplit   | 3            |     13.0850 ns |     0.0759 ns |     0.0710 ns |     1.04 |     0.01 |         - |          NA |
| Precompiled    | 3            |     12.6220 ns |     0.0959 ns |     0.0897 ns |     1.00 |     0.01 |         - |          NA |
|                |              |                |               |               |          |          |           |             |
| **TwoPointer** | **4**        | **10.9239 ns** | **0.2180 ns** | **0.2040 ns** | **1.68** | **0.03** |     **-** |      **NA** |
| SegmentSplit   | 4            |      6.2897 ns |     0.0264 ns |     0.0247 ns |     0.97 |     0.00 |         - |          NA |
| Precompiled    | 4            |      6.5080 ns |     0.0163 ns |     0.0144 ns |     1.00 |     0.00 |         - |          NA |
|                |              |                |               |               |          |          |           |             |
| **TwoPointer** | **5**        |  **0.3922 ns** | **0.0056 ns** | **0.0052 ns** | **1.49** | **0.03** |     **-** |      **NA** |
| SegmentSplit   | 5            |      3.2630 ns |     0.0189 ns |     0.0176 ns |    12.43 |     0.23 |         - |          NA |
| Precompiled    | 5            |      0.2625 ns |     0.0051 ns |     0.0048 ns |     1.00 |     0.03 |         - |          NA |
|                |              |                |               |               |          |          |           |             |
| **TwoPointer** | **6**        |  **7.9654 ns** | **0.0902 ns** | **0.0800 ns** | **3.91** | **0.04** |     **-** |      **NA** |
| SegmentSplit   | 6            |      1.8724 ns |     0.0105 ns |     0.0098 ns |     0.92 |     0.01 |         - |          NA |
| Precompiled    | 6            |      2.0385 ns |     0.0068 ns |     0.0064 ns |     1.00 |     0.00 |         - |          NA |
