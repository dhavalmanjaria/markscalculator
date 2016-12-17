using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarksCalculator
{
    class TwentyMarkStrategy : ICalcuation
    {
        public int getResults(List<int> values, List<int> maxValues)
        {
            if (values.Count != maxValues.Count)
            {
                try
                {
                    throw new Exception("Count of Values and Maximum Values different in calculation");
                }
                catch (Exception ex)
                {
                    new ExceptionDialog(ex.Message, "").ShowDialog();
                }
            }

            int retVal = 0;
            if (maxValues.Sum() == 0) // prevent DivideByZero
                return 0;
            retVal = (values.Sum() * 20 / maxValues.Sum());

            return retVal;
        }
    }
}
