using System;
using System.Collections.Generic;
using System.Text;

namespace Celestron.NexStar
{
    public class LatLong
    {
        /// <summary>
        /// Latitude in Decimal Degrees
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Latitude in DMS Degrees
        /// </summary>
        public byte LatitudeDegrees { get { return Convert.ToByte(Math.Truncate(Latitude)); } }

        /// <summary>
        /// Latitude in DMS Minutes
        /// </summary>
        public byte LatitudeMinutes { get { return Convert.ToByte(Math.Truncate(60 * (Latitude - LatitudeDegrees))); } }

        /// <summary>
        /// Latitude in DMS Seconds
        /// </summary>
        public byte LatitudeSeconds { get { return Convert.ToByte(Math.Truncate((3600 * (Latitude - LatitudeDegrees)) - (60 * LatitudeMinutes))); } }

        /// <summary>
        /// 0 for North, 1 for South
        /// </summary>
        public byte NorthSouth { get { return Convert.ToByte(Latitude < 0); } }

        /// <summary>
        /// Longitude in Decimal Degrees
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Longitude in DMS Degrees
        /// </summary>
        public byte LongitudeDegrees { get { return Convert.ToByte(Math.Truncate(Longitude)); } }

        /// <summary>
        /// Longitude in DMS Minutes
        /// </summary>
        public byte LongitudeMinutes { get { return Convert.ToByte(Math.Truncate(60 * (Longitude - LongitudeDegrees))); } }

        /// <summary>
        /// Longitude in DMS Seconds
        /// </summary>
        public byte LongitudeSeconds { get { return Convert.ToByte(Math.Truncate((3600 * (Longitude - LongitudeDegrees)) - (60 * LongitudeMinutes))); } }

        /// <summary>
        /// 0 for East, 1 for West
        /// </summary>
        public byte EastWest { get { return Convert.ToByte(Longitude < 0); } }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public LatLong() : this(0, 0) { }

        /// <summary>
        /// New LatLong using DMS byte data from NexStar.
        /// </summary>
        /// <param name="data">Byte data from NexStar</param>
        public LatLong(byte[] data)
        {
            Latitude = Convert.ToDouble(data[0]);
            Latitude += Convert.ToDouble(data[1]) / 60;
            Latitude += Convert.ToDouble(data[2]) / 3600;

            if (data[3] == 1) Latitude *= -1;  // 0 for north, 1 for south

            Longitude = Convert.ToDouble(data[4]);
            Longitude += Convert.ToDouble(data[5]) / 60;
            Longitude += Convert.ToDouble(data[6]) / 3600;

            if (data[7] == 1) Longitude *= -1; // 0 for east, 1 for west
        }

        /// <summary>
        /// New LatLong using decimal degrees.
        /// </summary>
        /// <param name="latitude">Decimal latitude</param>
        /// <param name="longitude">Decimal longitude</param>
        public LatLong(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Formatted string of Latitude and Logitude in decimal degrees.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Latitude},{Longitude}";
        }

        /// <summary>
        /// Formatted string of Latitude and Longitude in DMS format.
        /// </summary>
        public string ToStringDMS()
        {
            return $"{LatitudeDegrees}°{LatitudeMinutes.ToString("D2")}'{LatitudeSeconds.ToString("D2")}\"{(NorthSouth == 0?'N':'S')} {LongitudeDegrees}°{LongitudeMinutes.ToString("D2")}'{LongitudeSeconds.ToString("D2")}\"{(EastWest == 0?'E':'W')}";
        }

        /// <summary>
        /// Byte array used by NexStar for setting location
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return new byte[] {
                LatitudeDegrees, LatitudeMinutes, LatitudeSeconds, NorthSouth,
                LongitudeDegrees, LongitudeMinutes, LongitudeSeconds, EastWest
            };
        }
    }
}
