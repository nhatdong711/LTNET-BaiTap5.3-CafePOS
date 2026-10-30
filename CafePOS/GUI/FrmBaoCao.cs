using System;
using System.Drawing;
using System.Windows.Forms;
using CafePOS.BUS;

namespace CafePOS.GUI
{
    /// <summary>Báo cáo doanh thu theo khoảng ngày + Top 5 món bán chạy.</summary>
    public class FrmBaoCao : Form
    {
        private readonly HoaDonBUS bus = new HoaDonBUS();
        private readonly DateTimePicker dtpTu, dtpDen;
        private readonly DataGridView dgvHoaDon, dgvTop;
        private readonly Label lblDoanhThu;

        public FrmBaoCao()
        {
            Text = "Báo cáo doanh thu";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10F);

            FlowLayoutPanel top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            dtpTu = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130, Value = DateTime.Today.AddDays(-7) };
            dtpDen = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130, Value = DateTime.Today };
            Button btnXem = new Button { Text = "Xem báo cáo", Width = 120, Height = 30 };
            lblDoanhThu = new Label { AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkGreen, Margin = new Padding(30, 6, 0, 0) };
            top.Controls.AddRange(new Control[]
            {
                new Label { Text = "Từ ngày:", AutoSize = true, Margin = new Padding(0, 6, 0, 0) }, dtpTu,
                new Label { Text = "Đến ngày:", AutoSize = true, Margin = new Padding(10, 6, 0, 0) }, dtpDen,
                btnXem, lblDoanhThu
            });

            dgvHoaDon = FrmMain.TaoLuoi();
            dgvTop = FrmMain.TaoLuoi();
            GroupBox gbHD = new GroupBox { Text = "Hóa đơn đã thanh toán", Dock = DockStyle.Fill };
            gbHD.Controls.Add(dgvHoaDon);
            GroupBox gbTop = new GroupBox { Text = "Top 5 món bán chạy", Dock = DockStyle.Fill };
            gbTop.Controls.Add(dgvTop);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 600 };
            split.Panel1.Controls.Add(gbHD);
            split.Panel2.Controls.Add(gbTop);

            Controls.Add(split);
            Controls.Add(top);

            btnXem.Click += (s, e) => FrmMain.ThucHien(XemBaoCao);
            Load += (s, e) => FrmMain.ThucHien(XemBaoCao);
        }

        private void XemBaoCao()
        {
            dgvHoaDon.DataSource = bus.GetHoaDonDaThanhToan(dtpTu.Value, dtpDen.Value);
            dgvTop.DataSource = bus.GetTopMonBanChay(dtpTu.Value, dtpDen.Value);
            decimal doanhThu = bus.GetDoanhThu(dtpTu.Value, dtpDen.Value); // ExecuteScalar
            lblDoanhThu.Text = "Tổng doanh thu: " + doanhThu.ToString("N0") + " đ";

            if (dgvHoaDon.Columns.Contains("Tổng tiền")) dgvHoaDon.Columns["Tổng tiền"].DefaultCellStyle.Format = "N0";
            if (dgvHoaDon.Columns.Contains("Giờ vào")) dgvHoaDon.Columns["Giờ vào"].DefaultCellStyle.Format = "dd/MM HH:mm";
            if (dgvHoaDon.Columns.Contains("Giờ ra")) dgvHoaDon.Columns["Giờ ra"].DefaultCellStyle.Format = "dd/MM HH:mm";
            if (dgvTop.Columns.Contains("Doanh thu")) dgvTop.Columns["Doanh thu"].DefaultCellStyle.Format = "N0";
        }
    }
}
