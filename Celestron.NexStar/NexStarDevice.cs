using System;
using System.Collections.Generic;
using System.Text;

namespace Celestron.NexStar
{
    public enum NexStarDevice : byte
    {
        AZM_RA_Motor = 16,
        ALT_DEC_Motor = 17,
        GPS = 176,
        RTC = 178
    }
}
