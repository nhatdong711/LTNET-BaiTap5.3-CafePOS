using System;

namespace CafePOS.DTO
{
    /// <summary>Dữ liệu hóa đơn (bảng HoaDon).</summary>
    public class HoaDonDTO
    {
        public int MaHoaDon { get; set; }
        public int MaBan { get; set; }
        public DateTime GioVao { get; set; }
        public DateTime? GioRa { get; set; }
        public decimal TongTien { get; set; }
        public bool DaThanhToan { get; set; }
    }
}
