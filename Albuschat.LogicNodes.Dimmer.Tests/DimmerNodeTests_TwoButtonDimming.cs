using LogicModule.Nodes.TestHelper;
using LogicModule.ObjectModel;
using LogicModule.ObjectModel.TypeSystem;
using NUnit.Framework;
using System;

namespace d_albuschat_gmail_com.logic.Dimmer.Tests
{
    internal class MockSchedulerService : ISchedulerService
    {
        public DateTime Now { get; set; } = new DateTime(2018, 10, 10, 14, 0, 0, 0);

        public Action<TimeSpan> InvokeInObserver { get; set; }

        public Action InvokeAtAction { get; set; }
        public Action InvokeInAction { get; set; }

        public SchedulerToken InvokeAt(DateTime dueTime, Action action)
        {
            this.InvokeAtAction = action;
            return new SchedulerToken();
        }

        public SchedulerToken InvokeIn(TimeSpan delay, Action action)
        {
            InvokeInAction = action;
            InvokeInObserver?.Invoke(delay);
            return new SchedulerToken();
        }

        public void MockInvokeNow()
        {
            InvokeAtAction?.Invoke();
            InvokeInAction?.Invoke();
            InvokeAtAction = null;
            InvokeInAction = null;
        }

        public bool Remove(SchedulerToken schedulerToken)
        {
            this.InvokeInAction = null;
            this.InvokeAtAction = null;
            return true;
        }
    }

    [TestFixture]
    public class DimmerNodeTests_TwoButtonDimming
    {
        [Test]
        public void When_ButtonUpPressedShortlyWhileOff_Should_ToggleOn()
        {
            // Arrange: Create node, lightis off
            var node = new DimmerNode(TestNodeContext.Create(), new MockSchedulerService());
            node.Mode.Value = DimmerNode.MODE_TWO_BUTTON_DIMMING;
            node.LightState.Value = false;
            // Act: Press button and release button
            node.ButtonDimUp.Value = true;
            node.Execute();
            node.ButtonDimUp.Value = false;
            node.Execute();
            // Assert: Light should be toggled on and not dimmed
            Assert.IsTrue(node.SwitchOnOff.Value);
            Assert.IsFalse(node.Dim.HasValue);
        }

        [Test]
        public void When_ButtonUpPressedShortlyWhileOn_Should_ToggleOff()
        {
            // Arrange: Create node, lightis on
            var node = new DimmerNode(TestNodeContext.Create(), new MockSchedulerService());
            node.Mode.Value = DimmerNode.MODE_TWO_BUTTON_DIMMING;
            node.LightState.Value = true;
            // Act: Press button and release button
            node.ButtonDimUp.Value = true;
            node.Execute();
            node.ButtonDimUp.Value = false;
            node.Execute();
            // Assert: Light should be toggled off and not dimmed
            Assert.IsFalse(node.SwitchOnOff.Value);
            Assert.IsFalse(node.Dim.HasValue);
        }

        private enum UpDown
        {
            Up,
            Down
        };

        [TestCase(UpDown.Up)]
        [TestCase(UpDown.Down)]
        public void When_ButtonUpOrDownPressedLong_ShouldStartDimmingUpOrDown(int upOrDOwn)
        {
            // Arrange: Create node
            var schedMock = new MockSchedulerService();
            var node = new DimmerNode(TestNodeContext.Create(), schedMock);
            node.Mode.Value = DimmerNode.MODE_TWO_BUTTON_DIMMING;
            var testCases = new Tuple<BoolValueObject, double>[] {
                new Tuple<BoolValueObject, double>(node.ButtonDimUp, 100.0d),
                new Tuple<BoolValueObject, double>(node.ButtonDimDown, -100.0d)
            };
            // Act: Press button and release button
            testCases[upOrDOwn].Item1.Value = true;
            node.Execute();
            schedMock.MockInvokeNow();
            // Assert: Light should not be switched on or off, but dimming should be started
            Assert.IsFalse(node.SwitchOnOff.HasValue);
            Assert.AreEqual(testCases[upOrDOwn].Item2, node.Dim.Value);
        }

        [TestCase(UpDown.Up)]
        [TestCase(UpDown.Down)]
        public void When_ButtonUpOrDownReleasedAfterPresedLong_ShouldStopDimming(int upOrDown)
        {
            // Arrange: Create node
            var schedMock = new MockSchedulerService();
            var node = new DimmerNode(TestNodeContext.Create(), schedMock);
            node.Mode.Value = DimmerNode.MODE_TWO_BUTTON_DIMMING;
            var testCases = new Tuple<BoolValueObject, double>[] {
                new Tuple<BoolValueObject, double>(node.ButtonDimUp, 100.0d),
                new Tuple<BoolValueObject, double>(node.ButtonDimDown, -100.0d)
            };
            // Act Phase 2: Press button and release button
            testCases[upOrDown].Item1.Value = true;
            node.Execute();
            schedMock.MockInvokeNow();
            // Assert Phase 1: Light should not be switched on or off, but dimming should be started
            Assert.IsFalse(node.SwitchOnOff.HasValue);
            Assert.AreEqual(testCases[upOrDown].Item2, node.Dim.Value);
            // Act Phase 2: Release button after 800ms
            testCases[upOrDown].Item1.Value = false;
            schedMock.Now = schedMock.Now + TimeSpan.FromMilliseconds(800);
            node.Execute();
            // Assert: Light should not be switched on or off, and dimming should be stoped
            Assert.IsNull(schedMock.InvokeInAction); // No start dimming action should be scheduled
            Assert.IsFalse(node.SwitchOnOff.HasValue);
            Assert.AreEqual(0.0d, node.Dim.Value);
        }

        [TestCase(UpDown.Up, 1, 1, 1)]
        [TestCase(UpDown.Up, 5000, 55, 55)]
        [TestCase(UpDown.Down, 10, 100, -100)]
        [TestCase(UpDown.Down, 10000, 99, -99)]
        public void When_PressedButtonUpOrDownLongWithoutReleaseAfterTimeout_ShouldStopDimmingAfterTimeout(int upOrDown, int startDimDelay, int dimPercent, int expectedDimPercent)
        {
            // Arrange: Create node
            var schedMock = new MockSchedulerService();
            var node = new DimmerNode(TestNodeContext.Create(), schedMock);
            node.Mode.Value = DimmerNode.MODE_TWO_BUTTON_DIMMING;
            node.StartDimDelay.Value = startDimDelay;
            node.DimPercent.Value = dimPercent;
            var testCases = new[] {
                node.ButtonDimUp,
                node.ButtonDimDown
            };
            // Act 1: Press button
            testCases[upOrDown].Value = true;
            int callCounter = 0;
            schedMock.InvokeInObserver = (delay) => {
                ++callCounter;
                Assert.AreEqual(new TimeSpan(0, 0, 0, 0, startDimDelay), delay);
            };
            node.Execute();
            // Assert 1: The correct timeout is applied
            Assert.AreEqual(1, callCounter); // InvokeIn must be called exactly once.
            // Act 2:
            schedMock.MockInvokeNow();
            Assert.AreEqual((double)expectedDimPercent, node.Dim.Value);
            schedMock.Now = schedMock.Now.AddMilliseconds(startDimDelay + 100);
            testCases[upOrDown].Value = false;
            node.Execute();
            // Assert 2:
            
        }

        [TestCase(UpDown.Up)]
        [TestCase(UpDown.Down)]
        public void When_PressedButtonUpOrDownPressedAndReleasedBeforeTimeout_ShouldNotCancelDimmingAfterTimeout(int upOrDown)
        {
            Assert.Inconclusive();
        }
    }
}
