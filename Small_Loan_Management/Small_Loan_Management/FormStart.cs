using Small_Loan_Management.Lib;
using Small_Loan_Management.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Small_Loan_Management
{
    public partial class FormStart : Form, IFormSync
    {
        BindingSource bsL; //= new BindingSource();
        BindingSource bsP;// = new BindingSource();
        DataSet ds;
        public FormStart()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BindingGrid();
            BindControls();
        }

        private void BindingGrid()
        {
            ds = new DataSet();
            bsL = new BindingSource();
            bsP = new BindingSource();
            using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
            {

                using(SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Loans", con))
                {
                    da.Fill(ds, "Loans");
                   
                    da.SelectCommand.CommandText = "SELECT * FROM Payments";
                    da.Fill(ds, "Payments");
                    //Add image column
                    ds.Tables["Loans"].Columns.Add(new DataColumn("Img", typeof(byte[])));
                    //Fill Image column in rows
                    foreach (DataRow r in ds.Tables["Loans"].Rows)
                    {
                        r["Img"] = File.ReadAllBytes(Path.GetFullPath($@"..\..\Pictures\{r["CustomerPicture"]}"));
                    }
                    //
                    ds.Tables["Loans"].PrimaryKey = new DataColumn[] { ds.Tables["Loans"].Columns["LoanId"] };
                    ds.Tables["Payments"].PrimaryKey = new DataColumn[] { ds.Tables["Payments"].Columns["PaymentId"] };
                    ds.Relations.Add(new DataRelation("FK_L_P", ds.Tables["Loans"].Columns["LoanId"], ds.Tables["Payments"].Columns["LoanId"]));
                    bsL.DataSource = ds;
                    bsL.DataMember = "Loans";
                    bsP.DataSource = bsL;
                    bsP.DataMember = "FK_L_P";
                    dgvPayments.DataSource = bsP;
                }
            }
        }

        private void BindControls()
        {
            lblAdd.DataBindings.Clear();
            lblCust.DataBindings.Clear();
            lblAdd.DataBindings.Clear();
            lblPhone.DataBindings.Clear();
            lblGrn.DataBindings.Clear();
            lblGAd.DataBindings.Clear();
            lblAmt.DataBindings.Clear();
            lblApd.DataBindings.Clear();
            lblTnr.DataBindings.Clear();
            lblPayable.DataBindings.Clear();
            lblOtd.DataBindings.Clear();
            pictureBox1.DataBindings.Clear();

            lblCust.DataBindings.Add(new Binding("Text", bsL, "CustomerName"));
            lblAdd.DataBindings.Add(new Binding("Text", bsL, "Address"));
            lblPhone.DataBindings.Add(new Binding("Text", bsL, "Phone"));
            lblGrn.DataBindings.Add(new Binding("Text", bsL, "Guarantor"));
            lblGAd.DataBindings.Add(new Binding("Text", bsL, "GuarantorAddress"));
            Binding bAm = new Binding("Text", bsL, "LoanAmount", true, DataSourceUpdateMode.OnPropertyChanged);
            bAm.Format += BAm_Format;
            lblAmt.DataBindings.Add(bAm);
            
            Binding bApd = new Binding("Text", bsL, "ApprovalDate", true, DataSourceUpdateMode.OnPropertyChanged);
            bApd.Format += BApd_Format;
            lblApd.DataBindings.Add(bApd);
            Binding bT = new Binding("Text", bsL, "Tenure", true, DataSourceUpdateMode.OnPropertyChanged);
            bT.Format += BT_Format;
            lblTnr.DataBindings.Add(bT);
            //lblPayable.Text = ds.Tables["Payments"].AsEnumerable().Sum(r => r.Field<decimal>("Amount")).ToString();
            Binding bP = new Binding("Text", bsL, "TotalPayable", true, DataSourceUpdateMode.OnPropertyChanged);
            bP.Format += BP_Format;
            lblPayable.DataBindings.Add(bP);
            Binding bOst = new Binding("Text", bsL, "TotalPayable", true, DataSourceUpdateMode.OnPropertyChanged);
            bOst.Format += BOst_Format;
            lblOtd.DataBindings.Add(bOst);
            pictureBox1.DataBindings.Add(new Binding("Image", bsL, "Img", true));
        }

        private void BOst_Format(object sender, ConvertEventArgs e)
        {
            decimal v = (decimal)e.Value;
            e.Value = (v - ds.Tables["Payments"].AsEnumerable().Where(r=> r.Field<int>("LoanId")==(int)(bsL.Current as DataRowView).Row["LoanId"]).Sum(r => r.Field<decimal>("Amount"))).ToString("0.00");
        }

        private void BP_Format(object sender, ConvertEventArgs e)
        {
            decimal v = (decimal)e.Value;
            e.Value = v.ToString("0.00");
        }

        private void BT_Format(object sender, ConvertEventArgs e)
        {
            string v =e.Value.ToString();
            e.Value = $"{v} months";
        }

        private void BApd_Format(object sender, ConvertEventArgs e)
        {
            DateTime v = (DateTime)e.Value;
            e.Value = v.ToString("yyyy-MM-dd");
        }

        private void BAm_Format(object sender, ConvertEventArgs e)
        {
            decimal v = (decimal)e.Value;
            e.Value = v.ToString("0.00");
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            bsL.MoveFirst();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            bsL.MoveLast();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (bsL.Position < bsL.Count - 1)
                bsL.MoveNext();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (bsL.Position > 0)
                bsL.MovePrevious();
        }

        public void Sync()
        {
            this.BindingGrid();
            this.BindControls();
        }

        private void toolStripEdit_Click(object sender, EventArgs e)
        {
             int loanId= (int)(this.bsL.Current as DataRowView).Row["LoanId"];
            new EditForm { DataValue = loanId, FormToSync=this }.ShowDialog();
        }

        private void toolStripAdd_Click(object sender, EventArgs e)
        {
            new CreateForm {  FormToSync=this }.ShowDialog();
        }

        private void toolStripDelete_Click(object sender, EventArgs e)
        {
            string msg = "Are you sure to delete?\r\nLoan with payment recors will be deleted?";
            if (MessageBox.Show(msg, "Confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
                {
                    con.Open();
                    using(SqlTransaction trx = con.BeginTransaction())
                    {
                        using (SqlCommand cmd = new SqlCommand("DELETE Payments WHERE LoanId=@lid", con, trx))
                        {
                            cmd.Parameters.AddWithValue("@lid", (int)(bsL.Current as DataRowView).Row["LoanId"]);
                            try
                            {
                                cmd.ExecuteNonQuery();
                                SqlCommand cmd1 = new SqlCommand("DELETE Loans WHERE LoanId=@id", con, trx);
                                cmd1.Parameters.AddWithValue("@id", (int)(bsL.Current as DataRowView).Row["LoanId"]);
                                cmd1.ExecuteNonQuery();
                                trx.Commit();
                                this.Sync();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                MessageBox.Show(ex.Message, "Error");
                                trx.Rollback();
                            }
                        }
                    }
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new CustomerListForm().ShowDialog();

        }

        private void loansByCustomerToolStripMenuItem_Click(object sender, EventArgs e)
        {
           new LoanByCustomerForm().ShowDialog();
        }

        private void subReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SubReportForm().ShowDialog();
        }

        private void subReportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new SubReportForm().ShowDialog();
        }
    }
}
