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
using System.Security.Cryptography;


namespace ESABM {
    public partial class Form2:Form {
        public Form2() {
            InitializeComponent();
        }

        private void label2_Click(object sender,EventArgs e) {

        }

        private void button1_Click(object sender,EventArgs e) {
            string connectionString = "data source= SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";

            string id = textBox1.Text.Trim();
            string name = textBox2.Text.Trim();
            string email = textBox3.Text.Trim();
            string password = textBox4.Text.Trim();

            if(string.IsNullOrWhiteSpace(id)||
                string.IsNullOrWhiteSpace(name)||
                string.IsNullOrWhiteSpace(email)||
                string.IsNullOrWhiteSpace(password)) {
                MessageBox.Show("All fields must be filled out.","Validation Error",
                    MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);

            if(password.Length<6||!hasLetter||!hasDigit) {
                MessageBox.Show(
                    "Password must be at least 6 characters and contain letters and numbers.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            if(IsUserIdExists(id)) {
                MessageBox.Show(
                    "This User ID already exists. Please choose a different ID.",
                    "Duplicate User ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            string passwordHash = SecurityHelper.HashPassword(password);

            string query = "INSERT INTO [User] (UserId, Name, Email, PasswordHash) VALUES (@UserId, @Name, @Email, @PasswordHash)";

            using(SqlConnection connection = new SqlConnection(connectionString))
            using(SqlCommand command = new SqlCommand(query,connection)) {
                command.Parameters.AddWithValue("@UserId",id);
                command.Parameters.AddWithValue("@Name",name);
                command.Parameters.AddWithValue("@Email",email);
                command.Parameters.AddWithValue("@PasswordHash",passwordHash);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if(rowsAffected>0) {
                    string fpQuery = @"INSERT INTO FinancialProfile(UserId, MonthlyIncome, NeedsPercent, WantsPercent, SavingsPercent) VALUES (@uid, 0, 0, 0, 0)";

                    using(SqlCommand fpCmd = new SqlCommand(fpQuery,connection)) {
                        fpCmd.Parameters.AddWithValue("@uid",id);
                        fpCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Profile created successfully!","Success",
                        MessageBoxButtons.OK,MessageBoxIcon.Information);

                    this.Close();
                    new Form1().Show();
                }
            }
        }
        private bool IsUserIdExists(string userId) {
            string q = "SELECT COUNT(*) FROM [User] WHERE UserId = @id";

            using(SqlConnection con = new SqlConnection(
                "data source=SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI"))
            using(SqlCommand cmd = new SqlCommand(q,con)) {
                cmd.Parameters.AddWithValue("@id",userId);

                con.Open();
                int count = (int)cmd.ExecuteScalar();

                return count>0;
            }
        }


        private void button2_Click(object sender,EventArgs e) {
            this.Close();
        }

        private void button3_Click(object sender,EventArgs e) {
            Environment.Exit(0);
        }
    }
}
