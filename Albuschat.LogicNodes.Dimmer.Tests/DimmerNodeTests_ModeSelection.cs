using LogicModule.Nodes.TestHelper;
using NUnit.Framework;

namespace Albuschat.LogicNodes.Dimmer.Tests
{
    [TestFixture]
    public class DimmerNodeTests_ModeSelection
    {
        [Test]
        public void When_ModeIs_MODE_SINGLE_BUTTON_SWITCHING_ShouldOnlyActivateButtonSwitchOnOff_And_SwitchOnOff()
        {
            // Arrange: Create node, lightis off
            var node = new DimmerNode(TestNodeContext.Create(), new MockSchedulerService());
            // Act: Change mode
            node.Mode.Value = DimmerNode.MODE_SINGLE_BUTTON_SWITCHING;
            // Assert: Validate Inputs:
            Assert.IsNull(node.ButtonDimUp);
            Assert.IsNull(node.ButtonDimDown);
            Assert.IsNull(node.ButtonDimUpDown);
            Assert.IsNotNull(node.ButtonSwitchOnOff);
            // Assert: Validate Outputs:
            Assert.IsNotNull(node.SwitchOnOff);
            Assert.IsNull(node.Dim);
        }

        [Test]
        public void When_ModeIs_MODE_SINGLE_BUTTON_DIMMING_ShouldOnlyActivateButtonSwitchOnOff_And_SwitchOnOff()
        {
            // Arrange: Create node, lightis off
            var node = new DimmerNode(TestNodeContext.Create(), new MockSchedulerService());
            // Act: Change mode
            node.Mode.Value = DimmerNode.MODE_SINGLE_BUTTON_DIMMING;
            // Assert: Validate Inputs:
            Assert.IsNull(node.ButtonDimUp);
            Assert.IsNull(node.ButtonDimDown);
            Assert.IsNull(node.ButtonSwitchOnOff);
            Assert.IsNotNull(node.ButtonDimUpDown);
            // Assert: Validate Outputs:
            Assert.IsNotNull(node.SwitchOnOff);
            Assert.IsNotNull(node.Dim);
        }

        [Test]
        public void When_ModeIs_MODE_TWO_BUTTON_DIMMING_ShouldOnlyActivateButtonSwitchOnOff_And_SwitchOnOff()
        {
            // Arrange: Create node, lightis off
            var node = new DimmerNode(TestNodeContext.Create(), new MockSchedulerService());
            // Act: Change mode
            node.Mode.Value = DimmerNode.MODE_TWO_BUTTON_DIMMING;
            // Assert: Validate Inputs:
            Assert.IsNull(node.ButtonSwitchOnOff);
            Assert.IsNull(node.ButtonDimUpDown);
            Assert.IsNotNull(node.ButtonDimUp);
            Assert.IsNotNull(node.ButtonDimDown);
            // Assert: Validate Outputs:
            Assert.IsNotNull(node.SwitchOnOff);
            Assert.IsNotNull(node.Dim);
        }

    }
}
