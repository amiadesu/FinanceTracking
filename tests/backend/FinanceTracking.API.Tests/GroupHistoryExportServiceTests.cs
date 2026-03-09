using FinanceTracking.API.DTOs;
using FinanceTracking.API.Services;
using FluentAssertions;

namespace FinanceTracking.API.Tests.Services;

public class GroupHistoryExportServiceTests
{
    [Fact]
    public void ExportToExcel_ShouldReturnValidZipByteArray()
    {
        var service = new GroupHistoryExportService();
        var data = new List<GroupHistoryDto> 
        { 
            new GroupHistoryDto 
            { 
                ChangedAt = DateTime.UtcNow, 
                Note = "Test Note", 
                ChangedByUserName = "AdminUser", 
                TargetUserName = "TargetUser" 
            } 
        };

        var result = service.ExportToExcel(data);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        
        // All .xlsx files are actually ZIP archives under the hood. 
        // Zip files ALWAYS start with the hex bytes 0x50 0x4B.
        result[0].Should().Be(0x50);
        result[1].Should().Be(0x4B);
    }
}