using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using CafePOS.BUS;
using CafePOS.DTO;

namespace CafePOS.GUI
{
    /// <summary>
    /// Màn hình chính: sơ đồ bàn (Button động), gọi món, bớt món, đổi bàn, thanh toán.
    /// </summary>
    public class FrmMain : Form
    {
        private readonly BanAnBUS banBUS = new BanAnBUS();
        private readonly MonAnBUS monBUS = new MonAnBUS();
        private readonly HoaDonBUS hoaDonBUS = new HoaDonBUS();

        private DataSet dsMenu;              // Thực đơn nạp 1 lần (mô hình ngắt kết nối)
        private BanAnDTO banDangChon;
        private List<BanAnDTO> dsBan = new List<BanAnDTO>();

        private FlowLayoutPanel flpBan;
        private Label lblBanDangChon, lblGia, lblTongTien;
        private ComboBox cbDanhMuc, cbMon, cbChuyenBan;
        private NumericUpDown nudSoLuong;
        private Button btnThemMon, btnBotMon, btnThanhToan, btnChuyenBan;
        private DataGridView dgvOrder;

        private static readonly Color MAU_TRONG = Color.FromArgb(46, 160, 67);
        private static readonly Color MAU_CO_KHACH = Color.FromArgb(207, 34, 46);

        public FrmMain()
        {
            Text = "CAFE POS - Quản lý quán cafe & bán hàng";
            Size = new Size(1150, 680);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            TaoGiaoDien();
            Load += (s, e) => TaiDuLieu();
        }

        // ======================= GIAO DIỆN =======================
        private void TaoGiaoDien()
        {
            MenuStrip menu = new MenuStrip();
            menu.Items.Add("Quản lý thực đơn", null, (s, e) => MoQuanLyThucDon());
            menu.Items.Add("Báo cáo doanh thu", null, (s, e) => new FrmBaoCao().ShowDialog(this));
            menu.Items.Add("Làm mới", null, (s, e) => TaiDuLieu());
            menu.Items.Add("Thoát", null, (s, e) => Close());

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));

