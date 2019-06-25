using LogicModule.Nodes.TestHelper;
using NUnit.Framework;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Net;

namespace d_albuschat_gmail_com.LogicNodes.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_Authorization
    {
        // TODO: Merge this with the implementations from WebRequestNodeTests_{Runtime,Failure}
        private static WebServer server;
        private static CancellationTokenSource serverCancel;
        private static Task serverTask;
        private static readonly string dummyResponse = "Yehaaaw";

        private static HttpListenerRequest lastRequest;

        [OneTimeSetUp]
        public void Init()
        {
            var url = "http://localhost:12345/";

            // Create Webserver and attach LocalSession and Static
            // files module and CORS enabled
            server = WebServer.Create(url);
            server.RegisterModule(new FallbackModule((context, cancel) =>
            {
                lastRequest = context.Request;
                if (lastRequest.Url.ToString().EndsWith("echo"))
                {
                    lastRequest.InputStream.CopyTo(context.Response.OutputStream);
                }
                else
                {
                    using (var reader = new StreamReader(lastRequest.InputStream))
                    {
                        reader.ReadToEnd();
                    }
                    var responseBytes = Encoding.UTF8.GetBytes(dummyResponse);
                    context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                }
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
        public void When_AuthTypeIsNone_Should_NotHaveUserNameAndPasswordAndTokenInputs()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set AuthType to BasicAuth
            node.AuthType.Value = AuthCredentials.AuthType.NoAuth.ToString();
            // Assert:
            Assert.IsNull(node.AuthToken);
            Assert.IsNull(node.AuthPassword);
            Assert.IsNull(node.AuthUserName);
        }

        [Test]
        public void When_AuthTypeIsBasic_Should_HaveUserNameAndPasswordInputs_But_NoTokenInput()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set AuthType to BasicAuth
            node.AuthType.Value = AuthCredentials.AuthType.BasicAuth.ToString();
            // Assert:
            Assert.IsNull(node.AuthToken);
            Assert.IsNotNull(node.AuthUserName);
            Assert.IsNotNull(node.AuthPassword);
        }

        [Test]
        public void When_AuthTypeIsBasic_Should_HaveTokenInput_But_NoUserNameAndPasswordInputs()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set AuthType to BasicAuth
            node.AuthType.Value = AuthCredentials.AuthType.BearerToken.ToString();
            // Assert:
            Assert.IsNotNull(node.AuthToken);
            Assert.IsNull(node.AuthUserName);
            Assert.IsNull(node.AuthPassword);
        }

        [Test]
        public void When_AuthTypeIsUnchanged_Should_BeSetToNone_And_NotSendAuthorizationHeader()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.AreEqual(AuthCredentials.AuthType.NoAuth.ToString(), node.AuthType.Value);
            Assert.IsNull(lastRequest.Headers.Get("Authorization"));
        }

        [Test]
        public void When_AuthTypeIsNone_Should_NotSendAuthorizationHeader()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.AuthType.Value = "None";
            node.URL.Value = "http://localhost:12345/";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.IsNull(lastRequest.Headers.Get("Authorization"));
        }

        [Test]
        public void When_AuthTypeIsBearer_Should_SendBearerTokenInAuthHeader()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.AuthType.Value = AuthCredentials.AuthType.BearerToken.ToString();
            node.AuthToken.Value = "T0K3N";
            node.URL.Value = "http://localhost:12345/";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.AreEqual("Bearer T0K3N", lastRequest.Headers.Get("Authorization"));
        }

        [Test]
        public void When_AuthTypeIsBasic_Should_SendUserNameAndPasswordInAuthHeader()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.AuthType.Value = AuthCredentials.AuthType.BasicAuth.ToString();
            node.AuthUserName.Value = "admin";
            node.AuthPassword.Value = "p4$$w0rd";
            node.URL.Value = "http://localhost:12345/";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            string authHeader = lastRequest.Headers.Get("Authorization");
            Assert.AreEqual(0, authHeader.IndexOf("Basic "));
            Assert.AreEqual("admin:p4$$w0rd", Encoding.UTF8.GetString(System.Convert.FromBase64String(authHeader.Substring("Basic ".Length))));
        }

    }
}
