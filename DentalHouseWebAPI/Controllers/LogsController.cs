using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class LogsController : ControllerBase
    {
        private readonly ISystemLogService _logService;

        public LogsController(ISystemLogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? module = null,
            [FromQuery] string? action = null,
            [FromQuery] string? userName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isSuccess = null)
        {
            if (endDate.HasValue && endDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            var response = await _logService.GetPagedAsync(
                pageNumber,
                pageSize,
                module,
                action,
                userName,
                startDate,
                endDate,
                searchTerm,
                isSuccess);

            if (response.IsSuccess && response.Data != null)
            {
                return Ok(new
                {
                    pageNumber = response.Data.PageNumber,
                    pageSize = response.Data.PageSize,
                    totalRecords = response.Data.TotalRecords,
                    totalPages = response.Data.TotalPages,
                    data = response.Data.Data
                });
            }

            return StatusCode(500, new UnsuccessfulResponseDTO
            {
                Code = "500",
                Message = response.Message ?? "Error al consultar los logs del sistema."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new UnsuccessfulResponseDTO
                {
                    Code = "400",
                    Message = "El Id debe ser mayor a 0."
                });
            }

            var response = await _logService.GetByIdAsync(id);

            if (response.IsSuccess && response.Data != null)
            {
                return Ok(response.Data);
            }

            if (response.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(new UnsuccessfulResponseDTO
                {
                    Code = "404",
                    Message = "No se encontró el registro de log con el Id especificado."
                });
            }

            return StatusCode(500, new UnsuccessfulResponseDTO
            {
                Code = "500",
                Message = response.Message ?? "Error interno al consultar el detalle del log."
            });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var today = DateTime.Today;
            var endOfDay = today.AddDays(1).AddTicks(-1);
            return await GetAll(pageNumber, pageSize, startDate: today, endDate: endOfDay);
        }

        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? module = null,
            [FromQuery] string? action = null,
            [FromQuery] string? userName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null)
        {
            return await GetAll(pageNumber, pageSize, module, action, userName, startDate, endDate, searchTerm, isSuccess: false);
        }

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules()
        {
            var response = await _logService.GetModulesAsync();
            return Ok(response.Data);
        }

        [HttpGet("actions")]
        public async Task<IActionResult> GetActions()
        {
            var response = await _logService.GetActionsAsync();
            return Ok(response.Data);
        }
    }
}
