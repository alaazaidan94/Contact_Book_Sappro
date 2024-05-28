using AutoMapper;
using ContactBook_App.DTOs.Company;
using ContactBook_Domain.Models;
using ContactBook_Services.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepository<Company, int> _companyRepo;
        private readonly IMapper _mapper;

        public CompaniesController(
            IRepository<Company,int> companyRepo,
            IMapper mapper)
        {
            _companyRepo = companyRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<Company>> GetCompanies()
        {
            var companies = await _companyRepo.GetAllAsync();

            return Ok(companies);
        }

        [HttpPut("{companyId}")]
        public async Task<ActionResult<Company>> EditCompany(int companyId,EditCompanyDTO editCompanyDTO)
        {
            if (int.IsNegative(companyId))
                return BadRequest("Invalid Company");

            var company = await _companyRepo.GetByIdAsync(companyId);
            if (company == null)
                return BadRequest("Company Not Found");

             _mapper.Map(editCompanyDTO,company);

            var result = await _companyRepo.UpdateAsync(company);
            if (!result)
                return BadRequest("Invalid Edit Company");

            return Ok(company);
        }
    }
}
