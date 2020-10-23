using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Celestron.NexStar
{
    public class NexStarClient : IDisposable
    {
        /// <summary>
        /// Enable for debugging read/write functions
        /// </summary>
        private const bool Debug = false;

        /// <summary>
        /// Read terminator character
        /// </summary>
        private const byte ReadTerminator = 35; // '#'

        /// <summary>
        /// Serial port for communication
        /// </summary>
        private SerialPort _serialPort;

        /// <summary>
        /// Connect to a NexStar Hand Controller/Telescope using a serial port.
        /// </summary>
        /// <param name="portName">Serial port name, e.g. COM3 on Windows, /dev/tty.usbserial on MacOS, /dev/ttyUSB1 on Linux</param>
        /// <param name="baudRate">Baud rate, default: 9600</param>
        /// <param name="parity">Parity, default: None</param>
        /// <param name="dataBits">Data bits, default: 8</param>
        /// <param name="stopBits">Stop bits, default: 1</param>
        public NexStarClient(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
            : this(new SerialPort(portName, baudRate, parity, dataBits, stopBits)) { }
        
        /// <summary>
        /// Connect to a NexStar Hand Controller/Telescope using a serial port.
        /// </summary>
        /// <param name="serialPort"></param>
        public NexStarClient(SerialPort serialPort)
        {
            _serialPort = serialPort;
            _serialPort.Handshake = Handshake.None;

            // Specification docs suggests waiting up to 3.5 seconds
            _serialPort.WriteTimeout = 3500;
            _serialPort.ReadTimeout = 3500;

            if (!_serialPort.IsOpen) _serialPort.Open();
        }

        /// <summary>
        /// Get Location
        /// </summary>
        /// <returns>LatLong</returns>
        public LatLong GetLocation()
        {
            _WriteSerial('w');
            return new LatLong(_ReadSerial(8));
        }

        /// <summary>
        /// Set Location
        /// </summary>
        /// <param name="latLong">LatLong</param>
        public void SetLocation(LatLong latLong)
        {
            _WriteSerial('W');
            _WriteSerial(latLong.ToByteArray());
            _ = _ReadSerial(0);
        }

        /// <summary>
        /// Get Date/Time
        /// </summary>
        /// <returns>DateTimeOffset</returns>
        public DateTimeOffset GetTime()
        {
            _WriteSerial('h');
            var data = _ReadSerial(8);

            // Offset is a signed byte (2's complement)
            int offset = data[6];
            if (offset > 127) offset = offset - 256;

            return new DateTimeOffset(data[5] + 2000, data[3], data[4], data[0], data[1], data[2], TimeSpan.FromHours(offset));
        }

        /// <summary>
        /// Set Date/Time
        /// </summary>
        /// <param name="dateTime">DateTimeOffset</param>
        /// <param name="daylightSavings">True to enable Daylight Savings</param>
        public void SetTime(DateTimeOffset dateTime, bool daylightSavings = false)
        {
            // Offset is a signed byte (2's complement)
            int offset = (int) dateTime.Offset.TotalHours;
            if (offset < 0) offset = 256 - offset;

            _WriteSerial('H');
            _WriteSerial(new byte[] {
                (byte)dateTime.Hour,
                (byte)dateTime.Minute,
                (byte)dateTime.Second,
                (byte)dateTime.Month,
                (byte)dateTime.Day,
                (byte)(dateTime.Year - 2000),
                (byte)offset,
                Convert.ToByte(daylightSavings)
            });

            _ = _ReadSerial(0);
        }

        /// <summary>
        /// Get version of the hand controller
        /// </summary>
        /// <returns></returns>
        public VersionInfo GetVersion()
        {
            _WriteSerial('V');
            var data = _ReadSerial(2);
            return new VersionInfo(data);
        }

        /// <summary>
        /// Get version of a device connected to the hand controoller
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns>Version string</returns>
        public VersionInfo GetDeviceVersion(NexStarDevice device)
        {
            // 'P' + 1 + device + 254 + 0 + 0 + 0 + 2
            _WriteSerial('P');
            _WriteSerial(new byte[] { 1, (byte)device, 254, 0, 0, 0, 2 });
            var data = _ReadSerial(2);
            return new VersionInfo(data);
        }

        /// <summary>
        /// Get model
        /// </summary>
        /// <returns>NexStarModel</returns>
        public NexStarModel GetModel()
        {
            _WriteSerial('m');
            return (NexStarModel)_ReadSerial(1)[0];
        }

        /// <summary>
        /// Checks if telescope alignment has been done.
        /// </summary>
        /// <returns>True if aligned, false otherwise.</returns>
        public bool IsAligned()
        {
            _WriteSerial('J');
            return _ReadSerial(1)[0] == 1;
        }

        /// <summary>
        /// Checks if a GOTO operation is in progress.
        /// </summary>
        /// <returns>True if in progress, false otherwise.</returns>
        public bool IsGoToInProgress()
        {
            _WriteSerial('L');
           return _ReadSerial(1)[0] == 49; // ASCII "1"
        }

        /// <summary>
        /// Echo a byte sent, useful for testing communication.
        /// </summary>
        /// <param name="b">Byte to send</param>
        /// <returns>Byte echoed</returns>
        public byte Echo(byte b)
        {
            _WriteSerial('K');
            _WriteSerial(b);

            return _ReadSerial(1)[0];
        }

        /// <summary>
        /// Cancel a GOTO operation that is currently in progress.
        /// </summary>
        public void CancelGoto()
        {
            _WriteSerial('M');
            _ = _ReadSerial(0);
        }

        /// <summary>
        /// Ping if connection is alive (uses Echo feature).
        /// </summary>
        /// <returns>True if connection is alive.</returns>
        public bool Ping()
        {
            return Echo(1) == 1;
        }

        /// <summary>
        /// Write a single character
        /// </summary>
        /// <param name="c"></param>
        private void _WriteSerial(char c)
        {
            _WriteSerial(Convert.ToByte(c));
        }

        /// <summary>
        /// Write a single byte
        /// </summary>
        /// <param name="b"></param>
        private void _WriteSerial(byte b)
        {
            _WriteSerial(new byte[] { b });
        }

        /// <summary>
        /// Write an array of bytes
        /// </summary>
        /// <param name="bytes"></param>
        private void _WriteSerial(byte[] bytes)
        {
            if (Debug) Console.WriteLine($"Writing {bytes.Length} bytes...");

            _serialPort.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Read from serial with expected byte length (excluding terminator byte).
        /// </summary>
        /// <param name="expectedLength"></param>
        /// <returns></returns>
        private byte[] _ReadSerial(int expectedLength)
        {
            var buffer = new List<byte>();

            while (true)
            {
                var b = (byte)_serialPort.ReadByte();
                
                if (Debug) Console.WriteLine($"Read: {b}");

                if (b == ReadTerminator) break;

                buffer.Add(b);
            }

            if (buffer.Count != expectedLength) throw new NexStarReadException($"Length of response ({buffer.Count}) does not match expected length ({expectedLength}).");
            
            return buffer.ToArray();
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Dispose();
        }
    }
}
