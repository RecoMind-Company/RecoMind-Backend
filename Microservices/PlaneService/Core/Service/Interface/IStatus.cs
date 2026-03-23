using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface IStatus
    {
        Task<bool> AddStatus(string status);
        Task<string> GetStatusByName(string status);
        Task<IEnumerable<string>> GetAllStatuses();
        Task<bool> DeleteStatus(string status);
    }
}