            // ---- Trái: sơ đồ bàn ----
            GroupBox gbBan = new GroupBox { Text = "Sơ đồ bàn", Dock = DockStyle.Fill };
            flpBan = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(6) };
            FlowLayoutPanel chuThich = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 30 };
            chuThich.Controls.Add(TaoChuThich("Trống", MAU_TRONG));
            chuThich.Controls.Add(TaoChuThich("Có khách", MAU_CO_KHACH));
            gbBan.Controls.Add(flpBan);
            gbBan.Controls.Add(chuThich);

            // ---- Phải: gọi món ----
            GroupBox gbOrder = new GroupBox { Text = "Hóa đơn", Dock = DockStyle.Fill };

            TableLayoutPanel pnlGoiMon = new TableLayoutPanel { Dock = DockStyle.Top, Height = 172, ColumnCount = 4, RowCount = 4, Padding = new Padding(4) };
            pnlGoiMon.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            pnlGoiMon.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            pnlGoiMon.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            pnlGoiMon.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            pnlGoiMon.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            pnlGoiMon.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlGoiMon.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            pnlGoiMon.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            lblBanDangChon = new Label { Text = "Chưa chọn bàn", AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkBlue };
            cbDanhMuc = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cbMon = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            lblGia = new Label { Text = "", AutoSize = true, ForeColor = Color.DimGray, Anchor = AnchorStyles.Left };
            nudSoLuong = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 1, Width = 70 };
            btnThemMon = new Button { Text = "Thêm món", Dock = DockStyle.Fill, BackColor = Color.FromArgb(9, 105, 218), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBotMon = new Button { Text = "Bớt món", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat };

            pnlGoiMon.Controls.Add(lblBanDangChon, 0, 0);
            pnlGoiMon.SetColumnSpan(lblBanDangChon, 4);
            pnlGoiMon.Controls.Add(new Label { Text = "Danh mục:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            pnlGoiMon.Controls.Add(cbDanhMuc, 1, 1);
            pnlGoiMon.Controls.Add(new Label { Text = "Số lượng:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 1);
            pnlGoiMon.Controls.Add(nudSoLuong, 3, 1);
            pnlGoiMon.Controls.Add(new Label { Text = "Món:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            pnlGoiMon.Controls.Add(cbMon, 1, 2);
            pnlGoiMon.Controls.Add(btnThemMon, 2, 2);
            pnlGoiMon.SetColumnSpan(btnThemMon, 2);
            pnlGoiMon.Controls.Add(lblGia, 1, 3);
            pnlGoiMon.Controls.Add(btnBotMon, 2, 3);
            pnlGoiMon.SetColumnSpan(btnBotMon, 2);

            dgvOrder = TaoLuoi();

            TableLayoutPanel pnlThanhToan = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 104, ColumnCount = 3, RowCount = 2, Padding = new Padding(4) };
            pnlThanhToan.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            pnlThanhToan.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            pnlThanhToan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlThanhToan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            pnlThanhToan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            lblTongTien = new Label { Text = "Tổng tiền: 0 đ", AutoSize = true, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = MAU_CO_KHACH, Anchor = AnchorStyles.Left };
            btnThanhToan = new Button { Text = "THANH TOÁN", Dock = DockStyle.Fill, BackColor = MAU_TRONG, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            cbChuyenBan = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            btnChuyenBan = new Button { Text = "Đổi bàn", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat };
            pnlThanhToan.Controls.Add(lblTongTien, 0, 0);
            pnlThanhToan.Controls.Add(btnThanhToan, 1, 0);
            pnlThanhToan.SetColumnSpan(btnThanhToan, 2);
            pnlThanhToan.Controls.Add(new Label { Text = "Chuyển sang bàn trống:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            pnlThanhToan.Controls.Add(cbChuyenBan, 1, 1);
            pnlThanhToan.Controls.Add(btnChuyenBan, 2, 1);

            gbOrder.Controls.Add(dgvOrder);
            gbOrder.Controls.Add(pnlThanhToan);
            gbOrder.Controls.Add(pnlGoiMon);

            root.Controls.Add(gbBan, 0, 0);
            root.Controls.Add(gbOrder, 1, 0);

            Controls.Add(root);
            Controls.Add(menu);
            MainMenuStrip = menu;

            // ---- Sự kiện ----
            cbDanhMuc.SelectedIndexChanged += (s, e) => LocMonTheoDanhMuc();
            cbMon.SelectedIndexChanged += (s, e) => HienGiaMon();
            btnThemMon.Click += (s, e) => ThucHien(ThemMon);
            btnBotMon.Click += (s, e) => ThucHien(BotMon);
            btnThanhToan.Click += (s, e) => ThucHien(ThanhToan);
            btnChuyenBan.Click += (s, e) => ThucHien(ChuyenBan);
        }

        private static Control TaoChuThich(string text, Color mau)
        {
            Label lbl = new Label { Text = "  " + text + "  ", AutoSize = true, BackColor = mau, ForeColor = Color.White, Margin = new Padding(4) };
            return lbl;
        }

        internal static DataGridView TaoLuoi()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
        }

        // ======================= DỮ LIỆU =======================
        private void TaiDuLieu()
        {
            ThucHien(() =>
            {
                dsMenu = monBUS.LoadMenuDataSet();
                cbDanhMuc.DataSource = dsMenu.Tables["DanhMuc"];
                cbDanhMuc.DisplayMember = "TenDanhMuc";
                cbDanhMuc.ValueMember = "MaDanhMuc";
                LocMonTheoDanhMuc();
                VeSoDoBan();
            });
        }

        private void VeSoDoBan()
        {
            dsBan = banBUS.GetAll();
            flpBan.SuspendLayout();
            flpBan.Controls.Clear();
            foreach (BanAnDTO ban in dsBan)
            {
                Button btn = new Button
                {
                    Width = 115,
                    Height = 85,
                    Margin = new Padding(6),
                    Text = ban.TenBan + Environment.NewLine + ban.TrangThai,
                    Tag = ban,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    BackColor = ban.DangTrong ? MAU_TRONG : MAU_CO_KHACH
                };
                bool dangChon = banDangChon != null && banDangChon.MaBan == ban.MaBan;
                btn.FlatAppearance.BorderSize = dangChon ? 4 : 0;
                btn.FlatAppearance.BorderColor = Color.Gold;
                btn.Click += Ban_Click;
                flpBan.Controls.Add(btn);

                if (dangChon) banDangChon = ban; // cập nhật trạng thái mới nhất
            }
            flpBan.ResumeLayout();

            // Danh sách bàn trống để đổi bàn
            cbChuyenBan.DataSource = null;
            cbChuyenBan.DataSource = dsBan.FindAll(b => b.DangTrong && (banDangChon == null || b.MaBan != banDangChon.MaBan));

            HienOrder();
        }

        private void Ban_Click(object sender, EventArgs e)
        {
            banDangChon = (BanAnDTO)((Button)sender).Tag;
            ThucHien(VeSoDoBan);
        }

        private void LocMonTheoDanhMuc()
        {
            if (dsMenu == null) return;
            DataRowView dm = cbDanhMuc.SelectedItem as DataRowView;
            DataView dv = new DataView(dsMenu.Tables["MonAn"]);
            if (dm != null) dv.RowFilter = "MaDanhMuc = " + Convert.ToInt32(dm["MaDanhMuc"]);
            dv.Sort = "TenMon";
            cbMon.DataSource = dv;
            cbMon.DisplayMember = "TenMon";
            cbMon.ValueMember = "MaMon";
            HienGiaMon();
        }

        private void HienGiaMon()
        {
            DataRowView mon = cbMon.SelectedItem as DataRowView;
            lblGia.Text = mon == null ? "" : "Đơn giá: " + Convert.ToDecimal(mon["DonGia"]).ToString("N0") + " đ";
        }

        private void HienOrder()
        {
            if (banDangChon == null)
            {
                lblBanDangChon.Text = "Chưa chọn bàn - hãy bấm vào một bàn bên trái";
                dgvOrder.DataSource = null;
                lblTongTien.Text = "Tổng tiền: 0 đ";
                return;
            }

            lblBanDangChon.Text = banDangChon.TenBan + " - " + banDangChon.TrangThai;
            DataTable dt = hoaDonBUS.GetOrderCuaBan(banDangChon.MaBan);
            dgvOrder.DataSource = dt;
            if (dgvOrder.Columns.Contains("MaMon")) dgvOrder.Columns["MaMon"].Visible = false;
            foreach (string cot in new[] { "Đơn giá", "Thành tiền" })
                if (dgvOrder.Columns.Contains(cot)) dgvOrder.Columns[cot].DefaultCellStyle.Format = "N0";

            object tong = dt.Compute("SUM([Thành tiền])", "");
            decimal tongTien = tong == DBNull.Value ? 0 : Convert.ToDecimal(tong);
            lblTongTien.Text = "Tổng tiền: " + tongTien.ToString("N0") + " đ";
        }

        // ======================= NGHIỆP VỤ =======================
        private int MaMonDangChon()
        {
            DataRowView mon = cbMon.SelectedItem as DataRowView;
            return mon == null ? 0 : Convert.ToInt32(mon["MaMon"]);
        }

        private void ThemMon()
        {
            if (banDangChon == null) throw new ArgumentException("Vui lòng chọn bàn trước khi gọi món.");
            hoaDonBUS.ThemMon(banDangChon.MaBan, MaMonDangChon(), (int)nudSoLuong.Value);
            nudSoLuong.Value = 1;
            VeSoDoBan();
        }

        private void BotMon()
        {
            if (banDangChon == null) throw new ArgumentException("Vui lòng chọn bàn.");
            if (dgvOrder.CurrentRow == null) throw new ArgumentException("Hãy chọn món cần bớt trong lưới hóa đơn.");
            int maMon = Convert.ToInt32(dgvOrder.CurrentRow.Cells["MaMon"].Value);
            hoaDonBUS.BotMon(banDangChon.MaBan, maMon, (int)nudSoLuong.Value);
            VeSoDoBan();
        }

        private void ThanhToan()
        {
            if (banDangChon == null) throw new ArgumentException("Vui lòng chọn bàn cần thanh toán.");
            if (dgvOrder.Rows.Count == 0) throw new InvalidOperationException(banDangChon.TenBan + " chưa gọi món nào.");

            if (MessageBox.Show("Xác nhận thanh toán cho " + banDangChon.TenBan + "?\n" + lblTongTien.Text,
                    "Thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            decimal tong = hoaDonBUS.ThanhToan(banDangChon.MaBan);
            MessageBox.Show("Thanh toán thành công " + banDangChon.TenBan + "\nSố tiền: " + tong.ToString("N0") + " đ",
                "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
            VeSoDoBan();
        }

        private void ChuyenBan()
        {
            if (banDangChon == null) throw new ArgumentException("Vui lòng chọn bàn cần chuyển.");
            BanAnDTO banMoi = cbChuyenBan.SelectedItem as BanAnDTO;
            if (banMoi == null) throw new ArgumentException("Không còn bàn trống để chuyển.");
            hoaDonBUS.ChuyenBan(banDangChon.MaBan, banMoi.MaBan);
            MessageBox.Show("Đã chuyển " + banDangChon.TenBan + " sang " + banMoi.TenBan, "Đổi bàn",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            banDangChon = banMoi;
            VeSoDoBan();
        }

        private void MoQuanLyThucDon()
        {
            using (FrmQuanLyThucDon f = new FrmQuanLyThucDon()) f.ShowDialog(this);
            TaiDuLieu(); // nạp lại DataSet thực đơn sau khi chỉnh sửa
        }

        /// <summary>Bọc mọi thao tác để hiển thị lỗi nghiệp vụ / lỗi CSDL thân thiện.</summary>
        internal static void ThucHien(Action action)
        {
            try
            {
                action();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi CSDL (giao dịch đã được Rollback):\n" + ex.Message, "Lỗi SQL Server",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
