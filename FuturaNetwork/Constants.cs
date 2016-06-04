using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{
    public static class Constants
    {
        public static int SessionID = 1;

        public const int YDTransformer = 1024;
        public const int NetJunction = 512;
        public const int Energized = 1;
        public const int Unenergized = 2;
        public const int Disconnected = 4;
        public const int Source = 8;
        public const int Loop = 16;
        public const int WithFlow = 32;
        public const int AgainstFlow = 64;
        public const int NetworkProtector = 128;
        public const int Disabled = 256;
    }
}
