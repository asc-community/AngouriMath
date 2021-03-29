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

TEST(RunTests, ParsingTest3) {
    auto src = "x / 2 + 3 + integral(6, x)";
    AngouriMath::Entity entity = src;
    EXPECT_EQ(entity.ToString(), src);
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