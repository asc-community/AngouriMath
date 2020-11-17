using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.Core
{
    public sealed class Containers
    {
        [Fact]
        public void TestSimple1()
        {
            var container = new Container<int>(() => 4);
            Assert.Equal(4, container.Value);
            Assert.Equal(4, container.Value);
        }

        [Fact]
        public void TestSimpleString()
        {
            var container = new Container<string>(() => "ss");
            Assert.Equal("ss", container.Value);
            Assert.Equal("ss", container.Value);
        }

        private record SomeTestRecord
        {
            public Dictionary<string, string> Dict => dict.Value;
            private Container<Dictionary<string, string>> dict = new(() => new());
        }

        [Fact]
        public void TestThreadSafety()
        {
            SomeTestRecord someInstance = new SomeTestRecord();

            void ChangeADict(int threadId)
            {
                someInstance.Dict["someSpecificKey"] = threadId.ToString();
            }

            new ThreadingChecker(ChangeADict).Run(iterCount: 10000);
        }

        private record Person(string FirstName, string LastName)
        {
            public string FullName => fullName;
            private Container<string> fullName = new(() => FirstName + LastName);
        }

        [Fact]
        public void TestEqualityPure()
        {
            var a = new Person("John", "Ivanov");
            var b = new Person("John", "Ivanov");
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestEqualityOneInitted()
        {
            var a = new Person("John", "Ivanov");
            Assert.Equal("JohnIvanov", a.FullName);
            var b = new Person("John", "Ivanov");
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestEqualityBothInitted()
        {
            var a = new Person("John", "Ivanov");
            Assert.Equal("JohnIvanov", a.FullName);
            var b = new Person("John", "Ivanov");
            Assert.Equal("JohnIvanov", b.FullName);
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestUnequalityPure()
        {
            var a = new Person("Peter", "Smith");
            var b = new Person("John", "Ivanov");
            Assert.NotEqual(a, b);
        }
    }
}
