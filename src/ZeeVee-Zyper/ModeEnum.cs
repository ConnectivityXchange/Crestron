using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ZeeVee_Zyper
{
    public enum ModeEnum
    {
        none = 0,
        analogAudio,
        hdmiAudio,
        multiview,
        video,
        fastSwitched,
        genlocked,
        genlockedScaled,
        videoWall,
        usb
    }
}