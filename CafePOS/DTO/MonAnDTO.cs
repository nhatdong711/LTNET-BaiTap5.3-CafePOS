namespace CafePOS.DTO
{
    /// <summary>Dữ liệu món trong thực đơn (bảng MonAn).</summary>
    public class MonAnDTO
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public int MaDanhMuc { get; set; }
        public decimal DonGia { get; set; }
    }
}
