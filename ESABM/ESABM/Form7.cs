using Microsoft.VisualBasic;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ESABM {
    public partial class Form7:Form {
        string connectionString ="data source=SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";
        public Form7() {
            InitializeComponent();
        }
        private void button3_Click(object sender,EventArgs e) {
            Environment.Exit(0);
        }

        private void button2_Click(object sender,EventArgs e) {
            this.Close();
        }

        private void button1_Click(object sender,EventArgs e) {
            string identifier = textBox1.Text.Trim(); 
            string newPassword = textBox2.Text.Trim();
            string confirmPassword = textBox3.Text.Trim();

            if(string.IsNullOrWhiteSpace(identifier)||string.IsNullOrWhiteSpace(newPassword)||string.IsNullOrWhiteSpace(confirmPassword)) {
                MessageBox.Show("All fields are required");
                return;
            }

            if(newPassword!=confirmPassword) {
                MessageBox.Show("Passwords do not match");
                return;
            }

            bool hasLetter = newPassword.Any(char.IsLetter);
            bool hasDigit = newPassword.Any(char.IsDigit);

            if(newPassword.Length<6||!hasLetter||!hasDigit) {
                MessageBox.Show("Password must be at least 6 characters and contain letters & numbers");
                return;
            }

            string checkQuery = @"
                SELECT COUNT(*) 
                FROM [User]
                WHERE UserId = @id OR Email = @id";

            using(SqlConnection con = new SqlConnection(connectionString))
            using(SqlCommand cmd = new SqlCommand(checkQuery,con)) {
                cmd.Parameters.AddWithValue("@id",identifier);

                con.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if(count==0) {
                    MessageBox.Show("User not found");
                    return;
                }
            }
            string passwordHash = SecurityHelper.HashPassword(newPassword);

            string updateQuery =
                "UPDATE [User] SET PasswordHash = @pass WHERE UserId = @id OR Email = @id";

            using(SqlConnection con = new SqlConnection(connectionString))
            using(SqlCommand cmd = new SqlCommand(updateQuery,con)) {
                cmd.Parameters.AddWithValue("@pass",passwordHash);
                cmd.Parameters.AddWithValue("@id",identifier);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Password reset successfully","Success");

            this.Close();
            new Form1().Show();
        }
    }
}
