using LogicModule.Nodes.TestHelper;
using NUnit.Framework;

namespace d_albuschat_gmail_com.logic.Nodes.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_Outputs
    {

        [Test]
        public void When_ParametersAreDefault_Should_ShowOutputsResponseAndResponseErrorCodeAndResponseErrorCodeMessage()
        {
            // Arrange: New node with default parameters
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Nothing
            // Assert: The Outputs Response, ResponseErrorCode and ResponseErrorMessage must be visible
            Assert.IsNotNull(node.Response);
            Assert.IsNotNull(node.ErrorCode);
            Assert.IsNotNull(node.ErrorMessage);
            Assert.IsFalse(node.Response.HasValue);
            Assert.IsFalse(node.ErrorCode.HasValue);
            Assert.IsFalse(node.ErrorMessage.HasValue);
        }

    }
}
