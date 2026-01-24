using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ESABM {
    public partial class Form4:Form {
        private bool warningShown = false;
        string connectionString =
                "data source=SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";
        public Form4() {
            InitializeComponent();
        }
        private void LoadBudgetedCategories() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string query = @"
                    SELECT DISTINCT c.CategoryId, c.CategoryName
                    FROM Budget b
                    JOIN Category c ON b.CategoryId = c.CategoryId
                    WHERE b.UserId = @uid
                    AND b.Month = MONTH(GETDATE())
                    AND b.Year = YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(query,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBox1.DataSource=dt;
                comboBox1.DisplayMember="CategoryName";
                comboBox1.ValueMember="CategoryId";
                comboBox1.SelectedIndex=-1;

                if(dt.Rows.Count==0) {
                    MessageBox.Show("No budget found for this month.\nPlease set a budget first.","No Budget",
                        MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    this.Close();
                }
            }
        }

        private void Form4_Load(object sender,EventArgs e) {
            LoadBudgetedCategories();
            dateTimePicker1.MaxDate=DateTime.Today;
        }
        private bool IsCategoryBudgeted(int categoryId) {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
            SELECT COUNT(*) FROM Budget
            WHERE UserId=@uid
              AND CategoryId=@cid
              AND Month=MONTH(GETDATE())
              AND Year=YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                cmd.Parameters.AddWithValue("@cid",categoryId);

                con.Open();
                return (int)cmd.ExecuteScalar()>0;
            }
        }
        private bool IsExpenseAllowed(decimal newAmount) {
            decimal used = GetCurrentMonthExpense();
            decimal budget = GetCurrentMonthBudget();

            if(budget<=0)
                return true;

            decimal percent = ((used+newAmount)/budget)*100;

            if(percent>=80&&!warningShown) {
                MessageBox.Show("⚠ Budget usage exceeded 80%");
                warningShown=true;
            }
            if(percent>=100) {
                MessageBox.Show("❌ Budget exceeded. Expense blocked.");
                return false;
            }

            return true;
        }

        private decimal GetCurrentMonthExpense() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
                    SELECT ISNULL(SUM(Amount),0)
                    FROM Expense
                    WHERE UserId = @uid
                    AND MONTH(ExpenseDate) = MONTH(GETDATE())
                    AND YEAR(ExpenseDate) = YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
        private decimal GetCategoryExpense(int categoryId) {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
                    SELECT ISNULL(SUM(Amount),0)
                    FROM Expense
                    WHERE UserId = @uid
                    AND CategoryId = @cid
                    AND MONTH(ExpenseDate) = MONTH(GETDATE())
                    AND YEAR(ExpenseDate) = YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                cmd.Parameters.AddWithValue("@cid",categoryId);

                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
        private decimal GetCategoryBudget(int categoryId) {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
                    SELECT ISNULL(SUM(Amount),0)
                    FROM Budget
                    WHERE UserId = @uid
                    AND CategoryId = @cid
                    AND Month = MONTH(GETDATE())
                    AND Year = YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                cmd.Parameters.AddWithValue("@cid",categoryId);

                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
        private decimal GetCurrentMonthBudget() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
            SELECT ISNULL(SUM(Amount), 0)
            FROM Budget
            WHERE UserId = @uid
              AND Month = MONTH(GETDATE())
              AND Year = YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                con.Open();
                object result = cmd.ExecuteScalar();

                return Convert.ToDecimal(result);
            }
        }
        private void button1_Click(object sender,EventArgs e) {
            if(!decimal.TryParse(textBox1.Text,out decimal amount)||amount<=0) {
                MessageBox.Show("Invalid amount");
                return;
            }

            if(comboBox1.SelectedIndex==-1) {
                MessageBox.Show("Select category");
                return;
            }

            int categoryId = Convert.ToInt32(comboBox1.SelectedValue);

            decimal used = GetCategoryExpense(categoryId);
            decimal budget = GetCategoryBudget(categoryId);

            if(used+amount>budget) {
                MessageBox.Show("This expense exceeds the budget for this category.","Budget Limit",
                    MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            try {
                using(SqlConnection con = new SqlConnection(connectionString)) {
                    string q = @"INSERT INTO Expense
                        (UserId, CategoryId, Amount, ExpenseDate, Description)
                        VALUES (@uid, @cid, @amt, @date, @desc)";

                    SqlCommand cmd = new SqlCommand(q,con);
                    cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                    cmd.Parameters.AddWithValue("@cid",categoryId);
                    cmd.Parameters.AddWithValue("@amt",amount);
                    cmd.Parameters.AddWithValue("@date",dateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@desc",textBox2.Text.Trim());

                    con.Open();
                    cmd.ExecuteNonQuery();

                    warningShown=false;
                }

                MessageBox.Show("Expense Added Successfully");
                this.Close();
            }
            catch(Exception ex) {
                MessageBox.Show("Error saving expense:\n"+ex.Message);
            }
        }

        private void button2_Click(object sender,EventArgs e) {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender,EventArgs e) {

        }
    }
}
