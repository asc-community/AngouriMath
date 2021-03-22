// CPlusPlusPlayground.cpp : Defines the entry point for the application.
//

#include "CPlusPlusPlayground.h"
#include "AngouriMath.CPP.h"

using namespace std;

int main()
{
	Entity expr = "sin(x)";
	auto newExpr = expr.diff(Entity("x"));
	std::cout << newExpr.to_string();
	return 0;
}
