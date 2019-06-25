using LogicModule.Nodes.TestHelper;
using NUnit.Framework;

namespace d_albuschat_gmail_com.LogicNodes.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_BasicParameters
    {

        [Test]
        public void When_ParametersAreDefault_Should_ShowParametersTriggerAndMethodAndUrlAndHeaderMode()
        {
            // Arrange: New node with default parameters
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Nothing
            // Assert: The Parameters Trigger, Method, Url and HeaderMode must be visible and have the appropriate default values.
            Assert.IsFalse(node.Trigger.HasValue);
            Assert.AreEqual("GET", node.Method.Value);
            Assert.IsFalse(node.URL.HasValue);
            Assert.AreEqual("HeaderMode.0", node.HeaderMode.Value);
        }

        [Test]
        public void When_ParametersAreDefault_Should_NotShowBodyAndContentType()
        {
            // Arrange: New node with default parameters
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Nothing
            // Assert: Body and ContentType must not be visible
            Assert.IsNull(node.Body);
            Assert.IsNull(node.ContentType);
        }

        [Test]
        public void When_MethodIsGet_Should_HideBodyAndContentTypeParameters()
        {
            // Arrange: New node with default parameters
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Switch Method to POST and then back to GET
            node.Method.Value = "POST";
            node.Method.Value = "GET";
            // Assert: Parameter Body and ContentType must not be visible.
            Assert.IsNull(node.Body);
            Assert.IsNull(node.ContentType);
        }

        [Test]
        public void When_MethodIsPost_Should_ShowBodyAndContentTypeParameter()
        {
            // Arrange: New node with Method set to POST
            var node = new WebRequestNode(TestNodeContext.Create());
            node.Method.Value = "POST";
            // Act: Nothing
            // Assert: The Parameter Body and ContentType must be visible
            Assert.IsNotNull(node.Body);
            Assert.IsNotNull(node.ContentType);
        }

    }
}
