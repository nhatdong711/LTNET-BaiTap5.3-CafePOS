using System;
using System.Data;
using CafePOS.DAL;

namespace CafePOS.BUS
{
    public class HoaDonBUS
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        public DataTable GetOrderCuaBan(int maBan)
        {
            return dal.GetOrderCuaBan(maBan);
        }

        public void ThemMon(int maBan, int maMon, int soLuong)
        {
            if (maBan <= 0) throw new ArgumentException("Vui lòng chọn bàn.");
            if (maMon <= 0) throw new ArgumentException("Vui lòng chọn món.");
            if (soLuong <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            dal.ThemMon(maBan, maMon, soLuong);
        }

        public void BotMon(int maBan, int maMon, int soLuong)
        {
            if (maBan <= 0 || maMon <= 0) throw new ArgumentException("Vui lòng chọn món cần bớt trong hóa đơn.");
            if (soLuong <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            dal.BotMon(maBan, maMon, soLuong);
        }

        public decimal ThanhToan(int maBan)
        {
            if (maBan <= 0) throw new ArgumentException("Vui lòng chọn bàn cần thanh toán.");
            return dal.ThanhToan(maBan);
        }

        public void ChuyenBan(int maBanCu, int maBanMoi)
        {
            if (maBanCu <= 0 || maBanMoi <= 0) throw new ArgumentException("Vui lòng chọn bàn.");
            if (maBanCu == maBanMoi) throw new ArgumentException("Bàn mới phải khác bàn hiện tại.");
            dal.ChuyenBan(maBanCu, maBanMoi);
        }

        public DataTable GetHoaDonDaThanhToan(DateTime tu, DateTime den)
        {
            KiemTraKhoangNgay(tu, den);
            return dal.GetHoaDonDaThanhToan(tu, den);
        }

        public decimal GetDoanhThu(DateTime tu, DateTime den)
        {
            KiemTraKhoangNgay(tu, den);
            return dal.GetDoanhThu(tu, den);
        }

        public DataTable GetTopMonBanChay(DateTime tu, DateTime den)
        {
            KiemTraKhoangNgay(tu, den);
            return dal.GetTopMonBanChay(tu, den);
        }

        private static void KiemTraKhoangNgay(DateTime tu, DateTime den)
        {
            if (tu.Date > den.Date) throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        }
    }
}
