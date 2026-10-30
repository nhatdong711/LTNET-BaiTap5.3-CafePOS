using System;
using System.Collections.Generic;
using CafePOS.DAL;
using CafePOS.DTO;

namespace CafePOS.BUS
{
    public class DanhMucBUS
    {
        private readonly DanhMucDAL dal = new DanhMucDAL();

        public List<DanhMucDTO> GetAll()
        {
            return dal.GetAll();
        }

        public bool Them(DanhMucDTO dm)
        {
            KiemTra(dm);
            return dal.Insert(dm);
        }

        public bool Sua(DanhMucDTO dm)
        {
            if (dm.MaDanhMuc <= 0) throw new ArgumentException("Vui lòng chọn danh mục cần sửa.");
            KiemTra(dm);
            return dal.Update(dm);
        }

        public bool Xoa(int maDanhMuc)
        {
            if (maDanhMuc <= 0) throw new ArgumentException("Vui lòng chọn danh mục cần xóa.");
            if (dal.CountMon(maDanhMuc) > 0)
                throw new InvalidOperationException("Danh mục đang có món ăn, hãy xóa hoặc chuyển các món trước.");
            return dal.Delete(maDanhMuc);
        }

        private static void KiemTra(DanhMucDTO dm)
        {
            if (string.IsNullOrWhiteSpace(dm.TenDanhMuc))
                throw new ArgumentException("Tên danh mục không được để trống.");
            dm.TenDanhMuc = dm.TenDanhMuc.Trim();
            if (dm.TenDanhMuc.Length > 100)
                throw new ArgumentException("Tên danh mục tối đa 100 ký tự.");
        }
    }
}
