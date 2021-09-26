﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tensorflow;
using Tensorflow.NumPy;

namespace TensorFlowNET.UnitTest.NumPy
{
    /// <summary>
    /// https://numpy.org/doc/stable/reference/generated/numpy.prod.html
    /// </summary>
    [TestClass]
    public class MathTest : EagerModeTestBase
    {
        [TestMethod]
        public void prod()
        {
            var p = np.prod(1.0, 2.0);
            Assert.AreEqual(p, 2.0);

            p = np.prod(new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } });
            Assert.AreEqual(p, 24.0);

            p = np.prod(new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } }, axis: 1);
            Assert.AreEqual(p.shape, 2);
            Assert.IsTrue(Equal(p.ToArray<double>(), new[] { 2.0, 12.0 }));
        }

        [TestMethod]
        public void astype()
        {
            var x = np.array(new byte[] { 1, 100, 200 });
            var x1 = x.astype(np.float32);
            Assert.AreEqual(x1[2], 200f);
        }

        [TestMethod]
        public void divide()
        {
            var x = np.array(new float[] { 1, 100, 200 });
            var y = x / 2;
            Assert.AreEqual(y.dtype, np.float32);
        }

        [TestMethod]
        public void sin()
        {
            var x = np.sin(np.pi / 2);
            Assert.AreEqual(x, 1d);
        }

        [TestMethod]
        public void cos()
        {
            var x = np.cos(np.pi / 2);
            Assert.AreEqual(x, 6.123233995736766e-17);
        }

        [TestMethod]
        public void power()
        {
            var x = np.arange(6);
            var y = np.power(x, 3);
            Assert.AreEqual(y, new[] { 0, 1, 8, 27, 64, 125 });
        }
    }
}
