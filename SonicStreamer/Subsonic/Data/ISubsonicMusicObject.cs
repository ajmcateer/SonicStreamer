﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicStreamer.Subsonic.Data
{
    public interface ISubsonicMusicObject : ISubsonicObject
    {
        CoverArt Cover { get; set; }
    }
}
