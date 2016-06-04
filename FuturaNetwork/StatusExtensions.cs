using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuturaNetwork
{

    public static class StatusExtensions
    {
        public static bool IsDisconnected(int status)
        {
            if ((status & Constants.Disconnected) > 0)
                return true;
            else
                return false;
        }

        public static bool IsDisabled(int status)
        {
            if ((status & Constants.Disabled) > 0)
                return true;
            else
                return false;
        }

        public static bool IsUnenergizedNotDisabled(int status)
        {
            if (!IsDisabled(status) && IsUnenergized(status))
                return true;
            else
                return false;
        }

        public static bool IsNetJunction(int status)
        {
            if ((status & Constants.NetJunction)>0)
                return true;
            else
                return false;
        }

        public static bool IsUnenergized(int status)
        {
            if ((status & Constants.Unenergized) >0 || (status & Constants.Disconnected) >0)
                return true;
            else
                return false;
        }

        public static bool IsAgainstFlow(int status)
        {
            if ((status&Constants.AgainstFlow)>0)
                return true;
            else
                return false;
        }

        public static bool IsWithFlow(int status)
        {
            if ((status &Constants.WithFlow)>0)
                return true;
            else
                return false;
        }

        public static bool IsEnergized(int status)
        {
            if ((status & Constants.Energized)>0)
                return true;
            else
                return false;
        }

        public static bool IsSource(int status)
        {
            if ((status&Constants.Source)>0)
                return true;
            else
                return false;
        }

        public static bool IsLoop(int status)
        {
            if ((status &Constants.Loop)>0)
                return true;
            else
                return false;
        }

        public static int Connect(int status)
        {
            return (status & (~Constants.Disconnected));
        }

        public static int Unenerize(int status)
        {
            return (status & (~Constants.Energized));
        }

        public static int UnLoop(int status)
        {
            return (status & (~Constants.Loop));
        }

        public static int Energize(int status)
        {
            return (status & (~Constants.Unenergized));
        }

        public static bool IsNetworkProtector(int status)
        {
            if ((status & Constants.NetworkProtector)>0)
                return true;
            else
                return false;
        }

        public static bool IsYDTransformer(int status)
        {
            if ((status & Constants.YDTransformer) > 0)
                return true;
            else
                return false;
        }

        public static List<int> BreakDown(int num)
        {
            List<int> lst = new List<int>();
            for (uint currentPow = 1; currentPow != 0; currentPow <<= 1)
            {
                if ((currentPow & num) != 0)
                    lst.Add((int)currentPow); 
            }
            return lst;
        }
    }

    

}
