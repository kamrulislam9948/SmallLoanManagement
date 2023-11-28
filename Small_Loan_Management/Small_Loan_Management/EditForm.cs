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
    public partial class EditForm : Form, IDataTransfer<int>
    {
        DataTable dtPayments;
        string img = "";
        public EditForm()
        {
            InitializeComponent();
            this.dataGridView1.AutoGenerateColumns = false;
        }
        public FormStart FormToSync { get; set; }
        
        public int DataValue { get; set; }

        private void EditForm_Load(object sender, EventArgs e)
        {
            GetLoan();
        }

        private void GetLoan()
        {
            using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Loans WHERE LoanId=@i", con))
                {
                    cmd.Parameters.AddWithValue("@i", this.DataValue);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        cboTenure.Text = dr.GetInt32(dr.GetOrdinal("Tenure")) + " months";
                        txtName.Text = dr.GetString(dr.GetOrdinal("CustomerName"));
                        txtAddress.Text = dr.GetString(dr.GetOrdinal("Address"));
                        txtPhone.Text = dr.GetString(dr.GetOrdinal("Phone"));
                        txtGuarantor.Text = dr.GetString(dr.GetOrdinal("Guarantor"));
                        txtGuarantorAddress.Text = dr.GetString(dr.GetOrdinal("GuarantorAddress"));
                        txtLoanAMount.Text = dr.GetDecimal(dr.GetOrdinal("LoanAmount")).ToString("0.00");
                        dateTimePicker2.Value = dr.GetDateTime(dr.GetOrdinal("ApprovalDate"));
                        lblTotal.Text = dr.GetDecimal(dr.GetOrdinal("TotalPayable")).ToString("0.00");

                        lblInerest.Text = (dr.GetDecimal(dr.GetOrdinal("TotalPayable")) - dr.GetDecimal(dr.GetOrdinal("LoanAmount"))).ToString("0.00");
                        lblEmi.Text = (dr.GetDecimal(dr.GetOrdinal("TotalPayable")) / dr.GetInt32(dr.GetOrdinal("Tenure"))).ToString("0.00");
                        pictureBox1.Image = Image.FromFile(Path.GetFullPath($@"..\..\Pictures\{dr["CustomerPicture"]}"));

                    }
                    con.Close();
                    GetPayments(this.DataValue);
                }
            }
        }

        private void GetPayments(int lid)
        {
            using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Payments WHERE LoanId=@i", con))
                {
                  
                    da.SelectCommand.Parameters.AddWithValue("@i", this.DataValue);
                    dtPayments = new DataTable();
                    da.Fill(dtPayments);
                   this.dataGridView1.DataSource = dtPayments;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
           var r= dtPayments.NewRow();
            r["PaymentDate"] = dateTimePicker1.Value;
            r["Amount"]=txtAmount.Text;
            dtPayments.Rows.Add(r);
        }

        private void cboTenure_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (txtLoanAMount.Text == "") return;
            int t = GetTenure(cboTenure.Text);
            decimal loan = decimal.Parse(txtLoanAMount.Text);
            decimal interest = loan * (t ) * .04M;
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
            
            string fileName = "";
            if (img != "")
            {
                string ext = Path.GetExtension(img);
                fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                string savePath = Path.Combine(Path.GetFullPath(@"..\..\Pictures"), fileName);
                File.Copy(img, savePath);
            }
           
            using (SqlConnection con = new SqlConnection(ConnectionHelper.ConString))
            {
                con.Open();
                using(SqlTransaction trx = con.BeginTransaction())
                {
                    try
                    {
                        string sqlU = @"UPDATE Loans SET CustomerName=@cn, [Address]=@ad,Phone=@ph, Guarantor=@gr, GuarantorAddress=@ga, LoanAmount=@la, 
                            ApprovalDate=@ap, Tenure=@tn, TotalPayable=@ta ";
                        if(img != "")
                        {
                            sqlU += @",CustomerPicture=@pic ";
                        }
                        sqlU += @"WHERE LoanId=@li";
                        SqlCommand cmdLoan = new SqlCommand(sqlU, con, trx);
                        cmdLoan.Parameters.AddWithValue("@cn", txtName.Text);
                        cmdLoan.Parameters.AddWithValue("@ad", txtAddress.Text);
                        cmdLoan.Parameters.AddWithValue("@ph", txtPhone.Text);
                        cmdLoan.Parameters.AddWithValue("@gr", txtGuarantor.Text);
                        cmdLoan.Parameters.AddWithValue("@ga", txtGuarantorAddress.Text);
                        cmdLoan.Parameters.AddWithValue("@la", txtLoanAMount.Text);
                        cmdLoan.Parameters.AddWithValue("@ap", dateTimePicker2.Value.Date);
                        cmdLoan.Parameters.AddWithValue("@tn", GetTenure(cboTenure.Text));
                        cmdLoan.Parameters.AddWithValue("@ta", lblTotal.Text);
                        if(img != "")
                        {
                            cmdLoan.Parameters.AddWithValue("@pic", fileName);
                        }
                        cmdLoan.Parameters.AddWithValue("@li", DataValue);
                        cmdLoan.ExecuteNonQuery();
                        if (dt != null)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                //int pid = dr["PaymentId"];
                                if (dr.RowState == System.Data.DataRowState.Added)
                                {
                                    string sql = @"INSERT INTO Payments (LoanId, PaymentDate, Amount) VALUES (@l, @d, @a)";
                                    SqlCommand cmd = new SqlCommand(sql, con, trx);
                                    cmd.Parameters.AddWithValue("@l", DataValue);
                                    cmd.Parameters.AddWithValue("@d", dr["PaymentDate"]);
                                    cmd.Parameters.AddWithValue("@a", dr["Amount"]);
                                    cmd.ExecuteNonQuery();

                                }
                                if (dr.RowState == System.Data.DataRowState.Modified)
                                {
                                    string sql = @"UPDATE Payments SET LoanId=@l, PaymentDate=@d, Amount=@a WHERE PaymentId=@i";
                                    SqlCommand cmd = new SqlCommand(sql, con, trx);
                                    cmd.Parameters.AddWithValue("@l", DataValue);
                                    cmd.Parameters.AddWithValue("@d", dr["PaymentDate"]);
                                    cmd.Parameters.AddWithValue("@a", dr["Amount"]);
                                    cmd.Parameters.AddWithValue("@i", dr["PaymentId"]);
                                    cmd.ExecuteNonQuery();

                                }
                                if (dr.RowState == DataRowState.Deleted)
                                {
                                    if (dr["PaymentId", DataRowVersion.Original].ToString() != "")
                                    {
                                        string sql = "Delete  Payments WHERE PaymentId=@i";
                                        SqlCommand cmd = new SqlCommand(sql, con, trx);

                                        cmd.Parameters.AddWithValue("@i", dr["PaymentId", DataRowVersion.Original]);
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                            }
                        }
                        trx.Commit();
                        dtPayments.AcceptChanges();
                        MessageBox.Show("Data saved", "Success");
                       
                    }
                    catch(Exception ex)
                    {
                        trx.Rollback();
                        MessageBox.Show(ex.Message, "Error");
                    }
                   
                }
                con.Close();
            }

        }

        private void EditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormToSync.Sync();
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
           if(e.ColumnIndex== 3)
            {
                dtPayments.Rows[e.RowIndex].Delete();
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
    }
}
