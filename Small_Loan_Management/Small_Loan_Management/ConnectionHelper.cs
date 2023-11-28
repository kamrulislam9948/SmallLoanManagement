using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small_Loan_Management
{
    public class ConnectionHelper
    {
        public static string ConString
        {
            get
            {
                string dbPath = Path.Combine(Path.GetFullPath(@"..\..\"), "smallloandb.mdf");
                return $@"Data Source=(localdb)\mssqllocaldb;AttachDbFilename={dbPath};Initial Catalog=smallloandb;Trusted_Connection=True";
            }
            
            
        }
    }
}
