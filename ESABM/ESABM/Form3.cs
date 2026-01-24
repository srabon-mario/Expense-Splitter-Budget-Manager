using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Collections.Specialized.BitVector32;

namespace ESABM {
    public partial class Form3:Form {
        private decimal monthlyIncome = 0;
        string connectionString =
                "data source=SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";
        public Form3() {
            InitializeComponent();
        }

        decimal totalExpense = 0;
        private void LoadUserName() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = "SELECT Name FROM [User] WHERE UserId=@id";
                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@id",UserSession.UserId);

                con.Open();
                object name = cmd.ExecuteScalar();

                if(name!=null)
                    label1.Text="Welcome, "+name.ToString();
            }
        }

        private void LoadTotalExpense() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"SELECT ISNULL(SUM(Amount),0)
                     FROM Expense
                     WHERE UserId=@id
                     AND MONTH(ExpenseDate)=MONTH(GETDATE())
                     AND YEAR(ExpenseDate)=YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@id",UserSession.UserId);

                con.Open();
                totalExpense=Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
        decimal totalBudget = 0;
        //decimal monthlyIncome;
        int needs, wants, savings;

        private void LoadTotalBudget() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"SELECT ISNULL(SUM(Amount),0)
                     FROM Budget
                     WHERE UserId=@id
                     AND Month=MONTH(GETDATE())
                     AND Year=YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@id",UserSession.UserId);

                con.Open();
                totalBudget=Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
        private void LoadFinancialProfile() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"SELECT MonthlyIncome, NeedsPercent, WantsPercent, SavingsPercent
                     FROM FinancialProfile
                     WHERE UserId=@uid";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                con.Open();
                using(SqlDataReader dr = cmd.ExecuteReader()) {
                    if(dr.Read()) {
                        monthlyIncome=Convert.ToDecimal(dr["MonthlyIncome"]);
                        needs=Convert.ToInt32(dr["NeedsPercent"]);
                        wants=Convert.ToInt32(dr["WantsPercent"]);
                        savings=Convert.ToInt32(dr["SavingsPercent"]);
                    }
                }
            }
        }
        private void LoadCurrentSavings() {
            decimal currentSavings = monthlyIncome-totalExpense;

            if(currentSavings<0)
                currentSavings=0;

            label14.Text=$"{currentSavings:N2} BDT";
        }
        private void UpdateDashboard() {
            label11.Text=$"{totalBudget:N2} BDT";
            label2.Text=$"{monthlyIncome:N2} BDT";
            label12.Text=$"{totalExpense:N2} BDT";
            decimal needsAmt = monthlyIncome*needs/100m;
            decimal wantsAmt = monthlyIncome*wants/100m;
            decimal savingsAmt = monthlyIncome*savings/100m;

            label3.Text=$"{needsAmt:N2} BDT";
            label4.Text=$"{wantsAmt:N2} BDT";
            label7.Text=$"{savingsAmt:N2} BDT";
        }

        private void SetupPieChart() {
            chart1.Series.Clear();
            chart1.Titles.Clear();

            chart1.Titles.Add("Budget Distribution in Pie Chart");

            Series series = new Series {
                ChartType=SeriesChartType.Pie,
                IsValueShownAsLabel=true
            };

            chart1.Series.Add(series);
        }
        private void LoadBudgetPieChart() {
            if(monthlyIncome<=0) return;

            decimal needsAmt = monthlyIncome*needs/100m;
            decimal wantsAmt = monthlyIncome*wants/100m;
            decimal savingsAmt = monthlyIncome*savings/100m;

            var s = chart1.Series[0];
            s.Points.Clear();

            s.Points.AddXY("Needs",needsAmt);
            s.Points.AddXY("Wants",wantsAmt);
            s.Points.AddXY("Savings",savingsAmt);
        }
        private void LoadBudgetAmount() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"SELECT ISNULL(SUM(Amount),0)
                     FROM Budget
                     WHERE UserId=@uid
                     AND Month=MONTH(GETDATE())
                     AND Year=YEAR(GETDATE())";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                con.Open();
                decimal budget = Convert.ToDecimal(cmd.ExecuteScalar());
                label11.Text=$"{budget:N2} BDT";
            }
        }

        private void LoadRemaining() {
            decimal remaining = totalBudget-totalExpense;

            if(remaining<0) {
                MessageBox.Show(
                    "⚠ Budget exceeded!",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void LoadProgress() {
            progressBar1.Maximum=100;

            if(totalBudget<=0) {
                progressBar1.Value=0;
                return;
            }

            int percent = (int)((totalExpense*100)/totalBudget);
            percent=Math.Max(0,Math.Min(percent,100));

            progressBar1.Value=percent;
        }
        private void Form3_Load(object sender,EventArgs e) {
            RefreshDashboard();
        }

        private void button3_Click(object sender,EventArgs e) {
            Environment.Exit(0);
        }

        private void button2_Click(object sender,EventArgs e) {
            UserSession.UserId=null;
            this.Close();
            new Form1().Show();
        }

        private void button1_Click(object sender,EventArgs e) {
            Form4 f4 = new Form4();
            f4.ShowDialog();
            RefreshDashboard();


        }

        private void button4_Click(object sender,EventArgs e) {
            Form6 f6 = new Form6();
            f6.ShowDialog();
            RefreshDashboard();
        }

        private void button5_Click(object sender,EventArgs e) {
            using(Form5 f5 = new Form5()) {
                f5.ShowDialog();
                if(f5.BudgetSaved) {
                    RefreshDashboard();
                }
            }

        }
        private void LoadReport() {
            decimal totalExpense = 0;
            decimal totalBudget = 0;

            using(SqlConnection con = new SqlConnection(connectionString)) {
                con.Open();

                SqlCommand expCmd = new SqlCommand(@"
                    SELECT ISNULL(SUM(Amount),0)
                    FROM Expense
                    WHERE UserId=@uid
                    AND MONTH(ExpenseDate)=MONTH(GETDATE())
                    AND YEAR(ExpenseDate)=YEAR(GETDATE())",con);

                expCmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                totalExpense=Convert.ToDecimal(expCmd.ExecuteScalar());

                SqlCommand budCmd = new SqlCommand(@"
                    SELECT ISNULL(SUM(Amount),0)
                    FROM Budget
                    WHERE UserId=@uid
                    AND Month=MONTH(GETDATE())
                    AND Year=YEAR(GETDATE())",con);

                budCmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                totalBudget=Convert.ToDecimal(budCmd.ExecuteScalar());
            }

            
            label25.Text=$"{totalBudget-totalExpense} BDT";

            if(totalExpense>totalBudget)
                label26.Text="Budget Exceeded ❌";
            else if(totalBudget==0)
                label26.Text="Budget 0.00";
            else
                label26.Text="Within Budget ✅";
        }

        private void lblWelcome_Click(object sender,EventArgs e) {

        }

        private void button6_Click(object sender,EventArgs e) {
            DialogResult result = MessageBox.Show(
        "Are you sure you want to clear this month's budget?",
        "Confirm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );
            if(result!=DialogResult.Yes)
                return;

            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"DELETE FROM Budget
                     WHERE UserId = @uid
                     AND Month = MONTH(GETDATE())
                     AND Year = YEAR(GETDATE())";
                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Budget cleared successfully");
            RefreshDashboard();
        }

        private void button9_Click(object sender,EventArgs e) {
            Form8 f9 = new Form8();
            f9.ShowDialog();

            RefreshDashboard();
        }

        private void button7_Click(object sender,EventArgs e) {
            this.WindowState=FormWindowState.Minimized;
        }

        private void button10_Click(object sender,EventArgs e) {
            RefreshDashboard();
        }
        private void RefreshDashboard() {
            SetupPieChart();

            LoadUserName();
            LoadFinancialProfile();

            LoadTotalExpense();
            LoadTotalBudget();

            LoadReport();
            LoadRemaining();
            LoadProgress();
            LoadCurrentSavings();

            UpdateDashboard();
            LoadBudgetPieChart();
        }
    }
}
