using LogicModule.Nodes.TestHelper;
using NUnit.Framework;

namespace d_albuschat_gmail_com.LogicNodes.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_HeaderMode
    {

        [Test]
        public void When_NumberOfHeadersIsZero_Should_NotShowHeaderParameters()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set number of headers to 0
            node.HeaderMode.Value = "HeaderMode.0";
            // Assert: List of Headers is empty
            Assert.AreEqual(0, node.Headers.Count);
        }

        [Test]
        public void When_NumberOfHeadersIsOne_Should_ShowOneHeaderParameter()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set number of headers to 1
            node.HeaderMode.Value = "HeaderMode.1";
            // Assert: List of Headers is empty
            Assert.AreEqual(1, node.Headers.Count);
        }

        [Test]
        public void When_NumberOfHeadersIsTwo_Should_ShowTwoHeaderParameters()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set number of headers to 2
            node.HeaderMode.Value = "HeaderMode.2";
            // Assert: List of Headers is empty
            Assert.AreEqual(2, node.Headers.Count);
        }

        [Test]
        public void When_NumberOfHeadersIsThree_Should_ShowThreeHeaderParameters()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set number of headers to 3
            node.HeaderMode.Value = "HeaderMode.3";
            // Assert: List of Headers is empty
            Assert.AreEqual(3, node.Headers.Count);
        }

        [Test]
        public void When_NumberOfHeadersIsMax_Should_ShowMaxHeaderParameters()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set number of headers to max
            node.HeaderMode.Value = "HeaderMode.10";
            // Assert: List of Headers is empty
            Assert.AreEqual(10, node.Headers.Count);
        }

    }
}
