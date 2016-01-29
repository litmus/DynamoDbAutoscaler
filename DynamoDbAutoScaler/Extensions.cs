using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler
{
   public static class Extensions
    {
        public static int ToPercentage(this long value, long total)
        {
            if (value == 0)
            {
                return 0;
            }

            return (int)Math.Round((double)value / total * 100, 0, MidpointRounding.AwayFromZero);
        }
    }
}
