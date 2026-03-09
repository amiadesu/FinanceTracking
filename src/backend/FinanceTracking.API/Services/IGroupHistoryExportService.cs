using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IGroupHistoryExportService
{
    byte[] ExportToExcel(List<GroupHistoryDto> historyEntries);
    byte[] ExportToWord(List<GroupHistoryDto> historyEntries);
}