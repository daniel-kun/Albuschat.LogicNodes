using LogicModule.Nodes.TestHelper;
using NUnit.Framework;

namespace Albuschat.LogicNodes.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_ContentType
    {

        [Test]
        public void When_MethodIsGet_Should_NotShowContentType()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Nothig
            // Assert: ContentType is not available
            Assert.IsNull(node.ContentType);
            // Act: Set Method to GET
            node.Method.Value = "GET";
            // Assert: ContentType is still not available
            Assert.IsNull(node.ContentType);
        }

        [Test]
        public void When_MethodIsNotGet_Should_ShowContentType()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set Method to somethig other than GET
            node.Method.Value = "PUT";
            // Assert: ContentType is available
            Assert.IsNotNull(node.ContentType);
        }

        [Test]
        public void When_MethodIsNotGet_Should_OfferContentTypes()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set Method to something else than GET
            node.Method.Value = "POST";
            // Assert: ContentType should be available and have these entries:
            Assert.AreEqual(5, node.ContentType.AllowedValues.Count);
            Assert.IsTrue(node.ContentType.AllowedValues.Contains("ContentType.Empty"));
            Assert.IsTrue(node.ContentType.AllowedValues.Contains("text/plain"));
            Assert.IsTrue(node.ContentType.AllowedValues.Contains("application/json"));
            Assert.IsTrue(node.ContentType.AllowedValues.Contains("application/xml"));
            Assert.IsTrue(node.ContentType.AllowedValues.Contains("application/x-www-form-urlencoded"));
        }

        [Test]
        public void When_MethodIsChanged_Should_NotResetContentType()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set Method to somethig other than GET and change ContentType
            node.Method.Value = "PUT";
            node.ContentType.Value = "application/json";
            // Act: Change Method to something else, except GET
            node.Method.Value = "DELETE";
            // Assert: The previously ContentType is not reset
            Assert.AreEqual("application/json", node.ContentType.Value);
        }

    }
}
