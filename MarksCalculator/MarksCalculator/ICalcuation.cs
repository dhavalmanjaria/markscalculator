using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarksCalculator
{
    public interface ICalcuation
    {
        /**
         * For now, it seems like this is the best option -- to have a values and max values list
         */
        int getResults(List<int> values, List<int> maxValues);
    }
}
