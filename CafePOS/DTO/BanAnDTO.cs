namespace CafePOS.DTO
{
    /// <summary>Dữ liệu một bàn ăn (bảng BanAn).</summary>
    public class BanAnDTO
    {
        public const string TRONG = "Trống";
        public const string CO_KHACH = "Có khách";

        public int MaBan { get; set; }
        public string TenBan { get; set; }
        public string TrangThai { get; set; }

        public bool DangTrong { get { return TrangThai == TRONG; } }

        public override string ToString() { return TenBan; }
    }
}
