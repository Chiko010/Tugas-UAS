using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient; // ensure this using is present

namespace Aplikasi_Manajemen_Bangun_Geometri
{
    public partial class Form1 : Form
    {
        private string connectionString =
            "Server=localhost;Database=db_data_bangun_geometri;Uid=root;Pwd=;";

        private int selectedId = -1;
        private string currentSortColumn = "id";
        private string currentSortOrder = "ASC";

        // ==== TAMBAHAN UNTUK PAGINATION ====
        private int currentPage = 1;
        private int pageSize = 10;
        private int totalPages = 1;
        // ====================================


        public abstract class BangunGeometri
        {
            public int Id { get; set; }
            public string Nama { get; set; }
            public string Tipe { get; set; }

            public double Dimensi1 { get; set; }
            public double Dimensi2 { get; set; }

            public abstract double HitungLuas();
            public abstract double HitungKeliling();
        }


        public class Persegi : BangunGeometri
        {
            public override double HitungLuas()
            {
                return Dimensi1 * Dimensi1;
            }

            public override double HitungKeliling()
            {
                return 4 * Dimensi1;
            }
        }



        public class PersegiPanjang : BangunGeometri
        {
            public override double HitungLuas()
            {
                return Dimensi1 * Dimensi2;
            }

            public override double HitungKeliling()
            {
                return 2 * (Dimensi1 + Dimensi2);
            }
        }



        public class Lingkaran : BangunGeometri
        {
            public override double HitungLuas()
            {
                return Math.PI * Dimensi1 * Dimensi1;
            }

            public override double HitungKeliling()
            {
                return 2 * Math.PI * Dimensi1;
            }
        }



        public class Segitiga : BangunGeometri
        {
            public override double HitungLuas()
            {
                return 0.5 * Dimensi1 * Dimensi2;
            }

            public override double HitungKeliling()
            {
                double sisiMiring =
                    Math.Sqrt(Dimensi1 * Dimensi1 +
                              Dimensi2 * Dimensi2);

                return Dimensi1 + Dimensi2 + sisiMiring;
            }
        }



        public Form1()
        {
            InitializeComponent();

            btnTambah.Click += btnTambah_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnHapus.Click += btnHapus_Click;
            btnBersih.Click += btnBersih_Click;
            btnReset.Click += btnReset_Click;

            // ==== TAMBAHAN: event tombol pagination ====
            btnPrevPage.Click += btnPrevPage_Click;
            btnNextPage.Click += btnNextPage_Click;
            cmbPageSize.SelectedIndexChanged += cmbPageSize_SelectedIndexChanged;
            // =============================================

            txtCari.TextChanged += txtCari_TextChanged;

            cmbFilterTipe.SelectedIndexChanged +=
                cmbFilterTipe_SelectedIndexChanged;

            dgvData.CellClick += dgvData_CellClick;

            dgvData.ColumnHeaderMouseClick +=
                dgvData_ColumnHeaderMouseClick;

            InitializeDatabase();

            cmbFilterTipe.SelectedIndex = 0;
        }



