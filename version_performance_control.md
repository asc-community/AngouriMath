We compare performances through different versions with CommonFunctionsInterVersionTest.cs in Tests

|          Method |               380th |          391st      |
|---------------- |--------------------:|--------------------:|
|       ParseEasy |         73,669.3 ns |        134,120.0 ns |
|       ParseHard |        208,710.3 ns |        415,440.1 ns |
|    SimplifyEasy |     16,387,374.0 ns |     41,081,822.7 ns |
|    SimplifyHard |  8,120,319,273.0 ns |  9,715,629,251.0 ns |
|        Derivate |         39,895.0 ns |        161,507.0 ns |
|       SolveEasy |      4,066,049.4 ns |     33,760,616.5 ns |
| SolveEasyMedium |        741,114.4 ns |      2,861,751.6 ns |
|     SolveMedium |    126,650,858.0 ns |    690,232,457.0 ns |
| SolveMediumHard |       > 1 h         | 12,033,099,074.0 ns |
|       SolveHard |       > 1 h         | 21,565,808,100.0 ns |
|        EvalEasy |        200,766.1 ns |      1,548,901.3 ns |
|     CompileEasy |         38,300.7 ns |         62,328.0 ns |
|     CompileHard |         84,042.0 ns |        139,499.2 ns |
|         RunEasy |            126.8 ns |            123.2 ns |
|       RunMedium |            950.2 ns |            920.0 ns |
|         RunHard |          1,644.5 ns |          1,558.3 ns |