using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Xceed.Document.NET;
using Xceed.Words.NET;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public class GroupHistoryExportService : IGroupHistoryExportService
{
    private static readonly string[] _headers = { 
        "Date", "Action By", "Target User", "Note", 
        "Role Before", "Role After", "Active Before", "Active After" 
    };

    public byte[] ExportToExcel(List<GroupHistoryDto> historyEntries)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Group History");

        for (int i = 0; i < _headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = _headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        int row = 2;
        foreach (var entry in historyEntries)
        {
            worksheet.Cell(row, 1).Value = entry.ChangedAt.ToString("g");
            worksheet.Cell(row, 2).Value = entry.ChangedByUserName;
            worksheet.Cell(row, 3).Value = entry.TargetUserName;
            worksheet.Cell(row, 4).Value = entry.Note;
            worksheet.Cell(row, 5).Value = entry.RoleIdBefore?.ToString() ?? "-";
            worksheet.Cell(row, 6).Value = entry.RoleIdAfter?.ToString() ?? "-";
            worksheet.Cell(row, 7).Value = entry.ActiveBefore?.ToString() ?? "-";
            worksheet.Cell(row, 8).Value = entry.ActiveAfter?.ToString() ?? "-";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportToWord(List<GroupHistoryDto> historyEntries)
    {
        using var stream = new MemoryStream();
        using (var document = DocX.Create(stream))
        {
            document.InsertParagraph("Group History Report")
                .FontSize(18)
                .Bold()
                .SpacingAfter(10);

            document.InsertParagraph($"Generated on: {DateTime.UtcNow:g} UTC\n").SpacingAfter(20);

            var table = document.AddTable(historyEntries.Count + 1, _headers.Length);
            table.Design = TableDesign.TableGrid;

            for (int i = 0; i < _headers.Length; i++)
            {
                table.Rows[0].Cells[i].Paragraphs[0].Append(_headers[i]).Bold();
            }

            for (int i = 0; i < historyEntries.Count; i++)
            {
                var entry = historyEntries[i];
                var row = table.Rows[i + 1];

                row.Cells[0].Paragraphs[0].Append(entry.ChangedAt.ToString("g"));
                row.Cells[1].Paragraphs[0].Append(entry.ChangedByUserName);
                row.Cells[2].Paragraphs[0].Append(entry.TargetUserName);
                row.Cells[3].Paragraphs[0].Append(entry.Note);
                row.Cells[4].Paragraphs[0].Append(entry.RoleIdBefore?.ToString() ?? "-");
                row.Cells[5].Paragraphs[0].Append(entry.RoleIdAfter?.ToString() ?? "-");
                row.Cells[6].Paragraphs[0].Append(entry.ActiveBefore?.ToString() ?? "-");
                row.Cells[7].Paragraphs[0].Append(entry.ActiveAfter?.ToString() ?? "-");
            }

            document.InsertTable(table);
            document.Save();
        }
        
        return stream.ToArray();
    }
}