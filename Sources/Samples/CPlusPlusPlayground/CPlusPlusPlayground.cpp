// CPlusPlusPlayground.cpp : Defines the entry point for the application.
//

#include "CPlusPlusPlayground.h"
#include "AngouriMath.h"
#include "A.Usages.MathS.Functions.h"

using namespace std;

int main()
{
    AngouriMath::Entity expr("x + 2sin(x)");
    AngouriMath::ErrorCode err;
    auto newExpr = expr.Differentiate("x");
    // std::cout << AngouriMath::Abs(newExpr);
    for (auto ent : newExpr.Nodes())
        std::cout << ent << "\n";
    return 0;
}
