// CPlusPlusPlayground.cpp : Defines the entry point for the application.
//

#include "CPlusPlusPlayground.h"
#include "AngouriMath.CPP.h"
#include "A.Usages.MathS.Functions.h"

using namespace std;

int main()
{
	AngouriMath::Entity expr("x + 2sin(x)");
	auto newExpr = expr.Differentiate("x");
	std::cout << AngouriMath::Sin(newExpr);
	return 0;
}
