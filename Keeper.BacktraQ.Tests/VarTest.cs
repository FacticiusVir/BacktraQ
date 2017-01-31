using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Keeper.BacktraQ.Tests
{
    [TestClass]
    public class VarTest
    {
        [TestMethod]
        public void ShouldUnifyVarToPrimitive()
        {
            var target = new Var<int>();

            Assert.IsTrue(target.TryUnify(123));
        }

        [TestMethod]
        public void ShouldUnifyPrimitives()
        {
            var target = (Var<int>)123;

            Assert.IsTrue(target.TryUnify(123));
        }
    }
}
