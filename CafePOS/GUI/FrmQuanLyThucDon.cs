using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CafePOS.BUS;
using CafePOS.DTO;

namespace CafePOS.GUI
{
    /// <summary>Quản lý danh mục & thực đơn (Thêm / Sửa / Xóa / Tìm kiếm) theo 3 lớp GUI → BUS → DAL.</summary>
    public class FrmQuanLyThucDon : Form
    {
        private readonly MonAnBUS monBUS = new MonAnBUS();
        private readonly DanhMucBUS dmBUS = new DanhMucBUS();

        // Tab món
        private DataGridView dgvMon;
        private TextBox txtMaMon, txtTenMon, txtDonGia, txtTimKiem;
        private ComboBox cbDanhMucMon;

        // Tab danh mục
        private DataGridView dgvDanhMuc;
        private TextBox txtMaDM, txtTenDM;

        public FrmQuanLyThucDon()
        {
            Text = "Quản lý thực đơn";
            Size = new Size(950, 560);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10F);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(TaoTabMon());
            tabs.TabPages.Add(TaoTabDanhMuc());
            Controls.Add(tabs);

            Load += (s, e) => FrmMain.ThucHien(() => { TaiDanhMuc(); TaiMon(); });
        }

        // ======================= TAB MÓN =======================
        private TabPage TaoTabMon()
        {
            TabPage tab = new TabPage("Thực đơn món");

            dgvMon = FrmMain.TaoLuoi();
            dgvMon.SelectionChanged += (s, e) => ChonMon();

            Panel pnlTim = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(4) };
            txtTimKiem = new TextBox { Width = 300, Location = new Point(110, 8) };
            Button btnTim = new Button { Text = "Tìm", Location = new Point(420, 6), Width = 80 };
            pnlTim.Controls.Add(new Label { Text = "Tìm theo tên:", AutoSize = true, Location = new Point(4, 11) });
            pnlTim.Controls.Add(txtTimKiem);
            pnlTim.Controls.Add(btnTim);
            btnTim.Click += (s, e) => FrmMain.ThucHien(TaiMon);
            txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) FrmMain.ThucHien(TaiMon); };

            TableLayoutPanel pnlNhap = TaoKhungNhap();
            txtMaMon = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            txtTenMon = new TextBox { Dock = DockStyle.Fill };
            cbDanhMucMon = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            txtDonGia = new TextBox { Dock = DockStyle.Fill };
            ThemDong(pnlNhap, "Mã món:", txtMaMon);
            ThemDong(pnlNhap, "Tên món:", txtTenMon);
            ThemDong(pnlNhap, "Danh mục:", cbDanhMucMon);
            ThemDong(pnlNhap, "Đơn giá:", txtDonGia);
            ThemNut(pnlNhap,
                TaoNut("Thêm", () => { monBUS.Them(DocMon()); ThongBao("Đã thêm món."); TaiMon(); }),
                TaoNut("Sửa", () => { monBUS.Sua(DocMon()); ThongBao("Đã cập nhật món."); TaiMon(); }),
                TaoNut("Xóa", XoaMon),
                TaoNut("Làm mới", () => { txtMaMon.Clear(); txtTenMon.Clear(); txtDonGia.Clear(); txtTenMon.Focus(); }));

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 560 };
            split.Panel1.Controls.Add(dgvMon);
            split.Panel1.Controls.Add(pnlTim);
            split.Panel2.Controls.Add(pnlNhap);
            tab.Controls.Add(split);
            return tab;
        }

        private void TaiMon()
        {
            dgvMon.DataSource = monBUS.TimKiem(txtTimKiem.Text);
            if (dgvMon.Columns.Contains("MaDanhMuc")) dgvMon.Columns["MaDanhMuc"].Visible = false;
            if (dgvMon.Columns.Contains("DonGia")) dgvMon.Columns["DonGia"].DefaultCellStyle.Format = "N0";
        }

        private void ChonMon()
        {
            if (dgvMon.CurrentRow == null) return;
            DataRowView r = dgvMon.CurrentRow.DataBoundItem as DataRowView;
            if (r == null) return;
            txtMaMon.Text = r["MaMon"].ToString();
            txtTenMon.Text = r["TenMon"].ToString();
            cbDanhMucMon.SelectedValue = r["MaDanhMuc"];
            txtDonGia.Text = Convert.ToDecimal(r["DonGia"]).ToString("0");
        }

        private MonAnDTO DocMon()
        {
            decimal gia;
            if (!decimal.TryParse(txtDonGia.Text.Trim().Replace(",", "").Replace(".", ""), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out gia))
                throw new ArgumentException("Đơn giá phải là số (ví dụ: 25000).");

            int maMon;
            int.TryParse(txtMaMon.Text, out maMon);
            DanhMucDTO dm = cbDanhMucMon.SelectedItem as DanhMucDTO;
            return new MonAnDTO
            {
                MaMon = maMon,
                TenMon = txtTenMon.Text,
                MaDanhMuc = dm == null ? 0 : dm.MaDanhMuc,
                DonGia = gia
            };
        }

        private void XoaMon()
        {
            int maMon;
            int.TryParse(txtMaMon.Text, out maMon);
            if (maMon <= 0) throw new ArgumentException("Vui lòng chọn món cần xóa.");
            if (MessageBox.Show("Xóa món \"" + txtTenMon.Text + "\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            monBUS.Xoa(maMon);
            ThongBao("Đã xóa món.");
            TaiMon();
        }

        // ======================= TAB DANH MỤC =======================
        private TabPage TaoTabDanhMuc()
        {
            TabPage tab = new TabPage("Danh mục");
            dgvDanhMuc = FrmMain.TaoLuoi();
            dgvDanhMuc.SelectionChanged += (s, e) =>
            {
                DanhMucDTO dm = dgvDanhMuc.CurrentRow == null ? null : dgvDanhMuc.CurrentRow.DataBoundItem as DanhMucDTO;
                if (dm == null) return;
                txtMaDM.Text = dm.MaDanhMuc.ToString();
                txtTenDM.Text = dm.TenDanhMuc;
            };

            TableLayoutPanel pnlNhap = TaoKhungNhap();
            txtMaDM = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            txtTenDM = new TextBox { Dock = DockStyle.Fill };
            ThemDong(pnlNhap, "Mã danh mục:", txtMaDM);
            ThemDong(pnlNhap, "Tên danh mục:", txtTenDM);
            ThemNut(pnlNhap,
                TaoNut("Thêm", () => { dmBUS.Them(new DanhMucDTO { TenDanhMuc = txtTenDM.Text }); ThongBao("Đã thêm danh mục."); TaiDanhMuc(); }),
                TaoNut("Sửa", () => { dmBUS.Sua(new DanhMucDTO { MaDanhMuc = MaDM(), TenDanhMuc = txtTenDM.Text }); ThongBao("Đã cập nhật."); TaiDanhMuc(); TaiMon(); }),
                TaoNut("Xóa", () =>
                {
                    if (MaDM() <= 0) throw new ArgumentException("Vui lòng chọn danh mục cần xóa.");
                    if (MessageBox.Show("Xóa danh mục \"" + txtTenDM.Text + "\"?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                    dmBUS.Xoa(MaDM()); ThongBao("Đã xóa danh mục."); TaiDanhMuc();
                }),
                TaoNut("Làm mới", () => { txtMaDM.Clear(); txtTenDM.Clear(); txtTenDM.Focus(); }));

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 480 };
            split.Panel1.Controls.Add(dgvDanhMuc);
            split.Panel2.Controls.Add(pnlNhap);
            tab.Controls.Add(split);
            return tab;
        }

        private int MaDM()
        {
            int ma;
            int.TryParse(txtMaDM.Text, out ma);
            return ma;
        }

        private void TaiDanhMuc()
        {
            var ds = dmBUS.GetAll();
            dgvDanhMuc.DataSource = ds;
            cbDanhMucMon.DataSource = dmBUS.GetAll();
            cbDanhMucMon.DisplayMember = "TenDanhMuc";
            cbDanhMucMon.ValueMember = "MaDanhMuc";
        }

        // ======================= HỖ TRỢ GIAO DIỆN =======================
        private static TableLayoutPanel TaoKhungNhap()
        {
            TableLayoutPanel p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10), AutoScroll = true };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return p;
        }

        private static void ThemDong(TableLayoutPanel p, string nhan, Control c)
        {
            int row = p.RowCount++;
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            p.Controls.Add(new Label { Text = nhan, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            p.Controls.Add(c, 1, row);
        }

        private static void ThemNut(TableLayoutPanel p, params Button[] nut)
        {
            FlowLayoutPanel f = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 10, 0, 0) };
            f.Controls.AddRange(nut);
            int row = p.RowCount++;
            p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            p.Controls.Add(f, 0, row);
            p.SetColumnSpan(f, 2);
        }

        private static Button TaoNut(string text, Action action)
        {
            Button b = new Button { Text = text, Width = 85, Height = 34 };
            b.Click += (s, e) => FrmMain.ThucHien(action);
            return b;
        }

        private static void ThongBao(string msg)
        {
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
