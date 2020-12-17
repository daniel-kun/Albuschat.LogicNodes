using LogicModule.Nodes.TestHelper;
using NUnit.Framework;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Net;

namespace d_albuschat_gmail_com.logic.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_Failures
    {
        private static WebServer server;
        private static CancellationTokenSource serverCancel;
        private static Task serverTask;

        private static HttpListenerRequest lastRequest;

        [OneTimeSetUp]
        public void Init()
        {
            var url = "http://localhost:12346/";

            // Create Webserver and attach LocalSession and Static
            // files module and CORS enabled
            server = WebServer.Create(url);
            server.RegisterModule(new FallbackModule((context, cancel) =>
            {
                lastRequest = context.Request;
                string endpoint = lastRequest.RawUrl.Substring(1);
                bool redirect = endpoint.StartsWith("redirect/");
                if (redirect)
                {
                    endpoint = endpoint.Substring("redirect/".Length);
                }
                if (Regex.Matches(endpoint, "^[1-5][0-9][0-9]$").Count > 0) {
                    context.Response.StatusCode = int.Parse(endpoint);
                    switch (context.Response.StatusCode)
                    {
                        case 301:
                            context.Response.Headers.Add("Location: /redirect/200");
                            break;
                        case 302:
                            context.Response.Headers.Add("Location: /doesnotexist");
                            break;
                    }
                } else
                {
                    context.Response.StatusCode = 404;
                }
                string valueName = redirect ? "redirect" : "statusCode";
                var responseBytes = Encoding.UTF8.GetBytes($"{{{valueName}: {context.Response.StatusCode}}}");
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                return true;
            }));
            serverCancel = new CancellationTokenSource();
            serverTask = server.RunAsync(serverCancel.Token);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            serverCancel.Cancel();
            serverTask.Wait();
            server.Dispose();
        }

        [SetUp]
        public void InitTest()
        {
            lastRequest = null;
        }

        [Test]
        public void When_RequestSucceeds_Should_OutputRespose()
        {
            // Arrange: Create node pointing to a 200 response code
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/200";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Response body must include status code and Error* outputs are not set
            Assert.AreEqual("{statusCode: 200}", node.Response.Value);
            Assert.IsFalse(node.ErrorCode.HasValue);
            Assert.IsFalse(node.ErrorMessage.HasValue);
        }

        [Test]
        public void When_UrlIsMisformed_Should_OutputClientSideError()
        {
            // Arrange: Create node pointing to a malformed URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346@foo-bar@blah$$$dollar/200";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 999, ErrorMessage contains a message and Response must not be set
            Assert.AreEqual(999, node.ErrorCode.Value);
            Assert.IsNotNull(node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

        [Test]
        public void When_ProtocolIsNotSupported_Should_OutputError()
        {
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "rsync://localhost:12346/200";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 999, ErrorMessage contains a message and Response must not be set
            Assert.AreEqual(997, node.ErrorCode.Value);
            Assert.IsNotNull(node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

        [Test]
        public void When_UsingFTP_Should_FailWithErrorCode()
        {
            // Arrange: Create web request with FTP URI
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "ftp://192.168.178.1";
            // Act: Execute node
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Error code 997 with corresponding error message must be replied
            Assert.AreEqual(997, node.ErrorCode.Value);
            Assert.IsTrue(node.ErrorMessage.HasValue);
        }

        [Test]
        public void When_HeaderIsMisformed_Should_OutputError()
        {
            // Arrange: Create request with invalid header syntax
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/200";
            node.HeaderMode.Value = "HeaderMode.1";
            node.Headers[0].Value = "Header = Value";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 999, ErrorMessage contains a message and Response must not be set
            Assert.AreEqual(999, node.ErrorCode.Value);
            Assert.IsNotNull(node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

        /*
        This test is deactivated, because covering 1xx status codes is very specific
        [Test]
        public void When_ResposeReturns1xxCode_Should_OutputResponse()
        {
            // Arrange: Create request with 102 response code
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/102";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Respose is set and ErrorMessage and ErrorCode must not be set
            Assert.IsTrue(node.Response.HasValue); // In the tests, resposes with 102 status code did not include the body
            Assert.IsFalse(node.ErrorMessage.HasValue);
            Assert.IsFalse(node.ErrorCode.HasValue);
        }
        */

        [Test]
        public void When_ResposeReturns2xxCode_Should_OutputResponse()
        {
            // Arrange: Create request with 202 response code
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/202";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Response contains response body and Error* must not be set
            Assert.AreEqual("{statusCode: 202}", node.Response.Value);
            Assert.IsFalse(node.ErrorMessage.HasValue);
            Assert.IsFalse(node.ErrorCode.HasValue);
        }

        [Test]
        public void When_ResposeReturns3xxCode_Should_FollowRedirect_And_OutputResponse()
        {
            // Arrange: Create request to a redirecting endpoint
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/301";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Response contains response body from the redirected location, and Error* must not be set
            Assert.AreEqual("{redirect: 200}", node.Response.Value);
            Assert.IsFalse(node.ErrorMessage.HasValue);
            Assert.IsFalse(node.ErrorCode.HasValue);
        }

        [Test]
        public void When_ResposeReturns3xxCodeAndRedirectLocationFails_Should_OutputError()
        {
            // Arrange: Create request to a redirecting endpoint
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/302";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 404, ErrorMessage contains status code 404 and Response must not be set
            Assert.AreEqual(404, node.ErrorCode.Value);
            Assert.AreEqual("{statusCode: 404}", node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

        [Test]
        public void When_ResposeReturns4xxCode_Should_OutputError()
        {
            // Arrange: Create request to a 400 response
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/400";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 400, ErrorMessage contains status code 400 and Response must not be set
            Assert.AreEqual(400, node.ErrorCode.Value);
            Assert.AreEqual("{statusCode: 400}", node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

        [Test]
        public void When_ResposeReturns404Code_Should_OutputError()
        {
            // Arrange: Create request to a 404 response
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/404";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 404, ErrorMessage contains status code 404 and Response must not be set
            Assert.AreEqual(404, node.ErrorCode.Value);
            Assert.AreEqual("{statusCode: 404}", node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

        [Test]
        public void When_ResposeReturns5xxCode_Should_OutputError()
        {
            // Arrange: Create request to a 502 response
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/502";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 502, ErrorMessage contains status code 502 and Response must not be set
            Assert.AreEqual(502, node.ErrorCode.Value);
            Assert.AreEqual("{statusCode: 502}", node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

        [Test]
        public void When_ResposeReturns500Code_Should_OutputError()
        {
            // Arrange: Create request to a 500 response
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12346/500";
            // Act: Execute request
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: ErrorCode is set to 500, ErrorMessage contains status code 500 and Response must not be set
            Assert.AreEqual(500, node.ErrorCode.Value);
            Assert.AreEqual("{statusCode: 500}", node.ErrorMessage.Value);
            Assert.IsFalse(node.Response.HasValue);
        }

    }
}
