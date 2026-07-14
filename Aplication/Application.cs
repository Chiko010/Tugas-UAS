using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Aplikasi_Manajemen_Bangun_Geometri
{
    public class BangunGeometriService
    {
        private string connectionString;

        public BangunGeometriService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void InsertData(BangunGeometri bangun)
        {
            string sql = @"INSERT INTO bangun_geometri
                          (nama,tipe,dimensi1,dimensi2,luas,keliling)
                          VALUES
                          (@nama,@tipe,@d1,@d2,@luas,@keliling)";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nama", bangun.Nama);
                cmd.Parameters.AddWithValue("@tipe", bangun.Tipe);
                cmd.Parameters.AddWithValue("@d1", bangun.Dimensi1);
                cmd.Parameters.AddWithValue("@d2", bangun.Dimensi2);
                cmd.Parameters.AddWithValue("@luas", bangun.HitungLuas());
                cmd.Parameters.AddWithValue("@keliling", bangun.HitungKeliling());
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateData(int id, BangunGeometri bangun)
        {
            string sql = @"UPDATE bangun_geometri
                          SET nama=@nama, tipe=@tipe, dimensi1=@d1,
                              dimensi2=@d2, luas=@luas, keliling=@keliling
                          WHERE id=@id";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nama", bangun.Nama);
                cmd.Parameters.AddWithValue("@tipe", bangun.Tipe);
                cmd.Parameters.AddWithValue("@d1", bangun.Dimensi1);
                cmd.Parameters.AddWithValue("@d2", bangun.Dimensi2);
                cmd.Parameters.AddWithValue("@luas", bangun.HitungLuas());
                cmd.Parameters.AddWithValue("@keliling", bangun.HitungKeliling());
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteData(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM bangun_geometri WHERE id=@id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public int GetTotalData(string search, string tipeFilter)
        {
            string query = "SELECT COUNT(*) FROM bangun_geometri WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
                query += " AND nama LIKE @cari";

            if (!string.IsNullOrWhiteSpace(tipeFilter))
                query += " AND tipe=@tipe";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                if (!string.IsNullOrWhiteSpace(search))
                    cmd.Parameters.AddWithValue("@cari", "%" + search + "%");

                if (!string.IsNullOrWhiteSpace(tipeFilter))
                    cmd.Parameters.AddWithValue("@tipe", tipeFilter);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public DataTable GetData(string search, string tipeFilter,
                                 string sortColumn, string sortOrder,
                                 int pageSize, int offset)
        {
            string query = @"SELECT id, nama, tipe, dimensi1, dimensi2, luas, keliling
                           FROM bangun_geometri WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
                query += " AND nama LIKE @cari";

            if (!string.IsNullOrWhiteSpace(tipeFilter))
                query += " AND tipe=@tipe";

            query += $" ORDER BY {sortColumn} {sortOrder}";
            query += " LIMIT @pageSize OFFSET @offset";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                if (!string.IsNullOrWhiteSpace(search))
                    cmd.Parameters.AddWithValue("@cari", "%" + search + "%");

                if (!string.IsNullOrWhiteSpace(tipeFilter))
                    cmd.Parameters.AddWithValue("@tipe", tipeFilter);

                cmd.Parameters.AddWithValue("@pageSize", pageSize);
                cmd.Parameters.AddWithValue("@offset", offset);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}