        private void InitializeDatabase()
        {
            try
            {
                string masterConnection =
                    "Server=localhost;Uid=root;Pwd=;";

                using (MySqlConnection conn =
                    new MySqlConnection(masterConnection))
                {
                    conn.Open();

                    string createDb =
                        "CREATE DATABASE IF NOT EXISTS db_geometri";

                    new MySqlCommand(createDb, conn)
                        .ExecuteNonQuery();
                }

                using (MySqlConnection conn =
                    new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS bangun_geometri(
                        id INT PRIMARY KEY AUTO_INCREMENT,
                        nama VARCHAR(100),
                        tipe ENUM(
                        'Persegi',
                        'Persegi Panjang',
                        'Lingkaran',
                        'Segitiga'
                        ),
                        dimensi1 DOUBLE,
                        dimensi2 DOUBLE,
                        luas DOUBLE,
                        keliling DOUBLE
                        );";

                    new MySqlCommand(createTable, conn)
                        .ExecuteNonQuery();

                    string cek =
                        "SELECT COUNT(*) FROM bangun_geometri";

                    long jumlah =
                        Convert.ToInt64(
                        new MySqlCommand(cek, conn)
                        .ExecuteScalar());

                    if (jumlah == 0)
                    {
                        InsertSampleData(conn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                new{
                    nama="Lapangan",
                    tipe="Persegi",
                    d1=10.0,
                    d2=10.0,
                    luas=100.0,
                    kel=40.0
                },

                new{
                    nama="Basket",
                    tipe="Persegi Panjang",
                    d1=28.0,
                    d2=15.0,
                    luas=420.0,
                    kel=86.0
                },

                new{
                    nama="Kolam",
                    tipe="Lingkaran",
                    d1=5.0,
                    d2=5.0,
                    luas=78.54,
                    kel=31.42
                },

                new{
                    nama="Atap",
                    tipe="Segitiga",
                    d1=6.0,
                    d2=4.0,
                    luas=12.0,
                    kel=17.21
                }
            };

            foreach (var item in data)
            {
                MySqlCommand cmd =
                    new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@nama", item.nama);
                cmd.Parameters.AddWithValue("@tipe", item.tipe);
                cmd.Parameters.AddWithValue("@d1", item.d1);
                cmd.Parameters.AddWithValue("@d2", item.d2);
                cmd.Parameters.AddWithValue("@luas", item.luas);
                cmd.Parameters.AddWithValue("@keliling", item.kel);

                cmd.ExecuteNonQuery();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        // ==== TAMBAHAN: hitung total data sesuai filter aktif ====
        private int GetTotalData()
        {
            string query = "SELECT COUNT(*) FROM bangun_geometri WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(txtCari.Text))
                query += " AND nama LIKE @cari";

            if (cmbFilterTipe.SelectedIndex > 0)
                query += " AND tipe=@tipe";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(query, conn);

                if (!string.IsNullOrWhiteSpace(txtCari.Text))
                    cmd.Parameters.AddWithValue("@cari", "%" + txtCari.Text + "%");

                if (cmbFilterTipe.SelectedIndex > 0)
                    cmd.Parameters.AddWithValue("@tipe", cmbFilterTipe.SelectedItem.ToString());

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        // ===========================================================

        private void LoadData()
        {
            try
            {
                // ==== TAMBAHAN: hitung total data & total halaman ====
                int totalData = GetTotalData();

                totalPages = (int)Math.Ceiling((double)totalData / pageSize);
                if (totalPages < 1) totalPages = 1;

                if (currentPage > totalPages) currentPage = totalPages;
                if (currentPage < 1) currentPage = 1;

                int offset = (currentPage - 1) * pageSize;
                // =======================================================

                string query = @"SELECT id, nama, tipe,
                                 dimensi1, dimensi2,
                                 luas, keliling
                                 FROM bangun_geometri
                                 WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(txtCari.Text))
                {
                    query += " AND nama LIKE @cari";
                }

                if (cmbFilterTipe.SelectedIndex > 0)
                {
                    query += " AND tipe=@tipe";
                }

                query += $" ORDER BY {currentSortColumn} {currentSortOrder}";

                // ==== TAMBAHAN: batasi hasil dengan LIMIT & OFFSET ====
                query += " LIMIT @pageSize OFFSET @offset";
                // =========================================================

                using (MySqlConnection conn =
                    new MySqlConnection(connectionString))
                {
                    conn.Open();

                    MySqlCommand cmd =
                        new MySqlCommand(query, conn);

                    if (!string.IsNullOrWhiteSpace(txtCari.Text))
                    {
                        cmd.Parameters.AddWithValue("@cari",
                            "%" + txtCari.Text + "%");
                    }

                    if (cmbFilterTipe.SelectedIndex > 0)
                    {
                        cmd.Parameters.AddWithValue("@tipe",
                            cmbFilterTipe.SelectedItem.ToString());
                    }

                    // ==== TAMBAHAN: parameter pagination ====
                    cmd.Parameters.AddWithValue("@pageSize", pageSize);
                    cmd.Parameters.AddWithValue("@offset", offset);
                    // ===========================================

                    var da = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    dgvData.DataSource = dt;

                    if (dgvData.Columns["id"] != null)
                        dgvData.Columns["id"].Visible = false;

                    dgvData.Columns["nama"].HeaderText = "Nama Bangun";
                    dgvData.Columns["tipe"].HeaderText = "Tipe";
                    dgvData.Columns["dimensi1"].HeaderText = "Dimensi 1";
                    dgvData.Columns["dimensi2"].HeaderText = "Dimensi 2";
                    dgvData.Columns["luas"].HeaderText = "Luas";
                    dgvData.Columns["keliling"].HeaderText = "Keliling";

                    lblStatus.Text =
                        "Total Data : " + totalData;

                    // ==== TAMBAHAN: update info halaman & tombol nav ====
                    lblHalaman.Text = $"Halaman {currentPage} dari {totalPages}";

                    btnPrevPage.Enabled = currentPage > 1;
                    btnNextPage.Enabled = currentPage < totalPages;
                    // =======================================================
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ==== TAMBAHAN: navigasi halaman ====
        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadData();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadData();
            }
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPageSize.SelectedItem == null)
                return;

            if (int.TryParse(cmbPageSize.SelectedItem.ToString(), out int size))
            {
                pageSize = size;
                currentPage = 1;
                LoadData();
            }
        }
        // ========================================



        private BangunGeometri CreateBangunObject()
        {
            if (cmbTipe.SelectedIndex < 0)
                return null;

            double d1;
            double d2 = 0;

            if (!double.TryParse(txtDimensi1.Text, out d1))
                return null;

            string tipe = cmbTipe.SelectedItem.ToString();

            if (tipe == "Persegi Panjang" ||
                tipe == "Segitiga")
            {
                if (!double.TryParse(txtDimensi2.Text, out d2))
                    return null;
            }
            else
            {
                d2 = d1;
            }

            BangunGeometri bangun = null;

            switch (tipe)
            {
                case "Persegi":
                    bangun = new Persegi();
                    break;

                case "Persegi Panjang":
                    bangun = new PersegiPanjang();
                    break;

                case "Lingkaran":
                    bangun = new Lingkaran();
                    break;

                case "Segitiga":
                    bangun = new Segitiga();
                    break;
            }

            bangun.Nama = txtNama.Text;
            bangun.Tipe = tipe;
            bangun.Dimensi1 = d1;
            bangun.Dimensi2 = d2;

            return bangun;
        }



        private void HitungDanTampilkanLuasKeliling()
        {
            BangunGeometri bangun =
                CreateBangunObject();

            if (bangun == null)
            {
                lblLuasValue.Text = "0";
                lblKelilingValue.Text = "0";
                return;
            }

            lblLuasValue.Text =
                bangun.HitungLuas().ToString("F2");

            lblKelilingValue.Text =
                bangun.HitungKeliling().ToString("F2");
        }


        private void ClearForm()
        {
            txtNama.Clear();
            txtDimensi1.Clear();
            txtDimensi2.Clear();

            cmbTipe.SelectedIndex = -1;

            lblLuasValue.Text = "0";
            lblKelilingValue.Text = "0";

            selectedId = -1;

            txtNama.Focus();
        }



        private void cmbTipe_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (cmbTipe.SelectedIndex < 0)
                return;

            string tipe =
                cmbTipe.SelectedItem.ToString();

            if (tipe == "Persegi")
            {
                lblDimensi1.Text = "Sisi :";
                lblDimensi2.Text = "Tidak digunakan";
                txtDimensi2.Clear();
                txtDimensi2.Enabled = false;
            }
            else if (tipe == "Lingkaran")
            {
                lblDimensi1.Text = "Jari-jari :";
                lblDimensi2.Text = "Tidak digunakan";
                txtDimensi2.Clear();
                txtDimensi2.Enabled = false;
            }
            else if (tipe == "Persegi Panjang")
            {
                lblDimensi1.Text = "Panjang :";
                lblDimensi2.Text = "Lebar :";
                txtDimensi2.Enabled = true;
            }
            else
            {
                lblDimensi1.Text = "Alas :";
                lblDimensi2.Text = "Tinggi :";
                txtDimensi2.Enabled = true;
            }

            HitungDanTampilkanLuasKeliling();
        }

        private void InputDimensi_TextChanged(
            object sender,
            EventArgs e)
        {
            HitungDanTampilkanLuasKeliling();
        }


        private void btnTambah_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNama.Text))
                {
                    MessageBox.Show("Nama bangun harus diisi!");
                    txtNama.Focus();
                    return;
                }

                BangunGeometri bangun = CreateBangunObject();

                if (bangun == null)
                {
                    MessageBox.Show("Data dimensi tidak valid!");
                    return;
                }

                string sql = @"INSERT INTO bangun_geometri
                              (nama,tipe,dimensi1,dimensi2,luas,keliling)
                              VALUES
                              (@nama,@tipe,@d1,@d2,@luas,@keliling)";

                using (MySqlConnection conn =
                    new MySqlConnection(connectionString))
                {
                    conn.Open();

                    MySqlCommand cmd =
                        new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@nama", bangun.Nama);
                    cmd.Parameters.AddWithValue("@tipe", bangun.Tipe);
                    cmd.Parameters.AddWithValue("@d1", bangun.Dimensi1);
                    cmd.Parameters.AddWithValue("@d2", bangun.Dimensi2);
                    cmd.Parameters.AddWithValue("@luas", bangun.HitungLuas());
                    cmd.Parameters.AddWithValue("@keliling", bangun.HitungKeliling());

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data berhasil ditambahkan.");

                ClearForm();

                // ==== TAMBAHAN: pindah ke halaman terakhir agar data baru terlihat ====
                currentPage = int.MaxValue; // akan otomatis dikoreksi ke totalPages oleh LoadData()
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Pilih data terlebih dahulu.");
                return;
            }

            try
            {
                BangunGeometri bangun = CreateBangunObject();

                if (bangun == null)
                {
                    MessageBox.Show("Data tidak valid.");
                    return;
                }

                string sql = @"UPDATE bangun_geometri
                               SET
                               nama=@nama,
                               tipe=@tipe,
                               dimensi1=@d1,
                               dimensi2=@d2,
                               luas=@luas,
                               keliling=@keliling
                               WHERE id=@id";

                using (MySqlConnection conn =
                    new MySqlConnection(connectionString))
                {
                    conn.Open();

                    MySqlCommand cmd =
                        new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@id", selectedId);
                    cmd.Parameters.AddWithValue("@nama", bangun.Nama);
                    cmd.Parameters.AddWithValue("@tipe", bangun.Tipe);
                    cmd.Parameters.AddWithValue("@d1", bangun.Dimensi1);
                    cmd.Parameters.AddWithValue("@d2", bangun.Dimensi2);
                    cmd.Parameters.AddWithValue("@luas", bangun.HitungLuas());
                    cmd.Parameters.AddWithValue("@keliling", bangun.HitungKeliling());

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data berhasil diupdate.");

                ClearForm();
                LoadData(); // tetap di halaman yang sama
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Pilih data terlebih dahulu.");
                return;
            }

            DialogResult hasil =
                MessageBox.Show(
                    "Yakin ingin menghapus data ini?",
                    "Konfirmasi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (hasil == DialogResult.No)
                return;

            try
            {
                using (MySqlConnection conn =
                    new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string sql =
                        "DELETE FROM bangun_geometri WHERE id=@id";

                    MySqlCommand cmd =
                        new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@id", selectedId);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data berhasil dihapus.");

                ClearForm();
                LoadData(); // LoadData otomatis mengoreksi currentPage jika halaman jadi kosong
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void btnBersih_Click(object sender, EventArgs e)
        {
            ClearForm();
        }


        private void dgvData_CellClick(object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row =
                dgvData.Rows[e.RowIndex];

            selectedId =
                Convert.ToInt32(row.Cells["id"].Value);

            txtNama.Text =
                row.Cells["nama"].Value.ToString();

            cmbTipe.SelectedItem =
                row.Cells["tipe"].Value.ToString();

            txtDimensi1.Text =
                row.Cells["dimensi1"].Value.ToString();

            if (row.Cells["dimensi2"].Value != DBNull.Value)
            {
                txtDimensi2.Text =
                    row.Cells["dimensi2"].Value.ToString();
            }
            else
            {
                txtDimensi2.Clear();
            }

            HitungDanTampilkanLuasKeliling();
        }


        private void txtCari_TextChanged(object sender, EventArgs e)
        {
            // ==== TAMBAHAN: reset ke halaman 1 saat pencarian berubah ====
            currentPage = 1;
            LoadData();
        }



        private void cmbFilterTipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ==== TAMBAHAN: reset ke halaman 1 saat filter berubah ====
            currentPage = 1;
            LoadData();
        }



        private void btnReset_Click(object sender, EventArgs e)
        {
            txtCari.Clear();

            if (cmbFilterTipe.Items.Count > 0)
                cmbFilterTipe.SelectedIndex = 0;

            currentSortColumn = "id";
            currentSortOrder = "ASC";

            // ==== TAMBAHAN: reset halaman ====
            currentPage = 1;

            LoadData();
        }



        private void dgvData_ColumnHeaderMouseClick(
            object sender,
            DataGridViewCellMouseEventArgs e)
        {
            string columnName =
                dgvData.Columns[e.ColumnIndex].Name;

            if (columnName == "id")
                return;

            if (currentSortColumn == columnName)
            {
                currentSortOrder =
                    currentSortOrder == "ASC"
                    ? "DESC"
                    : "ASC";
            }
            else
            {
                currentSortColumn = columnName;
                currentSortOrder = "ASC";
            }

            // ==== TAMBAHAN: reset ke halaman 1 saat sorting berubah ====
            currentPage = 1;

            LoadData();
        }

    }
}