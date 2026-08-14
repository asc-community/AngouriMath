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

---

## The 1671st and the 1697th, measured together on one machine

**Why a pair rather than a column.** 2.0.0 shipped without any performance measurement at all, so
there is no column for it: it is commit 1691 and the table above stops at the 1671st. Filling that gap
with a single new column would have been worse than leaving it. Measured on a developer machine, every
one of the sixteen rows came out at 0.5–0.6× of the 1671st — a uniform factor across the board, which
is a faster machine and not faster code. A reader comparing the two columns would have seen an
across-the-board 1.75× improvement that does not exist, which is exactly the mistake the note at the
top of this file warns about.

So the 1671st was re-measured on the same machine, minutes apart, with nothing else running. Only the
right-hand pair below may be read as a ratio; neither may be compared with the columns above.

The 1697th is `6476513b`, which is 2.0.0 plus the five corrections merged after it and the `abs`/`sgn`
fix — so this answers "did the release and the fixes after it cost anything", which is the question
2.0.1 needs answered.

| Method | [1671st](https://github.com/asc-community/AngouriMath/commit/253b5a8d598a9a1d721b777340bd53ae3be12f99) | [1697th](https://github.com/asc-community/AngouriMath/commit/6476513b) | change |
|---|--:|--:|--:|
| ParseEasy | 5,887 ns | 6,052 ns | +2.8% |
| ParseHard | 1,364,570 ns | 1,430,477 ns | +4.8% |
| SimplifyEasy | 79,832 ns | 86,744 ns | **+8.7%** |
| SimplifyHard | 1,646,740,009 ns | 1,697,337,871 ns | +3.1% |
| Derivate | 13,745 ns | 14,367 ns | +4.5% |
| SolveEasy | 11,302,282 ns | 11,883,543 ns | +5.1% |
| SolveEasyMedium | 24,545 ns | 24,438 ns | −0.4% |
| SolveMedium | 420,926 ns | 433,918 ns | +3.1% |
| SolveMediumHard | 74,401,542 ns | 76,053,093 ns | +2.2% |
| SolveHard | 751,607,244 ns | 766,020,069 ns | +1.9% |
| EvalEasy | 1.34 ns | 0.85 ns | see below |
| EvalTrig | 720,189 ns | 711,009 ns | −1.3% |
| EvalTrigPrecise | 22,870,710 ns | 23,086,271 ns | +0.9% |
| CompileEasy | 191,188 ns | 191,289 ns | +0.1% |
| CompileHard | 320,897 ns | 323,255 ns | +0.7% |
| RunEasy | 20.12 ns | 20.24 ns | +0.6% |
| RunMedium | 163.8 ns | 169.9 ns | +3.7% |
| RunHard | 290.6 ns | 296.9 ns | +2.2% |

Allocation over the same pair: `SimplifyHard` 3,710,538,664 → 3,737,029,480 B (+0.7%), `SolveHard`
1,341,724,616 → 1,356,514,512 B (+1.1%), `SolveMediumHard` 152,244,664 → 155,187,208 B (+1.9%);
everything else within a few tenths of a per cent.

**Read: no regression, with one row worth watching.** Most of the table moves 1–5%, which by this
file's own standard says nothing. `SimplifyEasy` at **+8.7%** is above that, and there is a plausible
cause rather than a mystery: the corrections merged in that range added guards to the simplification
patterns — `WithinHalfPi`, `WithinArccotanRange`, and the `abs`/`sgn` zero test — and each calls
`Evaled` inside a pattern's `when` clause on a path that previously matched structurally. `Evaled` is
cached per node, so the cost is a first-call computation and a lookup thereafter, but it is not free
and it is on the hot path. Worth a measurement of its own before more guards are written that way.

`EvalEasy` is not a real movement in either direction. It measures a cached lookup rather than
arithmetic — see the note above the main table — and at one nanosecond the figure is dominated by
whatever the harness itself costs.

## The 1691st and the 1707th, measured together on one machine — the pair for 2.1.0

`v2.0.0` itself against the 2.1.0 candidate: the 1691st is
[`f2594259`](https://github.com/asc-community/AngouriMath/commit/f2594259), tagged `v2.0.0`, and the
1707th is [`9446c762`](https://github.com/asc-community/AngouriMath/commit/9446c762), sixteen commits
later. Both measured minutes apart in one session on one machine, so **only these two columns may be
read against each other**; neither may be compared with anything above.

This is the question a release needs answered — what the version users have costs against the version
they are being offered — which the pair above could not answer, since it took the 1697th rather than
the tag.

| Method | [1691st](https://github.com/asc-community/AngouriMath/commit/f2594259) | [1707th](https://github.com/asc-community/AngouriMath/commit/9446c762) | change |
|---|--:|--:|--:|
| ParseEasy | 5,920 ns | 5,703 ns | −3.7% |
| ParseHard | 1,343,362 ns | 1,339,656 ns | −0.3% |
| SimplifyEasy | 81,620 ns | 83,340 ns | +2.1% |
| SimplifyHard | 1,574,777,032 ns | 1,614,280,250 ns | +2.5% |
| Derivate | 13,510 ns | 13,741 ns | +1.7% |
| SolveEasy | 11,139,199 ns | 11,383,594 ns | +2.2% |
| SolveEasyMedium | 23,437 ns | 23,733 ns | +1.3% |
| SolveMedium | 397,908 ns | 416,180 ns | +4.6% |
| SolveMediumHard | 71,646,943 ns | 71,756,037 ns | +0.2% |
| SolveHard | 770,417,841 ns | 761,848,527 ns | −1.1% |
| EvalEasy | 1.211 ns | 1.336 ns | +10.3% |
| EvalTrig | 701,704 ns | 703,612 ns | +0.3% |
| EvalTrigPrecise | 22,673,032 ns | 22,585,377 ns | −0.4% |
| CompileEasy | 186,889 ns | 187,557 ns | +0.4% |
| CompileHard | 319,797 ns | 319,365 ns | −0.1% |
| RunEasy | 20.36 ns | 19.99 ns | −1.8% |
| RunMedium | 163.9 ns | 168.6 ns | +2.9% |
| RunHard | 293.0 ns | 294.6 ns | +0.5% |

Allocation over the same pair: `SimplifyEasy` 127,964 → 128,564 B (+0.5%), `CompileEasy`
16,207 → 16,408 B (+1.2%), `ParseHard` 3,486,001 → 3,498,137 B (+0.3%), `SimplifyHard`
3,737,107,808 → 3,733,175,840 B (−0.1%). `Derivate`, `EvalTrig` and `EvalTrigPrecise` are
**byte-identical**, and the remaining rows move by tens of bytes on totals in the millions.

**Read: no regression.** The largest real move is `SolveMedium` at +4.6%, which by the standard at the
top of this file says nothing on its own; the rest sits inside ±3%. `EvalEasy` at +10.3% is 0.125 ns and
is the cached-lookup row again.

### It also corrects the section above

That pair reported `SimplifyEasy` at **+8.7%**, attributed it to guards added in that range calling
`Evaled` inside a pattern's `when` clause, and asked for "a measurement of its own before more guards
are written that way". Since then roughly six more guards of exactly that shape were added — the
interval tests of [#887](https://github.com/asc-community/AngouriMath/issues/887), the zero test of
[#892](https://github.com/asc-community/AngouriMath/issues/892), the sign read of
[#881](https://github.com/asc-community/AngouriMath/issues/881), the positivity check of
[#902](https://github.com/asc-community/AngouriMath/issues/902) and the sort guard of
[#897](https://github.com/asc-community/AngouriMath/issues/897).

`SimplifyEasy` is **+2.1%** here, and its allocation moves 0.5%. If the guards were the cause, that row
should have moved further and allocation should have followed, since `Evaled` allocates on its first
call per node. Neither happened. **So the +8.7% was mostly the machine rather than the guards, and the
attribution in the section above was wrong** — which is the same error that section itself was written
to catch, made one level down: a single number read as a cause without a second measurement to hold it
against. The action item it raised is discharged, and the answer is that this pattern is not
measurably expensive at this scale.

## The 1709th and the 1724th, measured together on one machine — the pair for 2.2.0

`v2.1.0` against the 2.2.0 candidate: the 1709th is
[`b8cf4dcd`](https://github.com/asc-community/AngouriMath/commit/b8cf4dcd), tagged `v2.1.0`, and the
1724th is [`03a0bf64`](https://github.com/asc-community/AngouriMath/commit/03a0bf64), fifteen commits
later. Measured minutes apart in one session on one machine, so **only these two columns may be read
against each other**; neither may be compared with anything above.

| Method | [1709th](https://github.com/asc-community/AngouriMath/commit/b8cf4dcd) | [1724th](https://github.com/asc-community/AngouriMath/commit/03a0bf64) | change | allocated, 1709th | allocated, 1724th |
|---|--:|--:|--:|--:|--:|
| ParseEasy | 5,746 ns | 5,747 ns | +0.0% | 18,061 B | 18,061 B |
| ParseHard | 1,325,558 ns | 1,369,170 ns | +3.3% | 3,498,137 B | 3,498,137 B |
| SimplifyEasy | 82,305 ns | 81,872 ns | −0.5% | 128,564 B | 128,564 B |
| SimplifyHard | 1,590.6 ms | 1,581.7 ms | −0.6% | 3,733,136,832 B | 3,749,448,792 B |
| Derivate | 13,461 ns | 13,565 ns | +0.8% | 52,811 B | 52,823 B |
| SolveEasy | 11,208,548 ns | 11,177,085 ns | −0.3% | 20,220,106 B | 20,234,998 B |
| **SolveEasyMedium** | 23,509 ns | 28,503 ns | **+21.2%** | 80,184 B | **95,873 B** |
| **SolveMedium** | 411,557 ns | 461,734 ns | **+12.2%** | 554,368 B | **658,259 B** |
| **SolveMediumHard** | 74.9 ms | 85.4 ms | **+14.0%** | 155,190,904 B | **171,395,096 B** |
| **SolveHard** | 800.2 ms | 858.0 ms | **+7.2%** | 1,356,524,520 B | **1,486,581,640 B** |
| EvalEasy | 1.334 ns | 1.332 ns | −0.1% | — | — |
| EvalTrig | 706,201 ns | 700,257 ns | −0.8% | 1,341,377 B | 1,341,377 B |
| EvalTrigPrecise | 22,548,629 ns | 22,583,915 ns | +0.2% | 12,742,205 B | 12,742,205 B |
| CompileEasy | 186,227 ns | 188,554 ns | +1.2% | 16,191 B | 16,309 B |
| CompileHard | 321,647 ns | 319,845 ns | −0.6% | 38,377 B | 37,755 B |
| RunEasy | 20.117 ns | 20.047 ns | −0.3% | — | — |
| RunMedium | 165.849 ns | 163.145 ns | −1.6% | — | — |
| RunHard | 293.908 ns | 291.365 ns | −0.9% | — | — |

### Four solver rows moved, and the allocation moved with them

Everything that is not the solver is flat — parse, simplify, evaluate and the compiled-call trio all
within 2%, with allocation byte-identical on most rows. Four `Solve*` rows are 7% to 21% slower and
allocate 10% to 20% more.

**Allocation moving with the timing is what makes this a real change rather than the machine.** The
pair above this one is the cautionary case: a row reported +8.7% with allocation flat, and
re-measuring put it at +2.1%. Here four related rows move together in both, which noise does not do.

### It is [#918](https://github.com/asc-community/AngouriMath/pull/918), and it is a price rather than a regression

Isolated by measuring the commits either side of it. Allocation steps exactly once, at the polynomial
layer, and is identical before and after within each half:

| SolveMedium, allocated | |
|---|--:|
| the 1709th, `v2.1.0` | 554,368 B |
| [`51194ce8`](https://github.com/asc-community/AngouriMath/commit/51194ce8), before #918 | 554,368 B |
| [`69f66da7`](https://github.com/asc-community/AngouriMath/commit/69f66da7), after #918 | 658,259 B |
| the 1724th | 658,259 B |

The same clean step appears in all four rows. `SolveEasy` — a quadratic, which never reaches the
factorisation path — is flat throughout, which is the mechanism corroborating itself.

#918 made the equation solver the polynomial layer's first consumer: where a polynomial factors over
`Q`, each factor is now solved as a lower-degree equation of its own. That is what turned
`x^5 + 2x^3 - 2x^2 - 4` from three roots, one of them a float, into all five, exact. An incomplete
solution set is not a partial answer but a false one, so **this cost buys a wrong answer being
right**, and by the first rule in [AGENTS.md](../../../../AGENTS.md) it is the correct trade. It is
recorded here rather than fixed.

### What the suite cannot see, which is worth more than the rows it can

**None of these ten solver benchmarks benefit from #918.** They are quadratics, a substituted
quadratic and a trigonometric substitution; not one of them factors into lower-degree pieces, so
every one pays the search and none collects the answer. The column therefore shows the change as
pure cost, which is true of these inputs and false of the change.

A benchmark whose polynomial *does* factor — the quintic from #918's own changelog entry is the
obvious candidate — is owed before the next column, or this row will keep reporting a correctness
fix as a slowdown for as long as anyone reads it.
