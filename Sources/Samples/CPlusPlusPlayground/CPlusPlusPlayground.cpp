// CPlusPlusPlayground.cpp : Defines the entry point for the application.
//

#include "CPlusPlusPlayground.h"
#include "AngouriMath.h"
#include "A.Usages.MathS.Functions.h"

using namespace std;

int main()
{
    AngouriMath::Entity expr("x + 2 sin(x) + 2y");
    auto newExpr = expr.Differentiate("x");
    for (auto ent : newExpr.Nodes())
        std::cout << ent << "\n";
    std::cout << "\n\n";
    auto ex = AngouriMath::Entity("x + y");
    for (auto ent : ex.Vars())
        std::cout << ent << "\n";
    return 0;
}
