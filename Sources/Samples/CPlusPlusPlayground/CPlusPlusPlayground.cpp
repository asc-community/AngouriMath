// CPlusPlusPlayground.cpp : Defines the entry point for the application.
//

#include "CPlusPlusPlayground.h"
#include "AngouriMath.h"
#include "A.Usages.MathS.Functions.h"

using namespace std;

int main()
{
    /*
    AngouriMath::Entity expr("x + 2 sin(x) + 2y");
    auto newExpr = expr.Differentiate("x");
    for (auto ent : newExpr.Nodes())
        std::cout << ent << "\n";
    std::cout << "\n\n";
    auto ex = AngouriMath::Entity("x + y");
    for (auto ent : ex.Vars())
        std::cout << ent << "\n";
        */
    auto expr = AngouriMath::Entity("5 / 30");
    auto sim = expr.Simplify();
    auto rat = sim.AsRational();
    auto real = sim.AsReal();

    AngouriMath::Entity e("x2 + 3x + 1");

    auto solutions = e.SolveEquation("x");

    for (const auto& child : solutions.DirectChildren())
    {
        std::cout << child << '\n';
    }

    return 0;
}
