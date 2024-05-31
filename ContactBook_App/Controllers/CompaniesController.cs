using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Services;
using ContactBook_Services.DTOs.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactBook_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyService _companyService;
        private readonly IMapper _mapper;

        public CompaniesController(
            CompanyService companyService,
            IMapper mapper)
        {
            _companyService = companyService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<Company>> GetCompanies()
        {
            var companies = await _companyService.GetAllAsync();

            return Ok(companies);
        }

        [HttpPut]
        public async Task<ActionResult<Company>> EditCompany(EditCompanyDTO editCompanyDTO)
        {
            if (!await _companyService.UpdateAsync(editCompanyDTO))
                return BadRequest("The company has not been modified");

            return Ok(new JsonResult(new
            {
                title = "Company modified",
                message = "Your company is modified."
            }));
        }
    }
}
