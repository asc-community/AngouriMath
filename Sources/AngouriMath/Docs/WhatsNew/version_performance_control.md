We compare performances through different versions with CommonFunctionsInterVersionTest.cs in Tests.

Units: nanoseconds

Each column is headed by how far along the history of `master` the version measured is,
counting the first commit as the 1st, and links to that commit. To find the number for a
new column, count the commits up to the one being measured:

```
git rev-list --count <commit>
```

**Columns are not measured on one machine, so read a row across it as a trend rather than
as a ratio.** The 1620th column was taken on a GitHub-hosted `ubuntu-latest` runner by the
Kernel Benchmark workflow; the earlier ones were taken on contributors' own machines. A
change of a few per cent between neighbouring columns says nothing at all. The order-of-
magnitude moves are what this file is for.

The 1620th column is also the first since the 1446th, because the benchmark workflow filtered
on a path that does not exist and had not run since 2026-01-02 -- ninety-eight merged pull
requests went unmeasured. Fixed in
[#775](https://github.com/asc-community/AngouriMath/issues/775).

**Allocation is reported alongside the timings** from the 1671st column on. The
benchmark class has carried `[MemoryDiagnoser]` all along, so the figure was measured on every
run and then discarded, because the reporting code asked for `Mean`, `Error` and `StdDev` and
nothing else. Asked for by [#167](https://github.com/asc-community/AngouriMath/issues/167).

**`EvalTrig` and `EvalTrigPrecise`** are new in the same column and are the other half of
[#167](https://github.com/asc-community/AngouriMath/issues/167): what arbitrary precision costs
in trigonometry. Both evaluate `sin(1) + cos(1) + tan(1)`, the first at the default hundred
digits and the second at five hundred. Measured while writing them, on one machine, so treat
these as the shape rather than as the numbers:

| digits | per evaluation |
|---|---|
| 100 (default) | 3.8 ms |
| 200 | 10.2 ms |
| 500 | 25.4 ms |
| 1000 | 146 ms |

**Read `EvalEasy` with care.** It evaluates one `Entity` instance, and an `Entity` caches its
own `Evaled`, so from the moment that cache landed the row stopped measuring evaluation and
started measuring a lookup -- which is what the fall to single-digit nanoseconds is, not a
speed-up of the arithmetic. Measured directly: a hundred thousand evaluations of one instance
take a millisecond in total, while a thousand evaluations of freshly built nodes take a second.
The row is kept as it is so the column history stays comparable, and the two trigonometric
benchmarks build their expression afresh on every call for exactly this reason.

|          Method |         [331st](https://github.com/asc-community/AngouriMath/commit/10e6e5a90e7270336b68dc5fd6aa36f3e0e65d2b) |          [380th](https://github.com/asc-community/AngouriMath/commit/73ae36488ddb863c1d6f35db5ed2f5dcf1484a26) |     [391st](https://github.com/asc-community/AngouriMath/commit/c7e08e6936bfdc2373377bec81ffd160e406244f)      |         [410th](https://github.com/asc-community/AngouriMath/commit/20814936bc740a9f410af4a4368e9895eab7aaf7) |           [483rd](https://github.com/asc-community/AngouriMath/commit/355963dcdf0ff9da568e9f1144ad2b7b68c19584) |         [520th](https://github.com/asc-community/AngouriMath/commit/70aa71acb73307c9f7df0aac006faae31b06058c) |         [690th](https://github.com/asc-community/AngouriMath/commit/5cc894939cb3657f0aa7ef5a25fd55011058929f) |         [826th](https://github.com/asc-community/AngouriMath/commit/87e33ec3590a95dd4ec59ff5c1f77064a64196d1) |         [914th](https://github.com/asc-community/AngouriMath/commit/6134338df083a908369b6bcfb69e70a4269ec51b) |         [920th](https://github.com/asc-community/AngouriMath/commit/501a0a3a9b2e07cddf92c4446b73ad6e2748253a) |        [1034th](https://github.com/asc-community/AngouriMath/commit/a3f48b47795b2dc2b3435152989a6e15639a65b4) |        [1066th](https://github.com/asc-community/AngouriMath/commit/a33746651f56a380b6c17913aa844f162f258d8c) |        [1090th](https://github.com/asc-community/AngouriMath/commit/9530c5b04484e98023941f7693bbeb1a3282cee6) |           [1446th](https://github.com/asc-community/AngouriMath/commit/2abb2b537c03977281f3fc2cab1da2c78c36a5f5) | [1620th](https://github.com/asc-community/AngouriMath/commit/e05d71797f53a9ac7dbdad6075cd7302a91036e7) | [1671st](https://github.com/asc-community/AngouriMath/commit/253b5a8d598a9a1d721b777340bd53ae3be12f99) |
|---------------- |--------------:|---------------:|---------------:|--------------:|----------------:|--------------:|--------------:|--------------:|--------------:|--------------:|--------------:|--------------:|--------------:|-----------------:|--------------:|--------------:|
|       ParseEasy |        28,599 |         73,669 |        134,120 |        44,328 |          54,675 |        21,722 |        32,212 |        32,138 |        34,702 |        32,199 |        33,008 |        27,483 |        33,043 |        34,664 | 11,760 ns | 10,857 ns |
|       ParseHard |        92,037 |        208,710 |        415,440 |       178,760 |         287,865 |       209,853 |       624,769 |       698,094 |     4,272,898 |     3,862,375 |     4,731,275 |     4,051,823 |     5,284,613 |     5,657,058 | 2,617,362 ns | 2,493,337 ns |
|    SimplifyEasy |     6,225,707 |     16,387,374 |     41,081,822 |       397,973 |       2,594,367 |       168,462 |       225,393 |       281,626 |       122,809 |        79,184 |        94,505 |        86,478 |       102,600 |        87,759 | 153,704 ns | 152,631 ns |
|    SimplifyHard | 3,245,613,128 |  8,120,319,273 |  9,715,629,251 | 7,477,089,153 |  13,529,147,530 | 3,015,302,746 | 3,162,675,550 | 3,826,831,860 | 4,024,002,060 | 2,633,113,653 | 3,262,112,233 | 4,540,783,876 | 4,639,170,780 | 4,880,264,786 | 2,848,037,261 ns | 2,916,127,943 ns |
|        Derivate |         5,550 |         39,895 |        161,507 |        50,858 |          56,255 |        37,702 |        46,083 |        86,751 |        68,604 |        41,388 |        58,340 |        48,376 |        54,228 |        60,425 | 26,808 ns | 25,629 ns |
|       SolveEasy |       677,259 |      4,066,049 |     33,760,616 |     7,556,902 |     124,888,489 |    38,935,084 |    42,101,926 |    43,102,578 |    43,778,486 |    42,818,770 |    42,230,213 |    35,415,096 |    45,032,006 |    45,587,157 | 21,829,186 ns | 21,322,368 ns |
| SolveEasyMedium |       192,319 |        741,114 |      2,861,751 |       646,063 |         789,993 |       124,082 |       154,729 |       142,744 |       138,836 |        78,712 |       110,144 |        88,021 |       106,430 |       108,175 | 48,368 ns | 48,863 ns |
|     SolveMedium |    22,487,990 |    126,650,858 |    690,232,457 |   178,691,349 |     350,752,882 |     2,961,363 |     2,174,299 |     2,108,050 |     2,248,458 |     2,449,733 |     2,996,001 |     2,661,744 |     3,272,498 |     4,409,082 | 783,737 ns | 796,814 ns |
| SolveMediumHard |       > 1 h   |       > 1 h    | 12,033,099,074 | 2,754,157,261 |   5,779,298,835 |   240,930,082 |   258,560,720 |   309,944,856 |   346,532,318 |   258,799,676 |   305,404,083 |   354,286,153 |   409,437,694 |   453,280,166 | 124,316,631 ns | 129,750,583 ns |
|       SolveHard |       > 1 h   |       > 1 h    | 21,565,808,100 | 5,636,112,783 | 100,663,275,757 | 1,862,768,773 | 2,532,972,635 | 3,000,655,673 | 3,606,412,386 | 2,425,286,433 | 2,918,112,980 | 3,162,162,893 | 3,353,403,193 | 3,494,150,836 | 1,134,532,585 ns | 1,365,230,469 ns |
|        EvalEasy |        12,059 |        200,766 |      1,548,901 |       550,676 |       3,354,484 |            34 |            72 |            68 |            84 |            28 |            11 |            11 |            13 |             9 | 2 ns | 2 ns |
|        EvalTrig |             -- |              -- |              -- |            -- |               -- |            -- |            -- |            -- |            -- |            -- |            -- |            -- |            -- |               -- | 1,344,115 ns |
| EvalTrigPrecise |             -- |              -- |              -- |            -- |               -- |            -- |            -- |            -- |            -- |            -- |            -- |            -- |            -- |               -- | 43,285,515 ns |
|     CompileEasy |        24,771 |         38,300 |         62,328 |        33,684 |          40,751 |         9,208 |        15,467 |         5,487 |         5,623 |         6,283 |         6,254 |         5,143 |       565,597 |       589,540 | 352,800 ns | 352,449 ns |
|     CompileHard |        61,572 |         84,042 |        139,499 |        73,738 |          96,203 |        18,274 |        26,390 |        14,782 |        15,657 |        17,986 |        17,672 |        14,902 |     1,234,995 |     1,321,375 | 595,593 ns | 596,664 ns |
|         RunEasy |           138 |            126 |            123 |           117 |             128 |           157 |           170 |           165 |           175 |           167 |           161 |           149 |            48 |            43 | 42 ns | 42 ns |
|       RunMedium |           833 |            950 |            920 |           844 |             838 |         1,008 |         1,081 |         1,064 |         1,155 |         1,089 |           982 |           875 |           630 |           549 | 318 ns | 320 ns |
|         RunHard |         1,403 |          1,644 |          1,558 |         1,427 |           1,391 |         1,974 |         2,102 |         2,037 |         2,046 |         2,044 |         1,855 |         1,655 |         1,041 |           928 | 586 ns | 582 ns |
																													   


## Allocation, from the 1671st

Bytes allocated per operation, measured in the same run as the 1671st column above. There is
one column because this is the first version measured for it; later runs add to it the way the
timings do.

| Method | [1671st](https://github.com/asc-community/AngouriMath/commit/253b5a8d598a9a1d721b777340bd53ae3be12f99) |
|---|--:|
| ParseEasy | 18,052 B |
| ParseHard | 3,486,004 B |
| SimplifyEasy | 127,578 B |
| SimplifyHard | 3,710,538,008 B |
| Derivate | 52,821 B |
| SolveEasy | 20,213,448 B |
| SolveEasyMedium | 79,613 B |
| SolveMedium | 553,368 B |
| SolveMediumHard | 152,243,208 B |
| SolveHard | 1,341,716,112 B |
| EvalEasy | none |
| EvalTrig | 1,341,378 B |
| EvalTrigPrecise | 12,743,208 B |
| CompileEasy | 16,200 B |
| CompileHard | 37,944 B |
| RunEasy | none |
| RunMedium | none |
| RunHard | none |

**`EvalEasy` allocating nothing is the proof of the caching note above**, rather than a further
argument for it. Evaluating `1 + 2 + log(2, 3) + sqrt(4) - 4 ^ 7 + e * pi` cannot allocate zero
bytes; a dictionary lookup can. `RunEasy`, `RunMedium` and `RunHard` allocate nothing for the
honest reason -- a compiled delegate over `Complex` has nothing to put on the heap.
