using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small_Loan_Management.Lib
{
    public interface IDataTransfer<T>
    {
        T DataValue { get; set; }
    }
}
