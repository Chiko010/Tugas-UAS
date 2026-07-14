using System;
using MySql.Data.MySqlClient;

namespace Aplikasi_Manajemen_Bangun_Geometri
{
    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void InitializeDatabase()
        {
            try
            {
                string masterConnection = "Server=localhost;Uid=root;Pwd=;";

                using (MySqlConnection conn = new MySqlConnection(masterConnection))
                {
                    conn.Open();
                    string createDb = "CREATE DATABASE IF NOT EXISTS db_data_bangun_geometri";
                    new MySqlCommand(createDb, conn).ExecuteNonQuery();
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS bangun_geometri(
                        id INT PRIMARY KEY AUTO_INCREMENT,
                        nama VARCHAR(100),
                        tipe ENUM('Persegi','Persegi Panjang','Lingkaran','Segitiga'),
                        dimensi1 DOUBLE,
                        dimensi2 DOUBLE,
                        luas DOUBLE,
                        keliling DOUBLE
                        );";

                    new MySqlCommand(createTable, conn).ExecuteNonQuery();

                    string cek = "SELECT COUNT(*) FROM bangun_geometri";
                    long jumlah = Convert.ToInt64(new MySqlCommand(cek, conn).ExecuteScalar());

                    if (jumlah == 0)
                    {
                        InsertSampleData(conn);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database Error: " + ex.Message);
            }
        }

        private void InsertSampleData(MySqlConnection conn)
        {
            string sql = @"
                INSERT INTO bangun_geometri
                (nama,tipe,dimensi1,dimensi2,luas,keliling)
                VALUES
                (@nama,@tipe,@d1,@d2,@luas,@keliling);";

            var data = new[]
            {
                new { nama="Lapangan", tipe="Persegi", d1=10.0, d2=10.0, luas=100.0, kel=40.0 },
                new { nama="Basket", tipe="Persegi Panjang", d1=28.0, d2=15.0, luas=420.0, kel=86.0 },
                new { nama="Kolam", tipe="Lingkaran", d1=5.0, d2=5.0, luas=78.54, kel=31.42 },
                new { nama="Atap", tipe="Segitiga", d1=6.0, d2=4.0, luas=12.0, kel=17.21 }
            };

            foreach (var item in data)
            {
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nama", item.nama);
                cmd.Parameters.AddWithValue("@tipe", item.tipe);
                cmd.Parameters.AddWithValue("@d1", item.d1);
                cmd.Parameters.AddWithValue("@d2", item.d2);
                cmd.Parameters.AddWithValue("@luas", item.luas);
                cmd.Parameters.AddWithValue("@keliling", item.kel);
                cmd.ExecuteNonQuery();
            }
        }
    }
}