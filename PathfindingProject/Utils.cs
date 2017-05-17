using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PathfindingProject
{
    public static class Utils
    {
        public static void Swap(ref float first, ref float second)
        {
            float temp = first;
            first = second;
            second = temp;
        }
    }
}
