using LogicModule.Nodes.Helpers;
using LogicModule.ObjectModel;
using LogicModule.ObjectModel.TypeSystem;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Albuschat.LogicNodes.WakeOnLan
{
    /// <summary>
    /// A logic node that can wake Wake-On-LAN compatible devices via a Magic Packet (see https://en.wikipedia.org/wiki/Wake-on-LAN)
    /// </summary>
    public class WakeOnLanNode : LogicNodeBase
    {
        /// <summary>
        /// Returns a list of all ipv4 broadcast addresses for all network adapters.
        /// </summary>
        public static List<IPAddress> GetAllBroadcastAddresses()
        {
            var result = new List<IPAddress>();
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in nics)
            {
                foreach (var unicastAddr in nic.GetIPProperties().UnicastAddresses)
                {
                    if (unicastAddr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var broadcastAddr = ExtractBroadcastAddress(unicastAddr.Address, unicastAddr.IPv4Mask);
                        if (broadcastAddr != null)
                        {
                            result.Add(broadcastAddr);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Given any IP from a network (may be your network adapter's IP, the network itself or the broadcast or multicast address)
        /// and the subnet mask for that network, it returns the network's broadcast address.
        /// </summary>
        /// <param name="ip">Any IP from a network</param>
        /// <param name="netMask">The network's subnet mask</param>
        /// <returns>A broadcast address, or null if an unexpected error occured (i.e. one of the parameters is not correct or contains no valid address).</returns>
        private static IPAddress ExtractBroadcastAddress(IPAddress ip, IPAddress netMask)
        {
            var addr = ip.GetAddressBytes();
            var mask = netMask.GetAddressBytes();
            if (mask.Length == addr.Length)
            {
                var broadcastAddr = new byte[mask.Length];
                for (int i = 0; i < mask.Length; ++i)
                {
                    broadcastAddr[i] =
                        (byte)
                            ((addr[i] & mask[i]) |
                            (mask[i] ^ 0xff));
                }
                return new IPAddress(broadcastAddr);
            }
            return null;
        }

        /// <summary>
        /// Parses an MAC address and puts the result into a byte array.
        /// </summary>
        /// <param name="MacString">A string representation of a MAC address. Can contain : or not, can be lower or upper case.</param>
        /// <param name="MacAddress">If this function returns true, MacAddress will contain 6 bytes that contain the MAC address.</param>
        /// <returns>Returns true when MacString is a valid MAC address and MacAddress has been filled. Returns false when MacString is not correct.</returns>
        private static bool ParseMacAddress(string MacString, out byte[] MacAddress)
        {
            try
            {
                var mac = PhysicalAddress.Parse(MacString.Replace(":", "").Replace("-", "").ToUpper());
                MacAddress = mac.GetAddressBytes();
                return true;
            }
            catch (Exception)
            {
                MacAddress = new byte[] { };
                return false;
            }
        }

        /// <summary>
        /// Sends a Wake-On-Lan packet to the specified MAC address.
        /// </summary>
        /// <param name="mac">Physical MAC address to send WOL packet to.</param>
        public static void WakeOnLan(IPAddress targetIP, string macAddress)
        {
            byte[] mac;
            if (ParseMacAddress(macAddress, out mac))
            {
                // WOL packet is sent over UDP 255.255.255.0:40000.
                using (UdpClient client = new UdpClient())
                {
                    client.Connect(targetIP, 40000);

                    // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
                    byte[] packet = new byte[17 * 6];

                    // Trailer of 6 times 0xFF.
                    for (int i = 0; i < 6; i++)
                        packet[i] = 0xFF;

                    // Body of magic packet contains 16 times the MAC address.
                    for (int i = 1; i <= 16; i++)
                        for (int j = 0; j < 6; j++)
                            packet[i * 6 + j] = mac[j];

                    // Send WOL packet.
                    client.Send(packet, packet.Length);
                }
            }
        }

        public static void WakeOnLan(string macAddress)
        {
            var broadcasts = GetAllBroadcastAddresses();
            foreach (var broadcastAddr in broadcasts)
            {
                try
                {
                    WakeOnLan(broadcastAddr, macAddress);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.NetworkUnreachable)
                    {
                        // NetworkUnreachable might happen for inactive NICs,
                        // so we ignore this here and re-throw everything else.
                        throw;
                    }
                }
            }
        }

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
                WakeOnLan(IPAddress.Broadcast, this.MacAddress);
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
            return base.Localize(language, key);
        }
    }
}
