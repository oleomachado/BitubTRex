﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Linq;

using Bitub.Dto.BcfXml;

namespace Bitub.Dto.Bcf.Tests
{
    [TestClass]
    public class BcfXmlImportTests : BaseTest<BcfXmlImportTests>
    {
        [TestInitialize]
        public void StartUp()
        {
            base.StartUpLogging();
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Bcf21\Example1.bcfzip")]
        public void ReadExample1()
        {
            Assert.IsTrue(File.Exists(@"Resources\Bcf21\Example1.bcfzip"));
            var example1 = BcfFile.ReadFrom(@"Resources\Bcf21\Example1.bcfzip");
            Assert.IsNotNull(example1);
            
            var issues = example1.Issues.ToArray();

            Assert.IsNotNull(issues);
            Assert.AreEqual(16, issues.Length);

            Assert.IsTrue(issues.All(i => null != i.Markup.Topic && 0 < i.Viewpoints.Length));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\Bcf21\Example2.bcfzip")]
        public void ReadExample2()
        {
            Assert.IsTrue(File.Exists(@"Resources\Bcf21\Example2.bcfzip"));
            var example2 = BcfFile.ReadFrom(@"Resources\Bcf21\Example2.bcfzip");
            Assert.IsNotNull(example2);

            var issues = example2.Issues.ToArray();

            Assert.IsNotNull(issues);
            Assert.AreEqual(2, issues.Length);

            Assert.IsTrue(issues.All(i => null != i.Markup.Topic));
            Assert.AreEqual(1, issues.Count(i => i.Viewpoints.Length == 0));

            var extensions = example2.Extensions;
            Assert.AreEqual(6, extensions.Count);
            Assert.AreEqual(3, extensions.First(e => e.Key == "TopicType").Count());
        }
    }
}
