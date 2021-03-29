// CPlusPlusPlayground.cpp : Defines the entry point for the application.
//

#include "CPlusPlusPlayground.h"
#include "AngouriMath.h"
#include "A.Usages.MathS.Functions.h"

using namespace std;

int main()
{
    AngouriMath::ErrorCode err;
    AngouriMath::Entity expr("x + 2 sin(x) + 2", err);
    auto newExpr = expr.Differentiate("x");
    for (auto ent : newExpr.Nodes())
        std::cout << ent << "\n";
    return 0;
}
