using System;
using System.Data;
using CafePOS.DAL;
using CafePOS.DTO;

namespace CafePOS.BUS
{
    public class MonAnBUS
    {
        private readonly MonAnDAL dal = new MonAnDAL();

        public DataSet LoadMenuDataSet()
        {
            return dal.LoadMenuDataSet();
        }

        public DataTable TimKiem(string tuKhoa)
        {
            return dal.Search((tuKhoa ?? "").Trim());
        }

        public bool Them(MonAnDTO m)
        {
            KiemTra(m);
            return dal.Insert(m);
        }

        public bool Sua(MonAnDTO m)
        {
            if (m.MaMon <= 0) throw new ArgumentException("Vui lòng chọn món cần sửa.");
            KiemTra(m);
            return dal.Update(m);
        }

        public bool Xoa(int maMon)
        {
            if (maMon <= 0) throw new ArgumentException("Vui lòng chọn món cần xóa.");
            if (dal.CountDaBan(maMon) > 0)
                throw new InvalidOperationException("Món đã có trong hóa đơn, không thể xóa (chỉ có thể sửa giá/tên).");
            return dal.Delete(maMon);
        }

        /// <summary>Validation: tên món không trống, đơn giá không âm, phải chọn danh mục.</summary>
        private static void KiemTra(MonAnDTO m)
        {
            if (string.IsNullOrWhiteSpace(m.TenMon))
                throw new ArgumentException("Tên món không được để trống.");
            m.TenMon = m.TenMon.Trim();
            if (m.TenMon.Length > 100)
                throw new ArgumentException("Tên món tối đa 100 ký tự.");
            if (m.DonGia < 0)
                throw new ArgumentException("Đơn giá không được âm.");
            if (m.MaDanhMuc <= 0)
                throw new ArgumentException("Vui lòng chọn danh mục cho món.");
        }
    }
}
