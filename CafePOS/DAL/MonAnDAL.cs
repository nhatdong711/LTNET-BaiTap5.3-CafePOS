using System.Data;
using System.Data.SqlClient;
using CafePOS.DTO;

namespace CafePOS.DAL
{
    public class MonAnDAL
    {
        /// <summary>
        /// Mô hình ngắt kết nối: nạp toàn bộ thực đơn vào một DataSet gồm 2 DataTable
        /// (DanhMuc, MonAn) và tạo DataRelation giữa chúng. Form gọi món chỉ đọc DataSet này,
        /// không phải truy vấn CSDL mỗi lần đổi danh mục.
        /// </summary>
        public DataSet LoadMenuDataSet()
        {
            DataSet ds = new DataSet("Menu");
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                SqlDataAdapter daDM = new SqlDataAdapter("SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc", conn);
                SqlDataAdapter daMon = new SqlDataAdapter("SELECT MaMon, TenMon, MaDanhMuc, DonGia FROM MonAn", conn);
                daDM.Fill(ds, "DanhMuc");
                daMon.Fill(ds, "MonAn");
            }
            ds.Relations.Add("DanhMuc_MonAn",
                ds.Tables["DanhMuc"].Columns["MaDanhMuc"],
                ds.Tables["MonAn"].Columns["MaDanhMuc"]);
            return ds;
        }

        public DataTable Search(string tuKhoa)
        {
            const string sql =
                @"SELECT m.MaMon, m.TenMon, m.MaDanhMuc, d.TenDanhMuc, m.DonGia
                  FROM MonAn m JOIN DanhMuc d ON m.MaDanhMuc = d.MaDanhMuc
                  WHERE m.TenMon LIKE @TuKhoa
                  ORDER BY m.MaMon";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@TuKhoa", "%" + tuKhoa + "%"));
        }

        public bool Insert(MonAnDTO m)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO MonAn (TenMon, MaDanhMuc, DonGia) VALUES (@Ten, @MaDM, @Gia)",
                new SqlParameter("@Ten", m.TenMon),
                new SqlParameter("@MaDM", m.MaDanhMuc),
                new SqlParameter("@Gia", m.DonGia)) > 0;
        }

        public bool Update(MonAnDTO m)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE MonAn SET TenMon = @Ten, MaDanhMuc = @MaDM, DonGia = @Gia WHERE MaMon = @Ma",
                new SqlParameter("@Ten", m.TenMon),
                new SqlParameter("@MaDM", m.MaDanhMuc),
                new SqlParameter("@Gia", m.DonGia),
                new SqlParameter("@Ma", m.MaMon)) > 0;
        }

        public bool Delete(int maMon)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM MonAn WHERE MaMon = @Ma", new SqlParameter("@Ma", maMon)) > 0;
        }

        public int CountDaBan(int maMon)
        {
            return (int)DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM ChiTietHoaDon WHERE MaMon = @Ma", new SqlParameter("@Ma", maMon));
        }
    }
}
