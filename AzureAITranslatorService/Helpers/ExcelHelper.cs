using AzureAITranslatorService.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class ExcelHelper
{
    private static readonly TraceSource traceSource = new TraceSource("ExcelHelperTraceSource");

    static ExcelHelper()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public static void CreateExcelFile(string resxFilePath, List<ResxEntry> resxEntries)
    {
        string excelFilePath = Path.ChangeExtension(resxFilePath, ".xlsx");

        if (File.Exists(excelFilePath))
        {
            // Log a message that the file already exists
            traceSource.TraceEvent(TraceEventType.Information, 0, $"File already exists: {excelFilePath}");

            // Optionally, handle the file appropriately, e.g., delete, rename, or append
            File.Delete(excelFilePath);
            traceSource.TraceEvent(TraceEventType.Information, 0, $"Deleted existing file: {excelFilePath}");
        }

        using (ExcelPackage package = new ExcelPackage())
        {
            // Create the worksheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Resources");

            // Add headers
            worksheet.Cells[1, 1].Value = "Key";
            worksheet.Cells[1, 2].Value = "Value";
            worksheet.Cells[1, 3].Value = "Comment";

            // Add data
            for (int i = 0; i < resxEntries.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = resxEntries[i].Name;
                worksheet.Cells[i + 2, 2].Value = resxEntries[i].Value;
                worksheet.Cells[i + 2, 3].Value = resxEntries[i].Comment;
            }

            // Save the file
            FileInfo fileInfo = new FileInfo(excelFilePath);
            package.SaveAs(fileInfo);
        }

        traceSource.TraceEvent(TraceEventType.Information, 0, $"Excel file created: {excelFilePath}");
    }
}