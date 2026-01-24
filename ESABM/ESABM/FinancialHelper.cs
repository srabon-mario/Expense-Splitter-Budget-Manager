using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESABM {
    internal class FinancialHelper {
        static string cs =
            "data source=localhost\\SQLEXPRESS; database=Expense Splitter & Budget Manager; integrated security=SSPI";
        public static (int needs, int wants, int savings) GetIncomeRule(decimal income) {
            using(SqlConnection con = new SqlConnection(cs)) {
                string q = @"SELECT TOP 1 NeedsPercent, WantsPercent, SavingsPercent
                     FROM IncomeRule
                     WHERE @income >= MinIncome AND @income <= MaxIncome";

                SqlCommand cmd = new SqlCommand(q,con);
                cmd.Parameters.Add("@income",SqlDbType.Decimal).Value=income;

                con.Open();
                using(SqlDataReader dr = cmd.ExecuteReader()) {
                    if(dr.Read()) {
                        return (
                            Convert.ToInt32(dr["NeedsPercent"]),
                            Convert.ToInt32(dr["WantsPercent"]),
                            Convert.ToInt32(dr["SavingsPercent"])
                        );
                    }
                }
            }
            return (50, 30, 20);
        }
    }
}
