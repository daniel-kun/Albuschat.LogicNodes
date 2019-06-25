using LogicModule.Nodes.Helpers;
using LogicModule.ObjectModel;
using LogicModule.ObjectModel.TypeSystem;
using System.Net;
using System.Collections.Generic;

namespace d_albuschat_gmail_com.logic.Nodes.WakeOnLan
{
    /// <summary>
    /// A logic node that can wake Wake-On-LAN compatible devices via a Magic Packet (see https://en.wikipedia.org/wiki/Wake-on-LAN)
    /// </summary>
    public class WakeOnLanNode : LogicNodeBase
    {
         public WakeOnLanNode(INodeContext context)
        : base(context)
        {
            context.ThrowIfNull("context");

            // Get the TypeService from the context
            var typeService = context.GetService<ITypeService>();

            // Initialize the ports Input and Output with a double value object and the translateable name "Input" / "Output".
            // The port type 'Number' accepts connections to any other number.
            this.Trigger = typeService.CreateBool(PortTypes.Binary, "Trigger");
            this.MacAddress = typeService.CreateString(PortTypes.String, "MacAddress");
        }


        /// <summary>
        /// A trigger used to execute the Wake-on-LAN.
        /// </summary>
        [Input]
        public BoolValueObject Trigger { get; private set; }

        /// <summary>
        /// The MAC address of the device that should be woken over LAN.
        /// </summary>
        [Input]
        public StringValueObject MacAddress { get; private set; }

        /// <summary>
        /// Wakes the desired device via Wake-on-LAN.
        /// It only executes when Trigger is set to true.
        /// The MAC address must be a valid MAC address in format AA:BB:CC:DD:EE:FF or AABBCCDDEEFF.
        /// </summary>
        public override void Execute()
        {
            byte[] macAddress = new byte[6];
            if (this.MacAddress.HasValue && this.Trigger.HasValue && this.Trigger.Value)
            {
                WakeOnLanImpl.WakeOnLan(IPAddress.Broadcast, this.MacAddress);
            }
        }

        /// <summary>
        /// This function is called only once when the logic page is being loaded.
        /// The base function of this is empty. 
        /// </summary>
        public override void Startup()
        {
            base.Startup();
        }

        /// <summary>
        /// By default this function gets the translation for the node's in- and output from the <see cref="LogicNodeBase.ResourceManager"/>.
        /// A resource file with translation is required for this to work.
        /// </summary>
        /// <param name="language">The requested language, for example "en" or "de".</param>
        /// <param name="key">The key to translate.</param>
        /// <returns>The translation of <paramref name="key"/> in the requested language, or <paramref name="key"/> if the translation is missing.</returns>
        public override string Localize(string language, string key)
        {
            var translations = new Dictionary<string, Dictionary<string, string>>
            {
                { "de", new Dictionary<string, string>
                    {
                        { "Trigger", "Trigger" },
                        { "MacAddress", "MAC-Adresse" },
                    }
                },
                { "en", new Dictionary<string, string>
                    {
                        { "Trigger", "Trigger" },
                        { "MacAddress", "MAC address" },
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
