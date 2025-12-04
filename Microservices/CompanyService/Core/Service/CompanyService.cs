using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Service.Interface;


namespace Core.Service
{
    public class CompanyService : ICompanyService
    {

        readonly IUnitOfWork<Models.Company> _CompanyUnitOfWork;
        readonly private IMapper _mapper;
        public CompanyService(IUnitOfWork<Models.Company> CopmanyUnitOfWork, IMapper mapper)
        {
            _CompanyUnitOfWork = CopmanyUnitOfWork;
            _mapper = mapper;
        }
        public async Task<GetCompanyDTO> CreateCompanyAsync(CreateCompanyDTO createCompanyDTO)
        {
            if (createCompanyDTO == null) throw new ArgumentNullException(nameof(createCompanyDTO));
        
            var entity = _mapper.Map<Models.Company>(createCompanyDTO);
            entity.Id = Guid.NewGuid().ToString();
            entity.CreatedAt = DateTime.UtcNow;
            var result = await _CompanyUnitOfWork.Entity.AddAsync(entity);
            _CompanyUnitOfWork.Save();

            return _mapper.Map<GetCompanyDTO>(result);
        }
        public async Task<IEnumerable<GetCompanyDTO>> GetAllCompaniesAsync()
        {
            var items = await _CompanyUnitOfWork.Entity.GetAllAsync();
            return _mapper.Map<IEnumerable<GetCompanyDTO>>(items);
        }
        public async Task<GetCompanyDTO> GetCompanyByIdAsync(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

            var item = await _CompanyUnitOfWork.Entity.GetByIdNoTrackingAsync(companyId);
            if (item == null)
                throw new KeyNotFoundException($"Company with ID '{companyId}' was not found.");

            return _mapper.Map<GetCompanyDTO>(item);
        }

        public async Task<GetCompanyDTO> GetCompanyByAdminId(string adminId)
        {            
            var item = await _CompanyUnitOfWork.Entity.Find(x=>x.AdminId == adminId);
            
            if (item == null)
                throw new KeyNotFoundException($"Company with Code '{adminId}' was not found.");

            return _mapper.Map<GetCompanyDTO>(item);
        }
        public async Task<UpdateCompanyDTO> UpdateCompanyAsync(string companyId, CreateCompanyDTO companyDTO)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

            var existingCompany = await _CompanyUnitOfWork.Entity.GetByIdNoTrackingAsync(companyId);
            if (existingCompany == null)
                throw new KeyNotFoundException($"Company with ID '{companyId}' was not found.");

            var entity = _mapper.Map<Models.Company>(companyDTO);
            entity.Id = companyId;       // Preserve ID

            await _CompanyUnitOfWork.Entity.UpdateAsync(entity);
            _CompanyUnitOfWork.Save();

            return _mapper.Map<UpdateCompanyDTO>(entity);
        }
        public async Task<DeleteCompanyDTO> DeleteCompanyAsync(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

            var item = await _CompanyUnitOfWork.Entity.GetByIdNoTrackingAsync(companyId);
            if (item == null)
                throw new KeyNotFoundException($"Company with ID '{companyId}' was not found.");

            var entity = _mapper.Map<DeleteCompanyDTO>(item);

            _CompanyUnitOfWork.Entity.Delete(item);
            _CompanyUnitOfWork.Save();

            return entity;
        }

        //public async Task<GetCompanyDTO> CreateCompanyWithSubscriptionAsync(CreateCompanyDTO createCompanyDTO, string subscriptionId)
        //{
        //    var entity = _mapper.Map<Core.Models.Company>(createCompanyDTO);
        //    entity.Id = Guid.NewGuid().ToString();                    
        //    entity.SubscriptionId = subscriptionId;

        //    var result = await _CompanyUnitOfWork.Entity.AddAsync(entity);
        //    _CompanyUnitOfWork.Save();

        //    return _mapper.Map<GetCompanyDTO>(result);
        //}

        //public async Task<string> GetSubscriptionId(string companyId)
        //{
        //    if (string.IsNullOrWhiteSpace(companyId))
        //        throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

        //    var item =await _CompanyUnitOfWork.Entity.GetByIdNoTrackingAsync(companyId);

        //    if (item == null)
        //        throw new KeyNotFoundException($"Company with ID '{companyId}' was not found.");

        //    return item.SubscriptionId??"";
        //}
    }
}



