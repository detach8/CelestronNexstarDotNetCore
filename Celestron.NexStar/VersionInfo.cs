using System;
namespace Celestron.NexStar
{
    public class VersionInfo
    {
        public int Major { get; set; }
        public int Minor { get; set; }

        /// <summary>
        /// New VersionInfo using byte data from NexStar.
        /// </summary>
        /// <param name="data"></param>
        public VersionInfo(byte[] data) : this(data[0], data[1]) { }

        /// <summary>
        /// New VersionInfo using specified major and minor version.
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        public VersionInfo(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }
    }
}
