using LogicModule.Nodes.TestHelper;
using NUnit.Framework;

namespace d_albuschat_gmail_com.logic.WebRequest.Tests
{
    [TestFixture]
    public class WebRequestNodeTests_VariableParameters
    {

        [Test]
        public void When_UrlContainsNoVariables_Should_NotShowVariableParameters()
        {
            // Arrange: New node 
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set URL without variable placeholder
            node.URL.Value = "http://www.google.de/";
            // Assert: No Variable-Parameters must be visible
            Assert.AreEqual(0, node.Variables.Count);
        }

        [Test]
        public void When_UrlContainsOneVariableShouldShowOneVariableParameterWithVariableName()
        {
            // Arrange: New node 
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set URL without variable placeholder
            node.URL.Value = "http://www.google.de/?q={Variable1}";
            // Assert: One Variable-Parameter must be visible
            Assert.AreEqual(1, node.Variables.Count);
            Assert.AreEqual("Variable1", node.Variables[0].Name);
        }

        [Test]
        public void When_UrlContainsTwoVariables_Should_ShowTwoVariableParametersWithVariableNames()
        {
            // Arrange: New node 
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set URL without variable placeholder
            node.URL.Value = "http://www.google.de/?q={Variable1}+{Variable2}";
            // Assert: One Variable-Parameter must be visible
            Assert.AreEqual(2, node.Variables.Count);
            Assert.AreEqual("Variable1", node.Variables[0].Name);
            Assert.AreEqual("Variable2", node.Variables[1].Name);
        }

        [Test]
        public void When_UrlContainsThreeVariables_Should_ShowThreeVariableParametersWithVariableNames()
        {
            // Arrange: New node 
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set URL without variable placeholder
            node.URL.Value = "http://www.google.de/?q={Variable1}+{Variable2}+{Variable3}";
            // Assert: One Variable-Parameter must be visible
            Assert.AreEqual(3, node.Variables.Count);
            Assert.AreEqual("Variable1", node.Variables[0].Name);
            Assert.AreEqual("Variable2", node.Variables[1].Name);
            Assert.AreEqual("Variable3", node.Variables[2].Name);
        }

        [Test]
        public void When_UrlContainsTwentyVariables_Should_ShowTwentyVariableParametersWithVariableNames()
        {
            // Arrange: New node 
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set URL without variable placeholder
            node.URL.Value = "http://www.google.de/?q={Variable1}+{Variable2}+{Variable3}+{Variable4}+{Variable5}+{Variable6}+{Variable7}+{Variable8}+{Variable9}+{Variable10}+{Variable11}+{Variable12}+{Variable13}+{Variable14}+{Variable15}+{Variable16}+{Variable17}+{Variable18}+{Variable19}+{Variable20}";
            // Assert: One Variable-Parameter must be visible
            Assert.AreEqual(20, node.Variables.Count);
            Assert.AreEqual("Variable1", node.Variables[0].Name);
            Assert.AreEqual("Variable2", node.Variables[1].Name);
            Assert.AreEqual("Variable3", node.Variables[2].Name);
            Assert.AreEqual("Variable4", node.Variables[3].Name);
            Assert.AreEqual("Variable5", node.Variables[4].Name);
            Assert.AreEqual("Variable6", node.Variables[5].Name);
            Assert.AreEqual("Variable7", node.Variables[6].Name);
            Assert.AreEqual("Variable8", node.Variables[7].Name);
            Assert.AreEqual("Variable9", node.Variables[8].Name);
            Assert.AreEqual("Variable10", node.Variables[9].Name);
            Assert.AreEqual("Variable11", node.Variables[10].Name);
            Assert.AreEqual("Variable12", node.Variables[11].Name);
            Assert.AreEqual("Variable13", node.Variables[12].Name);
            Assert.AreEqual("Variable14", node.Variables[13].Name);
            Assert.AreEqual("Variable15", node.Variables[14].Name);
            Assert.AreEqual("Variable16", node.Variables[15].Name);
            Assert.AreEqual("Variable17", node.Variables[16].Name);
            Assert.AreEqual("Variable18", node.Variables[17].Name);
            Assert.AreEqual("Variable19", node.Variables[18].Name);
            Assert.AreEqual("Variable20", node.Variables[19].Name);
        }

        [Test]
        public void When_UrlContainsVariablesWithSameName_Should_AddVariableOnlyOnce()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set URL with multiple occurances of the same variables
            node.URL.Value = "http://www.google.de/?q={Variable1}+{Variable1}+{Variable3}+{Variable1}+{Variable2}";
            // Assert: Every variable occurs only once
            Assert.AreEqual(3, node.Variables.Count);
            Assert.AreEqual("Variable1", node.Variables[0].Name);
            Assert.AreEqual("Variable3", node.Variables[1].Name);
            Assert.AreEqual("Variable2", node.Variables[2].Name);
        }

        [Test]
        public void When_UrlContainsMalformedVariables_Should_HaveNoVariables()
        {
            // Arrange: New node 
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set url with valid variable placeholder first
            node.URL.Value = "http://www.google.de/?q={Variable1}";
            // Then update URL with invalid placeholder
            node.URL.Value = "http://www.google.de/?q={{}}+{12asdasd}+{_asdf}+{Asdf_Foo}+{Asdf-Foo}+{Asdf?}+{Über}";
            // Assert: No Variable-Parameter must be visible
            Assert.AreEqual(0, node.Variables.Count);
        }

        [Test]
        public void When_BodyContainsThreeVariables_Should_ShowThreeVariablesWithVariableNames()
        {
            // Arrange: New node
            var node = new WebRequestNode(TestNodeContext.Create());
            // Act: Set Body with variables
            node.Method.Value = "POST";
            node.Body.Value = "{data: {Data}, value: {Value}, order: {Order}}";
            // Assert: The variables should be available
            Assert.AreEqual(3, node.Variables.Count);
            Assert.AreEqual("Data", node.Variables[0].Name);
            Assert.AreEqual("Value", node.Variables[1].Name);
            Assert.AreEqual("Order", node.Variables[2].Name);
        }
    }
}
