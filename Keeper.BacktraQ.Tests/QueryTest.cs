using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.BacktraQ.Tests
{
    [TestClass]
    public class QueryTest
    {
        private static int Count(IEnumerable resultSet)
        {
            int count = 0;

            foreach (var result in resultSet)
            {
                count++;
            }

            return count;
        }

        [TestMethod]
        public void ShouldSucceedHardcodedTest()
        {
            var target = Query.Success;

            Assert.IsTrue(target.Succeeds());
        }

        [TestMethod]
        public void ShouldFailHardcodedTest()
        {
            var target = Query.Fail;

            Assert.IsFalse(target.Succeeds());
        }

        [TestMethod]
        public void ShouldHaveOneResultFromTest()
        {
            var target = Query.Success;

            var results = target.AsEnumerable();

            Assert.AreEqual(1, Count(results));
        }

        [TestMethod]
        public void ShouldHaveNoResultsFromFailedTest()
        {
            var target = Query.Fail;

            var results = target.AsEnumerable();

            Assert.AreEqual(0, Count(results));
        }

        [TestMethod]
        public void ShouldUnifyVariable()
        {
            var variable = new Var<int>();

            var target = variable <= 123;

            Assert.IsTrue(target.Succeeds());
        }

        [TestMethod]
        public void ShouldUpdateVariablesOnSuccess()
        {
            var variable = new Var<int>();

            var target = variable <= 123;

            Assert.IsTrue(target.Succeeds());

            Assert.AreEqual(123, variable.Value);
        }

        [TestMethod]
        public void ShouldRevertVariableOnFail()
        {
            var variable = new Var<int>();

            var target = variable <= 123 & Query.Fail;

            Assert.IsFalse(target.Succeeds());

            Assert.IsFalse(variable.HasValue);
        }

        [TestMethod]
        public void ShouldRevertVariableOnNot()
        {
            var variable = new Var<int>();

            var target = !(variable <= 123);

            Assert.IsFalse(target.Succeeds());

            Assert.IsFalse(variable.HasValue);
        }

        [TestMethod]
        public void ShouldListMask()
        {
            var var1 = new Var<int>();

            var list1 = VarList.Create(1, 2, 3, 4, 5);
            var list2 = VarList.Create(3, 4, 5, 6, 7);

            var target = list1.Member(var1) & !list2.Member(var1);

            CollectionAssert.AreEqual(new[] { 1, 2 }, target.AsEnumerable(var1).ToArray());
        }
    }
}
