﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace EP1
{
    public class Frequentador
    {
        public int Index { get; set; }
        public string Id { get; set; }
        public int DestinoX { get; set; }
        public int DestinoY { get; set; }
        public int OrigemX { get; set; }
        public int OrigemY { get; set; }
        public GenericList<Frequentador> Adjacentes { get; set; }
        public int Grau { get; set; }

        public string Status { get; set; } = "S";
    }
}
