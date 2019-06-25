using d_albuschat_gmail_com.LogicNodes.WebRequest;
using System.Threading;

namespace d_albuschat_gmail_com.LogicNodes.WebRequest.Tests
{
    public static class WebRequestNodeTestHelpers
    {
        public static void ExecuteAndWait(this WebRequestNode node, int waitTimeInMilliseconds = int.MaxValue)
        {
            var sem = new ManualResetEvent(false);
            node._BeforeExecute = () =>
            {
            };
            node._AfterExecute = () =>
            {
                sem.Set();
            };
            node.ExecuteImpl();
            sem.WaitOne(waitTimeInMilliseconds);
            sem.Dispose();
        }
    }
}
