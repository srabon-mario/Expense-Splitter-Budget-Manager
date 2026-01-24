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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ESABM {
    public partial class Form1:Form {
        public Form1() {
            InitializeComponent();
        }

        private void label1_Click(object sender,EventArgs e) {

        }

        private void button1_Click(object sender,EventArgs e) {
            string userId = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if(string.IsNullOrWhiteSpace(userId)||string.IsNullOrWhiteSpace(password)) {
                MessageBox.Show("Please enter both Id and Password.",
                    "Validation Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }

            string passwordHash = SecurityHelper.HashPassword(password);

            string connectionString=@"Data Source=SDC\SQLEXPRESS;Initial Catalog=Expense Splitter & Budget Manager;Integrated Security=True;";


            string query = "SELECT COUNT(*) FROM [User] WHERE UserId = @UserId AND PasswordHash = @PasswordHash"; 

            using(SqlConnection connection = new SqlConnection(connectionString))
            using(SqlCommand command = new SqlCommand(query,connection)) {
                command.Parameters.AddWithValue("@UserId",userId);
                command.Parameters.AddWithValue("@PasswordHash",passwordHash);

                connection.Open();
                int count = (int)command.ExecuteScalar();

                if(count>0) {
                    MessageBox.Show("Login successful!",
                        "Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    UserSession.UserId=userId.Trim();
                    this.Hide();
                    new Form3().Show();
                }
                else {
                    MessageBox.Show("Invalid Id or Password.",
                        "Login Failed",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender,EventArgs e) {
            Form2 f2 = new Form2();
            f2.Show();
        }

        private void button3_Click_1(object sender,EventArgs e) {
            Environment.Exit(0);
        }
        private void label5_Click(object sender,EventArgs e) {
            Form7 f7 = new Form7();
            f7.Show();
        }
    }
}
