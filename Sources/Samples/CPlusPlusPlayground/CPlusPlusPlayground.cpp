// CPlusPlusPlayground.cpp : Defines the entry point for the application.
//

#include "CPlusPlusPlayground.h"
#include "AngouriMath.CPP.h"

using namespace std;

int main()
{
	AngouriMath::Entity expr;
	//expr = AngouriMath::Entity("sin(x)");
	auto newExpr = expr.Differentiate("x");
	std::cout << newExpr;
	return 0;
}
