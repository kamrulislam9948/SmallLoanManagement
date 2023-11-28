using Small_Loan_Management.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Small_Loan_Management
{
    public partial class CreateForm : Form
    {
        DataTable dtPayments;
        string img = "";
        public CreateForm()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
            pictureBox1.Image = Image.FromFile(Path.GetFullPath($@"..\..\Pictures\picture-default.jpg"));
        }
        public IFormSync FormToSync { get; set; }
        private void CreateForm_Load(object sender, EventArgs e)
        {
            BuildDataTable();
        }

        private void BuildDataTable()
        {
            using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("SELECT TOP 1 * FROM Payments", con))
                {

                   
                    dtPayments = new DataTable();
                    da.Fill(dtPayments);
                    dtPayments.Rows.Clear();
                    this.dataGridView1.DataSource = dtPayments;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var r = dtPayments.NewRow();
            r["PaymentDate"] = dateTimePicker1.Value;
            r["Amount"] = txtAmount.Text;
            dtPayments.Rows.Add(r);
            txtAmount.Clear();
            dateTimePicker1.Value=DateTime.Now;
        }

        private void cboTenure_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (txtLoanAMount.Text == "" || cboTenure.Text == "") return;
            int t = GetTenure(cboTenure.Text);
            decimal loan = decimal.Parse(txtLoanAMount.Text);
            decimal interest = loan * (t) * .04M;
            decimal payable = loan + interest;
            decimal emi = payable / t;
            lblTotal.Text = payable.ToString("0.00");

            lblInerest.Text = interest.ToString("0.00");
            lblEmi.Text = emi.ToString("0.00");
        }
        private int GetTenure(string tenure)
        {
            if (tenure == "3 months") return 3;
            else if (tenure == "6 months") return 6;
            else if (tenure == "9 months") return 9;
            else if (tenure == "12 months") return 12;
            else return 3;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.EndEdit();
            DataTable dt = dtPayments.GetChanges();
            if (img == "") return;
            string ext = Path.GetExtension(img);
            string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
            string savePath = Path.Combine(Path.GetFullPath(@"..\..\Pictures"), fileName);
            File.Copy(img, savePath);
            using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
            {
                con.Open();
                using (SqlTransaction trx = con.BeginTransaction())
                {
                    try
                    {
                       
                               
                        SqlCommand cmdLoan = new SqlCommand("InsertLoan", con, trx);
                        cmdLoan.CommandType = CommandType.StoredProcedure;
                        cmdLoan.Parameters.AddWithValue("@CustomerName", txtName.Text);
                        cmdLoan.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmdLoan.Parameters.AddWithValue("@Phone", txtPhone.Text);                                         
                        cmdLoan.Parameters.AddWithValue("@Guarantor", txtGuarantor.Text);
                        cmdLoan.Parameters.AddWithValue("@GuarantorAddress", txtGuarantorAddress.Text);
                        cmdLoan.Parameters.AddWithValue("@LoanAmount", txtLoanAMount.Text);
                        cmdLoan.Parameters.AddWithValue("@ApprovalDate", dateTimePicker2.Value.Date);
                        cmdLoan.Parameters.AddWithValue("@Tenure", GetTenure(cboTenure.Text));
                        cmdLoan.Parameters.AddWithValue("@TotalPayable", lblTotal.Text);
                        cmdLoan.Parameters.AddWithValue("@CustomerPicture", fileName);
                        cmdLoan.Parameters.Add(new SqlParameter() { ParameterName = "@id", Direction = ParameterDirection.Output, DbType = DbType.Int32 });
                        cmdLoan.ExecuteNonQuery();
                        int lid = (int)cmdLoan.Parameters["@id"].Value;
                        foreach (DataRow dr in dt.Rows)
                        {
                            //int pid = dr["PaymentId"];
                           
                                string sql = @"INSERT INTO Payments (LoanId, PaymentDate, Amount) VALUES (@l, @d, @a)";
                                SqlCommand cmd = new SqlCommand(sql, con, trx);
                                cmd.Parameters.AddWithValue("@l", lid);
                                cmd.Parameters.AddWithValue("@d", dr["PaymentDate"]);
                                cmd.Parameters.AddWithValue("@a", dr["Amount"]);
                                cmd.ExecuteNonQuery();

                            
                          
                        }
                        trx.Commit();
                        dtPayments.AcceptChanges();
                        MessageBox.Show("Data saved", "Success");
                        pictureBox1.Image = Image.FromFile(Path.GetFullPath($@"..\..\Pictures\picture-default.jpg"));
                        txtName.Clear();
                        txtAddress.Clear();
                        txtPhone.Clear();
                        txtGuarantor.Clear();
                        txtGuarantorAddress.Clear();
                        txtAmount.Clear();
                        dateTimePicker2.Value=DateTime.Now;
                        cboTenure.Text = "";
                        lblInerest.Text = "";
                        lblTotal.Text = "";
                        lblEmi.Text = "";
                        dtPayments.Rows.Clear();
                    }
                    catch (Exception ex)
                    {
                        trx.Rollback();
                        MessageBox.Show(ex.Message, "Error");
                    }

                }
                con.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.img = this.openFileDialog1.FileName;
                pictureBox1.Image = Image.FromFile(this.openFileDialog1.FileName);
            }
        }

        private void CreateForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.FormToSync.Sync();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                dtPayments.Rows[e.RowIndex].Delete();
            }
        }
    }
}
