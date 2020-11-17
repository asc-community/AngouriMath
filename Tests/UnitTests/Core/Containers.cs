using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.Core
{
    public sealed class Containers
    {
        [Fact]
        public void TestSimple1()
        {
            var container = new LazyContainer<int>();
            Assert.Equal(4, container.GetValue(() => 4));
            Assert.Equal(4, container.GetValue(() => 4));
        }

        [Fact]
        public void TestSimpleString()
        {
            var container = new LazyContainer<string>();
            Assert.Equal("ss", container.GetValue(() => "ss"));
            Assert.Equal("ss", container.GetValue(() => "ss"));
        }

        private record SomeTestRecord
        {
            public ConcurrentDictionary<string, string> Dict => dict.GetValue(() => new());
            private LazyContainer<ConcurrentDictionary<string, string>> dict;
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
            public string FullName => fullName.GetValue(() => FirstName + LastName);
            private LazyContainer<string> fullName;
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
