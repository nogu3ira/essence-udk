﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;

namespace MapMakerApplication.Messages
{
    public class MapMakeMessage : MessageBase
    {
        public int Index { get; set; }
    }
}
