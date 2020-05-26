Since 331st commit, AM is not compatible to the previous versions. Here are changes you need to apply to migrate to the new version:

1. Log swapped to (base, number)
2. To check whether number is not real, use IsImaginary instead of IsComplex
3. Don't use constructor new Number. Use Number.Create(decimal, decimal) for complex numbers, Number.Create(long, long) for rationals, Number.Create(long) for integers, Number.Create(decimal) for real
4. 