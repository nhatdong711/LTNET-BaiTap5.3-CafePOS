using System.Collections.Generic;
using CafePOS.DAL;
using CafePOS.DTO;

namespace CafePOS.BUS
{
    public class BanAnBUS
    {
        private readonly BanAnDAL dal = new BanAnDAL();

        public List<BanAnDTO> GetAll()
        {
            return dal.GetAll();
        }
    }
}
