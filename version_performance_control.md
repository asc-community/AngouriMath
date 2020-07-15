We compare performances through different versions with CommonFunctionsInterVersionTest.cs in Tests

|          Method |              331st |               380th |          391st      |              410th |
|---------------- |-------------------:|--------------------:|--------------------:|-------------------:|
|       ParseEasy |        28,599.1 ns |         73,669.3 ns |        134,120.0 ns |        44,328.1 ns |
|       ParseHard |        92,037.1 ns |        208,710.3 ns |        415,440.1 ns |       178,760.4 ns |
|    SimplifyEasy |     6,225,707.4 ns |     16,387,374.0 ns |     41,081,822.7 ns |       397,973.5 ns |
|    SimplifyHard | 3,245,613,128.6 ns |  8,120,319,273.0 ns |  9,715,629,251.0 ns | 7,477,089,153.8 ns |
|        Derivate |         5,550.2 ns |         39,895.0 ns |        161,507.0 ns |        50,858.5 ns |
|       SolveEasy |       677,259.6 ns |      4,066,049.4 ns |     33,760,616.5 ns |     7,556,902.7 ns |
| SolveEasyMedium |       192,319.2 ns |        741,114.4 ns |      2,861,751.6 ns |       646,063.0 ns |
|     SolveMedium |    22,487,990.6 ns |    126,650,858.0 ns |    690,232,457.0 ns |   178,691,349.5 ns |
| SolveMediumHard |       > 1 h        |       > 1 h         | 12,033,099,074.0 ns | 2,754,157,261.1 ns |
|       SolveHard |       > 1 h        |       > 1 h         | 21,565,808,100.0 ns | 5,636,112,783.3 ns |
|        EvalEasy |        12,059.1 ns |        200,766.1 ns |      1,548,901.3 ns |       550,676.0 ns |
|     CompileEasy |        24,771.0 ns |         38,300.7 ns |         62,328.0 ns |        33,684.3 ns |
|     CompileHard |        61,572.1 ns |         84,042.0 ns |        139,499.2 ns |        73,738.5 ns |
|         RunEasy |           138.9 ns |            126.8 ns |            123.2 ns |           117.5 ns |
|       RunMedium |           833.4 ns |            950.2 ns |            920.0 ns |           844.0 ns |
|         RunHard |         1,403.9 ns |          1,644.5 ns |          1,558.3 ns |         1,427.9 ns |