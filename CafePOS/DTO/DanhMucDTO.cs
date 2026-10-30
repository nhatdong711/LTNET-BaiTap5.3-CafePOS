namespace CafePOS.DTO
{
    /// <summary>Dữ liệu danh mục món (bảng DanhMuc).</summary>
    public class DanhMucDTO
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }

        public override string ToString() { return TenDanhMuc; }
    }
}
