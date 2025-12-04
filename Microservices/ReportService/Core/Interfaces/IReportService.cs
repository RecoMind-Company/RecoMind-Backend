using Core.DTOs;

namespace Core.Interfaces;

public interface IReportService
{
    Task<AiReportResponseDto> GetReport(GetReportDto getReportDto);
}
