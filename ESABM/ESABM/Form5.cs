using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ESABM {
    public partial class Form5:Form {
        string connectionString =
            "data source=SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";

        public Form5() {
            InitializeComponent();
        }

        private void Form5_Load(object sender,EventArgs e) {
            string query = "SELECT CategoryId, CategoryName FROM Category";

            SqlDataAdapter da = new SqlDataAdapter(query,connectionString);
            DataTable dt = new DataTable();
            da.Fill(dt);

            comboBox1.DataSource=dt;
            comboBox1.DisplayMember="CategoryName";
            comboBox1.ValueMember="CategoryId";
            comboBox1.SelectedIndex=-1;

            numericUpDown1.Minimum=1;
            numericUpDown1.Maximum=12;
            numericUpDown1.Value=DateTime.Now.Month;

            numericUpDown2.Minimum=2020;
            numericUpDown2.Maximum=2100;
            numericUpDown2.Value=DateTime.Now.Year;
        }
        public bool BudgetSaved {
            get; private set;
        }
        private void button1_Click_1(object sender,EventArgs e) {
            if(comboBox1.SelectedIndex==-1) {
                MessageBox.Show("Select a category");
                return;
            }

            if(!decimal.TryParse(textBox1.Text,out decimal amount)||amount<=0) {
                MessageBox.Show("Invalid budget amount");
                return;
            }

            decimal monthlyIncome = GetMonthlyIncome();
            decimal totalBudgetBefore = GetTotalBudgetForMonth();

            decimal oldAmount = GetExistingBudgetAmount(
                (int)comboBox1.SelectedValue,
                (int)numericUpDown1.Value,
                (int)numericUpDown2.Value
            );

            decimal newTotalBudget = totalBudgetBefore-oldAmount+amount;

            if(newTotalBudget>monthlyIncome) {
                MessageBox.Show(
                    "⚠ Budget amount is bigger than monthly income.\nPlease reduce the amount.",
                    "Budget Limit Exceeded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            if(monthlyIncome<=0) {
                MessageBox.Show(
                    "Please set your monthly income first.",
                    "Income Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            using(SqlConnection con = new SqlConnection(connectionString)) {
                con.Open();

                SqlCommand check = new SqlCommand(@"
            SELECT COUNT(*) FROM Budget
            WHERE UserId=@uid AND CategoryId=@cid
            AND Month=@m AND Year=@y",con);

                check.Parameters.AddWithValue("@uid",UserSession.UserId);
                check.Parameters.AddWithValue("@cid",comboBox1.SelectedValue);
                check.Parameters.AddWithValue("@m",numericUpDown1.Value);
                check.Parameters.AddWithValue("@y",numericUpDown2.Value);

                int exists = (int)check.ExecuteScalar();

                SqlCommand cmd;

                if(exists>0) {
                    cmd=new SqlCommand(@"
                UPDATE Budget SET Amount=@amt
                WHERE UserId=@uid AND CategoryId=@cid
                AND Month=@m AND Year=@y",con);
                }
                else {
                    cmd=new SqlCommand(@"
                INSERT INTO Budget
                (UserId, CategoryId, Month, Year, Amount)
                VALUES (@uid, @cid, @m, @y, @amt)",con);
                }

                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                cmd.Parameters.AddWithValue("@cid",comboBox1.SelectedValue);
                cmd.Parameters.AddWithValue("@m",numericUpDown1.Value);
                cmd.Parameters.AddWithValue("@y",numericUpDown2.Value);
                cmd.Parameters.AddWithValue("@amt",amount);

                cmd.ExecuteNonQuery();
            }

            BudgetSaved=true;
            MessageBox.Show("Budget Saved Successfully");
            this.Close();
        }
        private decimal GetMonthlyIncome() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = "SELECT MonthlyIncome FROM FinancialProfile WHERE UserId=@uid";
                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                con.Open();
                object result = cmd.ExecuteScalar();
                return result==null ? 0 : Convert.ToDecimal(result);
            }
        }
        private decimal GetTotalBudgetForMonth() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
                    SELECT ISNULL(SUM(Amount),0)
                    FROM Budget
                    WHERE UserId=@uid
                    AND Month=@m
                    AND Year=@y";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                cmd.Parameters.AddWithValue("@m",numericUpDown1.Value);
                cmd.Parameters.AddWithValue("@y",numericUpDown2.Value);

                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
        private decimal GetExistingBudgetAmount(int categoryId,int month,int year) {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
            SELECT ISNULL(Amount,0)
            FROM Budget
            WHERE UserId=@uid
            AND CategoryId=@cid
            AND Month=@m
            AND Year=@y";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                cmd.Parameters.AddWithValue("@cid",categoryId);
                cmd.Parameters.AddWithValue("@m",month);
                cmd.Parameters.AddWithValue("@y",year);

                con.Open();
                object result = cmd.ExecuteScalar();
                return result==null ? 0 : Convert.ToDecimal(result);
            }
        }
        private void button2_Click(object sender,EventArgs e) {
            this.Close();
        }

        private void numericUpDown2_ValueChanged(object sender,EventArgs e) {

        }
    }
}
