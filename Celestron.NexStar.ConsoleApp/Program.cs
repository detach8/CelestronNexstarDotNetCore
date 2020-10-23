using System;
using System.IO.Ports;
using System.Reflection.Metadata.Ecma335;
using Celestron.NexStar;

namespace Celestron.NexStar.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var ports = SerialPort.GetPortNames();
            var selectedPort = 0;

            if (ports.Length == 0)
            {
                Console.WriteLine("No serial port(s) found, exiting.");
                return;
            }

            if (ports.Length > 1)
            {
                Console.WriteLine("Serial ports found:");

                for (var i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine($"{i}. {ports[i]}");
                }

                Console.Write("Select a serial port: ");

                selectedPort = int.Parse(System.Console.ReadLine());
            }

            Console.WriteLine($"Connecting using serial port {ports[selectedPort]}...");

            using (var client = new NexStarClient(ports[selectedPort]))
            {
                Console.WriteLine($"Ping: {client.Ping()}");

                client.CancelGoto();

                Console.WriteLine($"Model: {client.GetModel()}");
                Console.WriteLine($"Version: {client.GetVersion()}");
                Console.WriteLine($"ALT/DEC Motor Version: {client.GetDeviceVersion(NexStarDevice.ALT_DEC_Motor)}");
                Console.WriteLine($"AZM/RA Motor Version: {client.GetDeviceVersion(NexStarDevice.AZM_RA_Motor)}");

                try
                {
                    Console.WriteLine($"RTC Version: {client.GetDeviceVersion(NexStarDevice.RTC)}");
                }
                catch (NexStarReadException)
                {
                    Console.WriteLine($"RTC not installed.");
                }

                try
                {
                    Console.WriteLine($"GPS Version: {client.GetDeviceVersion(NexStarDevice.GPS)}");
                }
                catch (NexStarReadException)
                {
                    Console.WriteLine($"GPS not installed.");
                }

                Console.WriteLine("Setting time using local system time...");
                client.SetTime(DateTimeOffset.UtcNow);
                Console.WriteLine($"Date/Time: {client.GetTime()}");

                /*
                System.Console.WriteLine("Setting location using GPS...");
                client.SetLocation(new LatLong(1.267401, 103.8145683));
                */

                var latLong = client.GetLocation();
                Console.WriteLine($"Location (Decimal): {latLong}");
                Console.WriteLine($"Location (DMS): {latLong.ToStringDMS()}");

                Console.WriteLine($"Is Aligned: {client.IsAligned()}");
                Console.WriteLine($"GOTO in Progress: {client.IsGoToInProgress()}");
            }
        }
    }
}
