using Albuschat.LogicNodes.WakeOnLan;
using System;

namespace WoL
{
    /* This is a tiny command-line tool that uses the WakeOnLan LogicNode's implementation to send out a Wake On LAN telegram
     * for the MAC address specified as the first parameter.
     */
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                WakeOnLanNode.WakeOnLan(args[0]);
            }
            else
            {
                Console.WriteLine(@"WoL usage: WoL <Mac-Address>
(c) 2018 - 2019 Daniel Albuschat");
            }
        }
    }
}
