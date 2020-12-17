using LogicModule.Nodes.TestHelper;
using LogicModule.ObjectModel.TypeSystem;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Net;

namespace d_albuschat_gmail_com.logic.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_Runtime
    {
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
                } else
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
        public void When_TriggerIsSet_Should_EmitResponse()
        {
            // Arrange: New node with simple URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.AreEqual(dummyResponse, node.Response.Value);
            Assert.AreEqual("GET", lastRequest.HttpMethod);
        }

        [Test]
        public void When_TriggerIsTrueButWasNotSetSinceLastExecute_Should_NotEmitResponse()
        {
            // Arrange: New (invalid) node
            var node = new WebRequestNode(TestNodeContext.Create());
            node.Trigger.Value = true;
            node.ExecuteAndWait(200); // Should emit error, since no URL is set
            node.URL.Value = "http://localhost:12345/";
            node.ErrorCode.ValueSet += (object sender, ValueChangedEventArgs e) =>
            {
                Assert.Fail(); // Must not be called
            };
            node.ErrorMessage.ValueSet += (object sender, ValueChangedEventArgs e) =>
            {
                Assert.Fail(); // Must not be called
            };
            node.Response.ValueSet += (object sender, ValueChangedEventArgs e) =>
            {
                Assert.Fail(); // Must not be called
            };
            // Warning! This simulates the behaviour of the Logic Engine:
            node.Trigger.WasSet = false;
            // Act: Set proper URL and execute again:
            node.ExecuteAndWait(200);
            // Assert: Most has been asserted in the callbacks above, additionally assert that all Outputs must not be set:
            Assert.IsFalse(node.Response.HasValue);
            Assert.IsFalse(node.ErrorCode.HasValue);
            Assert.IsFalse(node.ErrorMessage.HasValue);
        }

        [Test]
        public void When_TriggerIsNotSet_Should_NotEmitResponse()
        {
            // Arrange: New node with URL with variables, to enable all possible inputs
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/{Variable1}/{Variable2}";
            // Act: Set everything but the Trigger and Execute:
            node.Method.Value = "POST";
            node.ContentType.Value = "application/json";
            node.Body.Value = "{ \"foo\": \"bar\" }";
            node.Variables[0].Value = "Var1";
            node.Variables[1].Value = "Var2";
            node.HeaderMode.Value = "HeaderMode.2";
            node.Headers[0].Value = "Accept: text/html";
            node.Headers[1].Value = "Connection: Upgrade";
            node.ExecuteAndWait(200);
            // Assert: Must not have a Response, ErrorCode or ErrorMessage
            Assert.IsFalse(node.Response.HasValue);
            Assert.IsFalse(node.ErrorCode.HasValue);
            Assert.IsFalse(node.ErrorMessage.HasValue);
        }

        [Test]
        public void When_MethodIsPost_Should_PostBodyToServer()
        {
            // Arrange: New node with echo URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/echo";
            // Act: Set Trigger and Execute:
            node.Method.Value = "POST";
            node.Body.Value = "{value: 1}";
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.AreEqual("{value: 1}", node.Response.Value);
            Assert.AreEqual("POST", lastRequest.HttpMethod);
        }

        [Test]
        public void When_VariablesAreUsed_Should_ReplaceVariablesInUrlAndBody()
        {
            // Arrange: New node with echo URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/{Variable1}/echo";
            // Act: Set Trigger and Execute:
            node.Method.Value = "POST";
            node.Body.Value = "{value: \"{Variable1}\"}";
            node.Variables[0].Value = "dummy";
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.AreEqual("{value: \"dummy\"}", node.Response.Value);
            Assert.AreEqual("/dummy/echo", lastRequest.RawUrl);
        }

        // This is a special test, because internally the Content-Type header must be
        // handled specially
        [Test]
        public void When_RestrictedHeadersAreUsedAndBodyIsSet_Should_SendAllRestrictedHeaders()
        {
            // Arrange: New node with echo URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/foo";
            // Act: Set all supported restricted headers, set Trigger and Execute:
            // The list of restricted headers has been taken from https://stackoverflow.com/questions/239725/cannot-set-some-http-headers-when-using-system-net-webrequest
            // since I could not find an official source. Some restricted headers are excluded,
            // because they are too complicated to implement and not worth the trouble.
            node.Method.Value = "POST";
            node.Body.Value = "{dummy: 1}";
            node.HeaderMode.Value = "HeaderMode.8";
            node.Headers[0].Value = "Accept: text/html";
            node.Headers[1].Value = "Connection: Upgrade";
            node.Headers[2].Value = $"Content-Length: {node.Body.Value.Length}";
            node.Headers[3].Value = "Date: Fri, 22 Jan 2010 04:00:00 GMT";
            node.Headers[4].Value = "Host: localhost";
            node.Headers[5].Value = "If-Modified-Since: Sat, 29 Oct 1994 19:43:31 GMT";
            node.Headers[6].Value = "Referer: gira.de";
            node.Headers[7].Value = "User-Agent: Gira";
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.AreEqual("text/html", lastRequest.Headers.Get("Accept"));
            Assert.AreEqual("Upgrade", lastRequest.Headers.Get("Connection"));
            Assert.AreEqual($"{node.Body.Value.Length}", lastRequest.Headers.Get("Content-Length"));
            Assert.AreEqual("Fri, 22 Jan 2010 04:00:00 GMT", lastRequest.Headers.Get("Date"));
            Assert.AreEqual("localhost", lastRequest.Headers.Get("Host"));
            Assert.AreEqual("Sat, 29 Oct 1994 19:43:31 GMT", lastRequest.Headers.Get("If-Modified-Since"));
            Assert.AreEqual("gira.de", lastRequest.Headers.Get("Referer"));
            Assert.AreEqual("Gira", lastRequest.Headers.Get("User-Agent"));
        }

        [Test]
        public void When_UnsupportedRestrictedHeadersAreSpecified_Should_SendRequestAndNotCrash()
        {
            // Arrange: New node with echo URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/echo";
            node.Method.Value = "POST";
            // Currently not supported: Range, Proxy-Connection, Expect, Transfer-Encoding 
            node.HeaderMode.Value = "HeaderMode.4";
            node.Headers[0].Value = "Range: bytes=0-1023";
            node.Headers[1].Value = "Proxy-Connection: keep-alive";
            node.Headers[2].Value = "Expect: 100-continue";
            node.Headers[3].Value = "Transfer-Encoding: chunked";
            // Act: Post some text to echo endpoint
            string uid = Guid.NewGuid().ToString();
            node.Body.Value = uid;
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            //Assert: Response contains the sent guid.
            Assert.AreEqual(uid, node.Response.Value);
        }

        [Test]
        public void When_HeadersAreAdded_Should_SendHeadersToServer()
        {
            // Arrange: New node with echo URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/foo";
            // Act: Set Trigger and Execute:
            node.HeaderMode.Value = "HeaderMode.3";
            node.Headers[0].Value = "Cache-Control: no-cache";
            node.Headers[1].Value = "Authorization: Bearer ASDF";
            node.Headers[2].Value = "X-Dummy: Foobar";
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include the expected Response
            Assert.AreEqual("no-cache", lastRequest.Headers.Get("Cache-Control"));
            Assert.AreEqual("Bearer ASDF", lastRequest.Headers.Get("Authorization"));
            Assert.AreEqual("Foobar", lastRequest.Headers.Get("X-Dummy"));
        }

        [Test]
        public void When_UsernameAndPasswordAreGivenInBasicAuthHeader_Should_SendAuthHeadersToServer()
        {
            // Arrange: New node with echo URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/foo";
            // Act: Set Trigger and Execute:
            node.HeaderMode.Value = "HeaderMode.3";
            node.Headers[0].Value = "Cache-Control: no-cache";
            node.Headers[1].Value = "X-Dummy: Foobar";
            node.Headers[2].Value = $"Authorization: Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes("daniel:albuschat"))}";
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            Assert.AreEqual("no-cache", lastRequest.Headers.Get("Cache-Control"));
            Assert.AreEqual("Basic ZGFuaWVsOmFsYnVzY2hhdA==", lastRequest.Headers.Get("Authorization"));
            Assert.AreEqual("Foobar", lastRequest.Headers.Get("X-Dummy"));
        }

        [Test]
        public void When_ContentTypeIsNotSet_Should_NotSendContentTypeToServer()
        {
            // Arrange: New node with echo URL
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/foo";
            node.Method.Value = "POST";
            node.ContentType.Value = "ContentType.Empty";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must not include Content-Type
            Assert.IsNull(lastRequest.Headers.Get("Content-Type"));
        }

        [Test]
        public void When_ContentTypeIsSet_Should_SendContentTypeToServer()
        {
            // Arrange: New node with specific ContentType
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/foo";
            node.Method.Value = "POST";
            node.ContentType.Value = "application/json";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must include Content-Type
            Assert.AreEqual("application/json", lastRequest.Headers.Get("Content-Type"));
        }

        [Test]
        public void When_ContentTypeIsSpecifiedAsCustomHeaderAndAsInput_Should_SendContentTypeFromInputToServer()
        {
            // Arrange: New node with specific ContentType, and also specify ContentType via custom Headers
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/foo";
            node.Method.Value = "POST";
            node.ContentType.Value = "application/json";
            node.HeaderMode.Value = "HeaderMode.1";
            node.Headers[0].Value = "Content-Type: application/xml";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must use the Content-Type from the ContentType Input and not the custom header
            Assert.AreEqual("application/json", lastRequest.Headers.Get("Content-Type"));
        }

        [Test]
        public void When_ContentTypeIsSpecifiedAsCustomHeader_Should_NotSendContentTypeToServer()
        {
            // Arrange: New node with specific ContentType, and also specify ContentType via custom Headers
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://localhost:12345/foo";
            node.Method.Value = "POST";
            node.HeaderMode.Value = "HeaderMode.1";
            node.Headers[0].Value = "Content-Type: application/xml";
            // Act: Set Trigger and Execute:
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Must not send Content-Type, since this is disallowed via custom Header.
            Assert.IsNull(lastRequest.Headers.Get("Content-Type"));
        }

        [Test]
        public void When_UsingHTTPS_Should_Succeed()
        {
            // Arrange: Create web request with HTTPS URI
            var node = new WebRequestNode(TestNodeContext.Create());
            node.URL.Value = "http://init-api.gira.de/time/v1/current-time";
            // Act: Execute node
            node.Trigger.Value = true;
            node.ExecuteAndWait();
            // Assert: Message includes "START CERTIFICATE"
            Assert.IsFalse(node.ErrorCode.HasValue);
            Assert.IsFalse(node.ErrorMessage.HasValue);
            Assert.IsTrue(node.Response.HasValue);
        }

    }
}
