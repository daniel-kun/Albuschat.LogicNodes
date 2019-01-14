using LogicModule.Nodes.Helpers;
using LogicModule.ObjectModel;
using LogicModule.ObjectModel.TypeSystem;
using System;
using System.Collections.Generic;

namespace Albuschat.LogicNodes.Dimmer
{
    public class DimmerNode : LogicNodeBase
    {
        public static string MODE_SINGLE_BUTTON_SWITCHING = "MODE_SINGLE_BUTTON_SWITCHING";
        public static string MODE_SINGLE_BUTTON_DIMMING = "MODE_SINGLE_BUTTON_DIMMING";
        public static string MODE_TWO_BUTTON_DIMMING = "MODE_TWO_BUTTON_DIMMING";
        public static string[] Modes = new string[]
        {
            MODE_SINGLE_BUTTON_SWITCHING,
            MODE_SINGLE_BUTTON_DIMMING,
            MODE_TWO_BUTTON_DIMMING
        };

        public DimmerNode(INodeContext context) : this(context, context.GetService<ISchedulerService>())
        {
        }

        public DimmerNode(INodeContext context, ISchedulerService schedulerService) : base(context)
        {
            context.ThrowIfNull("context");
            schedulerService.ThrowIfNull("schedulerService");
            this._typeService = context.GetService<ITypeService>();
            this._schedulerService = schedulerService;

            this.StartDimDelay = this._typeService.CreateInt(PortTypes.Integer, "StartDimDelay", 400, "ms");
            this.DimPercent = this._typeService.CreateInt(PortTypes.Integer, "DimPercent", 100, "%");
            this.Mode = this._typeService.CreateEnum("BINARYDIMMODE", "Mode", Modes, MODE_SINGLE_BUTTON_SWITCHING);
            this.Mode.ValueSet += (sender, e) => { this.UpdateMode(); };
            UpdateMode();
            this.LightState = this._typeService.CreateBool(PortTypes.Bool, "LightState");
            this.SwitchOnOff = this._typeService.CreateBool(PortTypes.Bool, "SwitchOnOff");
        }

        private void UpdateMode()
        {
            IntValueObject CreateStartDimDelay()
            {
                return this._typeService.CreateInt(PortTypes.Integer, "StartDimDelay", 400, "ms");
            }
            IntValueObject CreateDimPercent()
            {
                return this._typeService.CreateInt(PortTypes.Integer, "DimPercent", 100, "%");
            }

            if (this.Mode.HasValue)
            {
                if (this.Mode.Value == MODE_SINGLE_BUTTON_SWITCHING)
                {
                    this.StartDimDelay = null;
                    this.DimPercent = null;
                    this.ButtonSwitchOnOff = this._typeService.CreateBool(PortTypes.Bool, "ButtonSwitchOnOff");
                    this.ButtonDimUpDown = null;
                    this.ButtonDimUp = null;
                    this.ButtonDimDown = null;
                    this.Dim = null;
                }
                else if (this.Mode.Value == MODE_SINGLE_BUTTON_DIMMING)
                {
                    this.StartDimDelay = CreateStartDimDelay();
                    this.DimPercent = CreateDimPercent();
                    this.ButtonDimUpDown = this._typeService.CreateBool(PortTypes.Bool, "ButtonDimUpDown");
                    this.ButtonSwitchOnOff = null;
                    this.ButtonDimUp = null;
                    this.ButtonDimDown = null;
                    this.Dim = this._typeService.CreateDouble(PortTypes.Number, "Dim");
                }
                else if (this.Mode.Value == MODE_TWO_BUTTON_DIMMING)
                {
                    this.StartDimDelay = CreateStartDimDelay();
                    this.DimPercent = CreateDimPercent();
                    this.ButtonDimUpDown = null;
                    this.ButtonSwitchOnOff = null;
                    this.ButtonDimUp = this._typeService.CreateBool(PortTypes.Bool, "ButtonDimUp");
                    this.ButtonDimDown = this._typeService.CreateBool(PortTypes.Bool, "ButtonDimDown");
                    this.Dim = this._typeService.CreateDouble(PortTypes.Number, "Dim");
                }
            }
        }

        [Parameter(IsRequired = true)]
        public EnumValueObject Mode { get; private set; }

        [Parameter(IsRequired = true)]
        public IntValueObject StartDimDelay { get; private set; }

        [Parameter(IsRequired = true)]
        public IntValueObject DimPercent { get; private set; }

        [Input]
        public BoolValueObject ButtonSwitchOnOff { get; private set; }

        [Input]
        public BoolValueObject ButtonDimUpDown { get; private set; }

        [Input]
        public BoolValueObject ButtonDimUp { get; private set; }

        [Input]
        public BoolValueObject ButtonDimDown { get; private set; }

        [Input]
        public BoolValueObject LightState { get; private set; }

        [Output]
        public BoolValueObject SwitchOnOff { get; private set; }

        [Output]
        public DoubleValueObject Dim { get; private set; }

