using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Small_Loan_Management.Reports
{
    public partial class CustomerListForm : Form
    {
        public CustomerListForm()
        {
            InitializeComponent();
        }

        private void CustomerListForm_Load(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
            {

                using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Loans", con))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds, "Loans_1");

                    da.SelectCommand.CommandText = "SELECT * FROM Payments";
                    da.Fill(ds, "Payments");
                    //Add image column
                    ds.Tables["Loans_1"].Columns.Add(new DataColumn("Image", typeof(byte[])));
                    //Fill Image column in rows
                    foreach (DataRow r in ds.Tables["Loans_1"].Rows)
                    {
                        r["Image"] = File.ReadAllBytes(Path.GetFullPath($@"..\..\Pictures\{r["CustomerPicture"]}"));
                    }
                    //
                    CustomersListReport rpt = new CustomersListReport();
                    rpt.SetDataSource(ds);
                    this.crystalReportViewer1.ReportSource = rpt;
                    rpt.Refresh();
                    crystalReportViewer1.Refresh();
                  
                }
            }
        }
    }
}
