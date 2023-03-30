﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyScoop.Source.FileHandling.FileTypes
{
    internal class FileTypeWav
    {

        public string Header => @"\x25\x50\x44\x46\x2D"; // PDF signature
        public string Footer => @"\x52\x49\x46\x46\x00\x00\x00\x00\x57\x41\x56\x45\x66\x6D\x74\x20"; // The \x00\x00\x00\x00 is a place holder as file size should be located there

    }
}
