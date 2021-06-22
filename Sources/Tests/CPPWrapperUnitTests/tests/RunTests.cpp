#include <AngouriMath.h>
#include <gtest/gtest.h>

TEST(RunTests, ParsingTest1) {
    auto src = "x / 2 + 3";
    AngouriMath::Entity entity = src;
    EXPECT_EQ(entity.ToString(), src);
}

TEST(RunTests, ParsingTest2) {
    auto src = "x / 2 + 3 + 6";
    AngouriMath::Entity entity = src;
    EXPECT_EQ(entity.ToString(), src);
}

TEST(RunTests, ToString1) {
    auto src = "sqrt(x)";
    AngouriMath::Entity entity = src;
    EXPECT_EQ(entity.ToString(), src);
}

TEST(RunTests, ParsingTest3) {
    auto src = "x / 2 + 3 + integral(6, x)";
    AngouriMath::Entity entity = src;
    EXPECT_EQ(entity.ToString(), src);
}

TEST(RunTests, Latex1) {
    auto src = "x + 1";
    AngouriMath::Entity entity = src;
    EXPECT_EQ("x+1", entity.Latexise());
}

TEST(RunTests, Latex2) {
    auto src = "sqrt(x)";
    AngouriMath::Entity entity = src;
    EXPECT_EQ("\\sqrt{x}", entity.Latexise());
}

TEST(RunTests, DiffTest1) {
    auto src = "a x + 2";
    AngouriMath::Entity entity = src;
    auto actual = entity.Differentiate("x");
    auto expected = AngouriMath::Entity("a");
    EXPECT_EQ(expected.ToString(), actual.ToString());
}

TEST(RunTests, DiffTest2) {
    auto src = "a x + 2x";
    AngouriMath::Entity entity = src;
    auto actual = entity.Differentiate("x");
    auto expected = AngouriMath::Entity("a + 2");
    EXPECT_EQ(expected.ToString(), actual.ToString());
}

TEST(RunTests, IntTest1) {
    auto src = "x";
    AngouriMath::Entity entity = src;
    auto actual = entity.Integrate("x");
    auto expected = AngouriMath::Entity("x2 / 2");
    EXPECT_EQ(expected.ToString(), actual.ToString());
}

TEST(RunTests, IntTest2) {
    auto src = "x + 2";
    AngouriMath::Entity entity = src;
    auto actual = entity.Integrate("x");
    auto expected = AngouriMath::Entity("x2 / 2 + 2x");
    EXPECT_EQ(expected.ToString(), actual.ToString());
}

TEST(RunTests, LimTest1) {
    auto src = "x + 2";
    AngouriMath::Entity entity = src;
    auto actual = entity.Limit("x", "3");
    auto expected = AngouriMath::Entity("5");
    EXPECT_EQ(expected.ToString(), actual.ToString());
}

TEST(RunTests, LimTest2) {
    auto src = "sin(a x) / (b x)";
    AngouriMath::Entity entity = src;
    auto actual = entity.Limit("x", "0");
    auto expected = AngouriMath::Entity("a / b");
    EXPECT_EQ(expected.ToString(), actual.ToString());
}

TEST(RunTests, Nodes1) {
    auto nodes = AngouriMath::Entity("x + 1").Nodes();
    EXPECT_EQ(3, nodes.size());
    EXPECT_EQ(AngouriMath::Entity("x + 1").ToString(), nodes[0].ToString());
    EXPECT_EQ(AngouriMath::Entity("x").ToString(), nodes[1].ToString());
    EXPECT_EQ(AngouriMath::Entity("1").ToString(), nodes[2].ToString());
}

TEST(RunTests, Vars1) {
    auto expr = AngouriMath::Entity("x + pi + y");
    auto nodes = expr.Vars();
    EXPECT_EQ(2, nodes.size());
    EXPECT_EQ(AngouriMath::Entity("x").ToString(), nodes[0].ToString());
    EXPECT_EQ(AngouriMath::Entity("y").ToString(), nodes[1].ToString());
}

TEST(RunTests, VarsAndConstants1) {
    auto expr = AngouriMath::Entity("x + pi + y");
    auto nodes = expr.VarsAndConsts();
    EXPECT_EQ(3, nodes.size());
    EXPECT_EQ(AngouriMath::Entity("x").ToString(), nodes[0].ToString());
    EXPECT_EQ(AngouriMath::Entity("pi").ToString(), nodes[1].ToString());
    EXPECT_EQ(AngouriMath::Entity("y").ToString(), nodes[2].ToString());
}


TEST(RunTests, Alternate1) {
    auto expr = AngouriMath::Entity("x + sin(x / 2)");
    auto nodes = expr.Alternate();
    EXPECT_EQ(2, nodes.size());
    EXPECT_EQ(AngouriMath::Entity("x + sin(x / 2)").ToString(), nodes[0].ToString());
    EXPECT_EQ(AngouriMath::Entity("sin(x / 2) + x").ToString(), nodes[1].ToString());
}

TEST(RunTests, Simplify1) {
    auto expr = AngouriMath::Entity("sin(a)2 + cos(a)2");
    EXPECT_EQ(expr.Simplify().ToString(), AngouriMath::Entity("1").ToString());
}

/*

TEST(RunTests, Evaluate1) {
    auto expr = AngouriMath::Entity("sqrt(2)");
    auto evaled = expr.Evaled();
    auto ans = evaled.ToString();
    EXPECT_EQ(ans, "1.4142");
}

TEST(RunTests, InnerSimplified1) {
    auto expr = AngouriMath::Entity("1 * 0");
    auto innerSimplified = expr.InnerSimplified();
    EXPECT_EQ(innerSimplified.ToString(), "0");
}

*/

/*

TEST(RunTests, ToLong1) {
    auto expr = AngouriMath::Entity("116");
    EXPECT_EQ(116l, expr.AsInteger());
}

*/

/*

TEST(RunTests, ToRational1) {
    auto expr = AngouriMath::Entity("5 / 30");
    auto sim = expr.Simplify(); 
    auto rat = sim.AsRational();
    EXPECT_EQ(1l, rat.first);
    EXPECT_EQ(6l, rat.second);
}

*/

TEST(RunTests, ToDouble) {
    auto expr = AngouriMath::Entity("6");
    EXPECT_EQ(6.0, expr.AsReal());
}

TEST(RunTests, ToComplex) {
    auto expr = AngouriMath::Entity("6 + i");
    auto comSim = expr.Simplify();
    auto com = comSim.AsComplex();
    EXPECT_EQ(6.0, com.real());
    EXPECT_EQ(1.0, com.imag());
}
