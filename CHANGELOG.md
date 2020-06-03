Since 331st commit, AM is not compatible to the previous versions. Here are changes you need to apply to migrate to the new version:

1. Log as a static function swapped to (base, number) (but as an entity's method's the order remains)
2. To check whether number is not real, use IsImaginary instead of IsComplex
3. Don't use constructor new Number. Use Number.Create(re, im) for complex numbers, Number.Create(num) for integers, Number.Create(num) for real
4. MathS.Utils.EQUALITY_THRESHOLD -> MathS.Settings.PrecisionErrorCommon
5. Use MathS.Settings to set some system parameters, e. g. MathS.Settings.PrecisionErrorCommon.Set(1.0e-7m) & MathS.Settings.PrecisionErrorCommon.Unset()
6. FromLinq removed
7. All obsolete methods removed, check previous version to look for the replacing method
8. CubicFormula numerical case removed
9. InngerSimplify -> InnerEval