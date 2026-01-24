using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace ESABM {
    public partial class Form6:Form {
        string connectionString =
            "data source=SDC\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";

        public Form6() {
            InitializeComponent();
        }

        private void Form6_Load(object sender,EventArgs e) {
            LoadExpenses();
        }

        private void LoadExpenses() {
            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q =    @"SELECT e.ExpenseId,
                                       c.CategoryName,
                                       e.Amount,
                                       e.ExpenseDate,
                                       e.Description
                                FROM Expense e
                                JOIN Category c ON e.CategoryId = c.CategoryId
                                WHERE e.UserId = @uid";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource=dt;
                dataGridView1.Columns["ExpenseId"].Visible=false;
            }

            dataGridView1.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void button2_Click(object sender,EventArgs e) {
            if(dataGridView1.CurrentRow==null) {
                MessageBox.Show("Select an expense");
                return;
            }

            int id = Convert.ToInt32(
                dataGridView1.CurrentRow.Cells["ExpenseId"].Value
            );

            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = "DELETE FROM Expense WHERE ExpenseId=@id";
                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@id",id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Expense Deleted");
            LoadExpenses();
        }
        private void button3_Click(object sender,EventArgs e) {
            this.Close();

        }

        private void button1_Click(object sender,EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter="CSV File (*.csv)|*.csv";
            sfd.FileName="Expenses.csv";

            if(sfd.ShowDialog()!=DialogResult.OK)
                return;

            using(SqlConnection con = new SqlConnection(connectionString)) {
                string q = @"
        SELECT Amount, ExpenseDate, Description
        FROM Expense
        WHERE UserId=@uid";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.AddWithValue("@uid",UserSession.UserId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Amount,Date,Description");

                while(dr.Read()) {
                    sb.AppendLine(
                        $"{dr["Amount"]},{Convert.ToDateTime(dr["ExpenseDate"]).ToShortDateString()},{dr["Description"]}"
                    );
                }

                File.WriteAllText(sfd.FileName,sb.ToString());
            }

            MessageBox.Show("Export completed successfully!");
        }
    }
}