        public override void Execute()
        {
            if (this.Mode.HasValue)
            {
                if (this.Mode.Value == MODE_SINGLE_BUTTON_SWITCHING)
                {
                    ExecuteButtonSwitchOrDimming(new BoolValueObject[] { this.ButtonSwitchOnOff }, new double?[] { null }, () => { });
                }
                else if (this.Mode.Value == MODE_SINGLE_BUTTON_DIMMING)
                {
                    ExecuteButtonSwitchOrDimming(new BoolValueObject[] { this.ButtonDimUpDown }, new double?[] { _nextDimValue }, () =>
                    {
                        _nextDimValue = _nextDimValue * -1;
                    });
                }
                else if (this.Mode.Value == MODE_TWO_BUTTON_DIMMING)
                {
                    ExecuteButtonSwitchOrDimming(new BoolValueObject[] { this.ButtonDimUp, this.ButtonDimDown }, new double?[] { this.DimPercent.Value, -this.DimPercent.Value }, () => { });
                }
            }
        }

        public void ExecuteButtonSwitchOrDimming(BoolValueObject[]  buttons, double?[] dimValues, Action startDimAction)
        {
            for (uint i = 0; i < buttons.Length; ++i)
            {
                if (!buttons[i].WasSet)
                {
                    continue;
                }
                var newButtonState = buttons[i].HasValue && buttons[i].Value ? ButtonState.Down : ButtonState.Up;
                if (newButtonState != this._buttonState[i])
                {
                    this._buttonState[i] = newButtonState;
                    if (newButtonState == ButtonState.Down)
                    {
                        if (dimValues[i].HasValue)
                        {
                            // Dimming mode:
                            this._lastButtonDown = _schedulerService.Now;
                            this._scheduledDimValue = dimValues[i].Value;
                            this._startDimAction = startDimAction;
                            this._startDimSchedulerToken = this._schedulerService.InvokeIn(TimeSpan.FromMilliseconds(this.StartDimDelay.Value), StartDim);
                        } else
                        {
                            // Switching mode:
                            this.SwitchOnOff.Value = this.LightState.HasValue ? !this.LightState.Value : true;
                        }
                    }
                    else
                    {
                        if (this._startDimSchedulerToken != null)
                        {
                            this._schedulerService.Remove(this._startDimSchedulerToken);
                        }
                        if (this._lastButtonDown != DateTime.MinValue)
                        {
                            var now = _schedulerService.Now;
                            if ((now - this._lastButtonDown).TotalMilliseconds < this.StartDimDelay.Value)
                            {
                                this.SwitchOnOff.Value = this.LightState.HasValue ? ! this.LightState.Value : true;
                            } else
                            {
                                this.Dim.Value = 0.0d;
                            }
                        }
                    }
                }
            }
        }

        private void StartDim()
        {
            this._startDimAction();
            this._startDimSchedulerToken = null;
            this.Dim.Value = this._scheduledDimValue;
        }

        private enum ButtonState
        {
            Up,
            Down
        }

        private ButtonState[] _buttonState = new ButtonState[] { ButtonState.Up, ButtonState.Up };
        private DateTime _lastButtonDown;
        private double _scheduledDimValue = 0.0d;
        private Action _startDimAction = null;
        private double _nextDimValue = 100.0d; // Only for SINGLE_BUTTON Mode
        private ITypeService _typeService;
        private ISchedulerService _schedulerService;
        private SchedulerToken _startDimSchedulerToken = null;


        public override string Localize(string language, string key)
        {
            var translations = new Dictionary<string, Dictionary<string, string>>
            {
                { "de", new Dictionary<string, string>
                    {
                        { MODE_SINGLE_BUTTON_SWITCHING, "Umschalten, 1 Taste" },
                        { MODE_SINGLE_BUTTON_DIMMING, "Umschalten und Dimmen, 1 Taste" },
                        { MODE_TWO_BUTTON_DIMMING, "Umschalten und Dimmen, 2 Tasten" },
                        { "StartDimDelay", "Zeit zwischen Schalten und Dimmen" },
                        { "DimPercent", "Dimmen um..." },
                        { "ButtonSwitchOnOff", "Umschalten" },
                        { "ButtonDimUpDown", "Umschalten oder Dimmen" },
                        { "ButtonDimUp", "Umschalten oder Heller" },
                        { "ButtonDimDown", "Umschalten oder Dunkler" },
                        { "LightState", "Aktueller Schaltzustand" },
                        { "SwitchOnOff", "Schalten" },
                        { "Dim", "Dimmen relativ" },
                        { string.Empty, string.Empty }
                    }
                },
                { "en", new Dictionary<string, string>
                    {
                        { MODE_SINGLE_BUTTON_SWITCHING, "Toggle, 1 button" },
                        { MODE_SINGLE_BUTTON_DIMMING, "Toggle and dim, 1 button" },
                        { MODE_TWO_BUTTON_DIMMING, "Toggle and dim, 2 buttons" },
                        { "StartDimDelay", "Time between switching and dimming" },
                        { "DimPercent", "Dim by..." },
                        { "ButtonSwitchOnOff", "Toggle" },
                        { "ButtonDimUpDown", "Toggle or dim" },
                        { "ButtonDimUp", "Toggle or brighter" },
                        { "ButtonDimDown", "Toggle or darker" },
                        { "LightState", "Current switch state" },
                        { "SwitchOnOff", "Switch" },
                        { "Dim", "Relative dimming" },
                        { string.Empty, string.Empty }
                    }
                }
            };
            if (translations.ContainsKey(language) && translations[language].ContainsKey(key))
            {
                return translations[language][key];
            }
            else if (translations["en"].ContainsKey(key))
            {
                return translations["en"][key];
            }
            else
            {
                return key;
            }
        }


    }
}
