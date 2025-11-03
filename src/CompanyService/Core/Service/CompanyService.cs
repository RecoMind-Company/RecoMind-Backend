using AutoMapper;
using Core.Const;
using Core.DTOs;
using Core.Interfaces;
using Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class CompanyService : ICompanyService
    {
        readonly IUnitOfWork<Core.Models.Company> _CompanyUnitOfWork;
        readonly private IMapper _mapper;
        public CompanyService(IUnitOfWork<Core.Models.Company> CopmanyUnitOfWork, IMapper mapper)
        {
            _CompanyUnitOfWork = CopmanyUnitOfWork;
            _mapper = mapper;
        }

        public async Task<GetCompanyDTO> CreateCompanyAsync(CreateCompanyDTO createCompanyDTO)
        {
            var entity = _mapper.Map<Core.Models.Company>(createCompanyDTO);
            entity.Id= Guid.NewGuid().ToString();
            var result = await _CompanyUnitOfWork.Entity.AddAsync(entity);
            _CompanyUnitOfWork.Save();
            return _mapper.Map<GetCompanyDTO>(result);
        }
        public async Task<IEnumerable<GetCompanyDTO>> GetAllCompaniesAsync()
        {
            var items = await _CompanyUnitOfWork.Entity.GetAllAsync();
            if (items.Count() == 0) throw new Exception("No Companies Registered !");
            return _mapper.Map<IEnumerable<GetCompanyDTO>>(items);
        }
        public async Task<GetCompanyDTO> GetCompanyByIdAsync(string companyId)
        {
            var item = await _CompanyUnitOfWork.Entity.GetByIdNoTrackingAsync(companyId);
            if (item == null) throw new Exception("This Id Not Found ");
            return _mapper.Map<GetCompanyDTO>(item);
        }

        public async Task<UpdateCompanyDTO> UpdateCompanyAsync(string companyId, CreateCompanyDTO companyDTO)
        {
            var item = await _CompanyUnitOfWork.Entity.GetByIdNoTrackingAsync(companyId);
            if (item == null) throw new Exception("This Id Not Found ");

            var entity = _mapper.Map<Core.Models.Company>(companyDTO);
            entity.Id = companyId;
            await _CompanyUnitOfWork.Entity.UpdateAsync(entity);
            _CompanyUnitOfWork.Save();

            return _mapper.Map<UpdateCompanyDTO>(entity);
        }
        public async Task<DeleteCompanyDTO> DeleteCompanyAsync(string companyId)
        {
            var item = await _CompanyUnitOfWork.Entity.GetByIdNoTrackingAsync(companyId);

            if (item == null) throw new Exception("Please Enter Right Id");

            var entity = _mapper.Map<DeleteCompanyDTO>(item);
            _CompanyUnitOfWork.Entity.Delete(item);
            _CompanyUnitOfWork.Save();

            return entity;
        }       
    }
}
