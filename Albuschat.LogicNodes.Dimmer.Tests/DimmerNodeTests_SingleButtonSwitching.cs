using LogicModule.Nodes.TestHelper;
using NUnit.Framework;

namespace d_albuschat_gmail_com.logic.Dimmer.Tests
{
    [TestFixture]
    public class DimmerNodeTests_SingleButtonSwitching
    {
        [Test]
        public void When_ButtonDown_AndLightIsOn_Should_TurnLightOff_And_IgnoreButtonUp()
        {
            // Arrange: Create node, lightis off
            var node = new DimmerNode(TestNodeContext.Create(), new MockSchedulerService());
            node.Mode.Value = DimmerNode.MODE_SINGLE_BUTTON_SWITCHING;
            node.LightState.Value = false;

            // Act 1: Press button and release button
            node.ButtonSwitchOnOff.Value = true;
            node.Execute();

            // Assert 1: Light should be toggled on and not dimmed
            Assert.IsTrue(node.SwitchOnOff.Value);

            // Act 2: Press button and release button
            node.ButtonSwitchOnOff.Value = false;
            node.Execute();

            // Assert 2: Light should NOT be toggled again
            Assert.IsTrue(node.SwitchOnOff.Value);
        }

    }
}
