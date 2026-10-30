using System;
using System.Data;
using System.Data.SqlClient;
using CafePOS.DTO;

namespace CafePOS.DAL
{
    public class HoaDonDAL
    {
        /// <summary>
        /// Nạp danh sách món đã gọi của bàn (hóa đơn chưa thanh toán) vào DataTable
        /// bằng SqlDataAdapter (mô hình ngắt kết nối).
        /// </summary>
        public DataTable GetOrderCuaBan(int maBan)
        {
            const string sql =
                @"SELECT ct.MaMon, m.TenMon AS [Tên món], ct.SoLuong AS [SL],
                         ct.DonGia AS [Đơn giá], ct.SoLuong * ct.DonGia AS [Thành tiền]
                  FROM ChiTietHoaDon ct
                  JOIN HoaDon h ON ct.MaHoaDon = h.MaHoaDon
                  JOIN MonAn m  ON ct.MaMon = m.MaMon
                  WHERE h.MaBan = @MaBan AND h.TrangThai = 0";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter("@MaBan", maBan));
        }

        /// <summary>
        /// Thêm món cho bàn. Nếu bàn chưa có hóa đơn mở thì tạo hóa đơn mới.
        /// Nếu món đã có trong hóa đơn thì cộng dồn số lượng. Đổi trạng thái bàn sang "Có khách".
        /// Toàn bộ chạy trong một SqlTransaction.
        /// </summary>
        public void ThemMon(int maBan, int maMon, int soLuong)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int maHoaDon = LayHoaDonMo(conn, trans, maBan);
                    if (maHoaDon == 0)
                    {
                        SqlCommand cmdHD = new SqlCommand(
                            @"INSERT INTO HoaDon (MaBan, GioVao, TongTien, TrangThai) VALUES (@MaBan, GETDATE(), 0, 0);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans);
                        cmdHD.Parameters.AddWithValue("@MaBan", maBan);
                        maHoaDon = (int)cmdHD.ExecuteScalar();
                    }

                    SqlCommand cmdCT = new SqlCommand(
                        @"IF EXISTS (SELECT 1 FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD AND MaMon = @MaMon)
                              UPDATE ChiTietHoaDon SET SoLuong = SoLuong + @SL WHERE MaHoaDon = @MaHD AND MaMon = @MaMon
                          ELSE
                              INSERT INTO ChiTietHoaDon (MaHoaDon, MaMon, SoLuong, DonGia)
                              SELECT @MaHD, MaMon, @SL, DonGia FROM MonAn WHERE MaMon = @MaMon", conn, trans);
                    cmdCT.Parameters.AddWithValue("@MaHD", maHoaDon);
                    cmdCT.Parameters.AddWithValue("@MaMon", maMon);
                    cmdCT.Parameters.AddWithValue("@SL", soLuong);
                    cmdCT.ExecuteNonQuery();

                    CapNhatTrangThaiBan(conn, trans, maBan, BanAnDTO.CO_KHACH);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Bớt món: giảm số lượng, nếu về 0 thì xóa dòng chi tiết.</summary>
        public void BotMon(int maBan, int maMon, int soLuong)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int maHoaDon = LayHoaDonMo(conn, trans, maBan);
                    if (maHoaDon == 0) throw new InvalidOperationException("Bàn chưa có hóa đơn.");

                    SqlCommand cmd = new SqlCommand(
                        @"UPDATE ChiTietHoaDon SET SoLuong = SoLuong - @SL WHERE MaHoaDon = @MaHD AND MaMon = @MaMon AND SoLuong > @SL;
                          IF @@ROWCOUNT = 0
                              DELETE FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD AND MaMon = @MaMon;", conn, trans);
                    cmd.Parameters.AddWithValue("@MaHD", maHoaDon);
                    cmd.Parameters.AddWithValue("@MaMon", maMon);
                    cmd.Parameters.AddWithValue("@SL", soLuong);
                    cmd.ExecuteNonQuery();

                    // Nếu hóa đơn không còn món nào thì hủy hóa đơn và trả bàn về "Trống"
                    SqlCommand cmdDem = new SqlCommand("SELECT COUNT(*) FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD", conn, trans);
                    cmdDem.Parameters.AddWithValue("@MaHD", maHoaDon);
                    if ((int)cmdDem.ExecuteScalar() == 0)
                    {
                        SqlCommand cmdXoa = new SqlCommand("DELETE FROM HoaDon WHERE MaHoaDon = @MaHD", conn, trans);
                        cmdXoa.Parameters.AddWithValue("@MaHD", maHoaDon);
                        cmdXoa.ExecuteNonQuery();
                        CapNhatTrangThaiBan(conn, trans, maBan, BanAnDTO.TRONG);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Thanh toán: tính tổng tiền → cập nhật GioRa, TongTien, TrangThai = 1 → trả bàn về "Trống".
        /// Tất cả bọc trong 1 SqlTransaction. Trả về tổng tiền đã thanh toán.
        /// </summary>
        public decimal ThanhToan(int maBan)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int maHoaDon = LayHoaDonMo(conn, trans, maBan);
                    if (maHoaDon == 0) throw new InvalidOperationException("Bàn này chưa có hóa đơn để thanh toán.");

                    SqlCommand cmdTong = new SqlCommand(
                        "SELECT ISNULL(SUM(SoLuong * DonGia), 0) FROM ChiTietHoaDon WHERE MaHoaDon = @MaHD", conn, trans);
                    cmdTong.Parameters.AddWithValue("@MaHD", maHoaDon);
                    decimal tongTien = Convert.ToDecimal(cmdTong.ExecuteScalar());

                    SqlCommand cmdHD = new SqlCommand(
                        "UPDATE HoaDon SET GioRa = GETDATE(), TongTien = @Tong, TrangThai = 1 WHERE MaHoaDon = @MaHD", conn, trans);
                    cmdHD.Parameters.AddWithValue("@Tong", tongTien);
                    cmdHD.Parameters.AddWithValue("@MaHD", maHoaDon);
                    cmdHD.ExecuteNonQuery();

                    CapNhatTrangThaiBan(conn, trans, maBan, BanAnDTO.TRONG);

                    trans.Commit();
                    return tongTien;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Đổi bàn: chuyển hóa đơn mở từ bàn cũ sang bàn mới (bàn mới phải đang trống).</summary>
        public void ChuyenBan(int maBanCu, int maBanMoi)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    if (LayHoaDonMo(conn, trans, maBanMoi) != 0)
                        throw new InvalidOperationException("Bàn mới đang có khách, không thể chuyển.");

                    SqlCommand cmd = new SqlCommand(
                        "UPDATE HoaDon SET MaBan = @Moi WHERE MaBan = @Cu AND TrangThai = 0", conn, trans);
                    cmd.Parameters.AddWithValue("@Moi", maBanMoi);
                    cmd.Parameters.AddWithValue("@Cu", maBanCu);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Bàn hiện tại chưa có hóa đơn để chuyển.");

                    CapNhatTrangThaiBan(conn, trans, maBanCu, BanAnDTO.TRONG);
                    CapNhatTrangThaiBan(conn, trans, maBanMoi, BanAnDTO.CO_KHACH);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public DataTable GetHoaDonDaThanhToan(DateTime tuNgay, DateTime denNgay)
        {
            const string sql =
                @"SELECT h.MaHoaDon AS [Mã HĐ], b.TenBan AS [Bàn], h.GioVao AS [Giờ vào],
                         h.GioRa AS [Giờ ra], h.TongTien AS [Tổng tiền]
                  FROM HoaDon h JOIN BanAn b ON h.MaBan = b.MaBan
                  WHERE h.TrangThai = 1 AND h.GioRa >= @Tu AND h.GioRa < @Den
                  ORDER BY h.GioRa DESC";
            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@Tu", tuNgay.Date),
                new SqlParameter("@Den", denNgay.Date.AddDays(1)));
        }

        public decimal GetDoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            object kq = DatabaseHelper.ExecuteScalar(
                "SELECT ISNULL(SUM(TongTien), 0) FROM HoaDon WHERE TrangThai = 1 AND GioRa >= @Tu AND GioRa < @Den",
                new SqlParameter("@Tu", tuNgay.Date),
                new SqlParameter("@Den", denNgay.Date.AddDays(1)));
            return Convert.ToDecimal(kq);
        }

        public DataTable GetTopMonBanChay(DateTime tuNgay, DateTime denNgay)
        {
            const string sql =
                @"SELECT TOP 5 m.TenMon AS [Món], SUM(ct.SoLuong) AS [Số lượng bán],
                         SUM(ct.SoLuong * ct.DonGia) AS [Doanh thu]
                  FROM ChiTietHoaDon ct
                  JOIN HoaDon h ON ct.MaHoaDon = h.MaHoaDon
                  JOIN MonAn m ON ct.MaMon = m.MaMon
                  WHERE h.TrangThai = 1 AND h.GioRa >= @Tu AND h.GioRa < @Den
                  GROUP BY m.TenMon
                  ORDER BY SUM(ct.SoLuong) DESC";
            return DatabaseHelper.ExecuteQuery(sql,
                new SqlParameter("@Tu", tuNgay.Date),
                new SqlParameter("@Den", denNgay.Date.AddDays(1)));
        }

        // ---------------- Hàm dùng chung trong transaction ----------------
        private static int LayHoaDonMo(SqlConnection conn, SqlTransaction trans, int maBan)
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 MaHoaDon FROM HoaDon WHERE MaBan = @MaBan AND TrangThai = 0", conn, trans);
            cmd.Parameters.AddWithValue("@MaBan", maBan);
            object kq = cmd.ExecuteScalar();
            return kq == null ? 0 : (int)kq;
        }

        private static void CapNhatTrangThaiBan(SqlConnection conn, SqlTransaction trans, int maBan, string trangThai)
        {
            SqlCommand cmd = new SqlCommand("UPDATE BanAn SET TrangThai = @TT WHERE MaBan = @MaBan", conn, trans);
            cmd.Parameters.AddWithValue("@TT", trangThai);
            cmd.Parameters.AddWithValue("@MaBan", maBan);
            cmd.ExecuteNonQuery();
        }
    }
}
