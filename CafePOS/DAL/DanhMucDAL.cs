using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CafePOS.DTO;

namespace CafePOS.DAL
{
    public class DanhMucDAL
    {
        public List<DanhMucDTO> GetAll()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc ORDER BY MaDanhMuc");
            List<DanhMucDTO> list = new List<DanhMucDTO>();
            foreach (DataRow r in dt.Rows)
                list.Add(new DanhMucDTO { MaDanhMuc = (int)r["MaDanhMuc"], TenDanhMuc = r["TenDanhMuc"].ToString() });
            return list;
        }

        public bool Insert(DanhMucDTO dm)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO DanhMuc (TenDanhMuc) VALUES (@Ten)",
                new SqlParameter("@Ten", dm.TenDanhMuc)) > 0;
        }

        public bool Update(DanhMucDTO dm)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE DanhMuc SET TenDanhMuc = @Ten WHERE MaDanhMuc = @Ma",
                new SqlParameter("@Ten", dm.TenDanhMuc),
                new SqlParameter("@Ma", dm.MaDanhMuc)) > 0;
        }

        public bool Delete(int maDanhMuc)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM DanhMuc WHERE MaDanhMuc = @Ma",
                new SqlParameter("@Ma", maDanhMuc)) > 0;
        }

        public int CountMon(int maDanhMuc)
        {
            return (int)DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM MonAn WHERE MaDanhMuc = @Ma",
                new SqlParameter("@Ma", maDanhMuc));
        }
    }
}
