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
    public partial class Form8:Form {
        string cs =
            "data source=SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";
        public Form8() {
            InitializeComponent();
        }

        private void button1_Click(object sender,EventArgs e) {
            if(!decimal.TryParse(textBox1.Text,out decimal income)||income<=0) {
                MessageBox.Show("Please enter a valid income amount");
                return;
            }

            var rule = FinancialHelper.GetIncomeRule(income);

            using(SqlConnection con = new SqlConnection(cs)) {
                con.Open();
                string check = "SELECT COUNT(*) FROM FinancialProfile WHERE UserId=@uid";
                SqlCommand chk = new SqlCommand(check,con);
                chk.Parameters.AddWithValue("@uid",UserSession.UserId);

                int exists = (int)chk.ExecuteScalar();

                if(exists==0) {
                    SqlCommand ins = new SqlCommand(@"
                INSERT INTO FinancialProfile
                (UserId, MonthlyIncome, NeedsPercent, WantsPercent, SavingsPercent)
                VALUES (@uid, @i, @n, @w, @s)",con);

                    ins.Parameters.AddWithValue("@uid",UserSession.UserId);
                    ins.Parameters.AddWithValue("@i",income);
                    ins.Parameters.AddWithValue("@n",rule.needs);
                    ins.Parameters.AddWithValue("@w",rule.wants);
                    ins.Parameters.AddWithValue("@s",rule.savings);
                    ins.ExecuteNonQuery();
                }
                else {
                    SqlCommand up = new SqlCommand(@"
                UPDATE FinancialProfile
                SET MonthlyIncome=@i,
                    NeedsPercent=@n,
                    WantsPercent=@w,
                    SavingsPercent=@s
                WHERE UserId=@uid",con);

                    up.Parameters.AddWithValue("@i",income);
                    up.Parameters.AddWithValue("@n",rule.needs);
                    up.Parameters.AddWithValue("@w",rule.wants);
                    up.Parameters.AddWithValue("@s",rule.savings);
                    up.Parameters.AddWithValue("@uid",UserSession.UserId);
                    up.ExecuteNonQuery();
                }

                // Optional: you can delete budget only if you want
                // SqlCommand del = new SqlCommand("DELETE FROM Budget WHERE UserId=@uid AND Month=MONTH(GETDATE()) AND Year=YEAR(GETDATE())",con);
                // del.Parameters.AddWithValue("@uid",UserSession.UserId);
                // del.ExecuteNonQuery();
            }

            MessageBox.Show("Income saved successfully");
            this.Close();
        }
        private void GenerateMonthlyBudget(decimal income,(int needs, int wants, int savings) rule) {
            InsertBudget(1,income*rule.needs/100m);
            InsertBudget(2,income*rule.wants/100m);
            InsertBudget(3,income*rule.savings/100m); 
        }

        private void InsertBudget(int categoryId,decimal amount) {
            using(SqlConnection con = new SqlConnection(cs)) {
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Budget
                    (UserId, CategoryId, Month, Year, Amount)
                    VALUES (@uid, @cid, @m, @y, @amt)",con);

                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);
                cmd.Parameters.AddWithValue("@cid",categoryId);
                cmd.Parameters.AddWithValue("@m",DateTime.Now.Month);
                cmd.Parameters.AddWithValue("@y",DateTime.Now.Year);
                cmd.Parameters.AddWithValue("@amt",amount);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void button2_Click(object sender,EventArgs e) {
            this.Close();
        }
    }
}
