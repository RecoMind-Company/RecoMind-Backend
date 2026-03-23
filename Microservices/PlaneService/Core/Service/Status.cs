using Core.Interfaces;
using Core.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service
{
    public class Status : IStatus
    {
        readonly IUnitOfWork<Core.Models.Status> _unitOfWork;
        public Status(IUnitOfWork<Core.Models.Status> UnitOfWork)
        {
            _unitOfWork = UnitOfWork;
        }

        public async Task<bool> AddStatus(string status)
        {
            var newStatus = new Core.Models.Status { 
                 Name = status ,
                 Id = Guid.NewGuid().ToString()
            };

            await _unitOfWork.Entity.AddAsync(newStatus);
            _unitOfWork.Save();

            return true;
        }

        public async Task<bool> DeleteStatus(string status)
        {
            var statusName = await GetStatusByName(status);

            if (statusName != null)
            {
                _unitOfWork.Entity.Delete(new Core.Models.Status { Name = statusName });
                _unitOfWork.Save();

                return true;
            }

            return false; // Status not found
        }

        public async Task<IEnumerable<string>> GetAllStatuses()
        {
            var result = new List<string>();

            var items = await _unitOfWork.Entity.GetAllAsync();

            if (items != null)
            {
                foreach (var item in items)
                {
                    result.Add(item.Name);
                }
                return result;
            }
            return Enumerable.Empty<string>();
        }

        public async Task<string> GetStatusByName(string status)
        {
            var statusEntity = await _unitOfWork.Entity.Find(s => s.Name.ToLower() == status.ToLower());

            if (statusEntity == null)
            {
                return null;     // Return null if no status is found
            }

            return statusEntity.Name;
        }
    }
}
