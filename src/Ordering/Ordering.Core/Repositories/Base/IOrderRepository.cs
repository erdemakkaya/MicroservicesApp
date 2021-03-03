using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ordering.Core.Entities;

namespace Ordering.Core.Repositories.Base
{
   public interface IOrderRepository:IRepository<Order>
   {
       Task<IEnumerable<Order>> GetOrderByUserName(string userName);
   }
}
