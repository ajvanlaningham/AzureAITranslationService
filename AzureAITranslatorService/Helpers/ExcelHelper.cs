using AzureAITranslatorService.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

public static class ExcelHelper
{
    static ExcelHelper()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public static void CreateExcelFile(string resxFilePath, List<ResxEntry> resxEntries)
    {
        string excelFilePath = Path.ChangeExtension(resxFilePath, ".xlsx");

        using (ExcelPackage package = new ExcelPackage())
        {
            // Initially, we just create the file without adding any data
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Resources");

            FileInfo fileInfo = new FileInfo(excelFilePath);
            package.SaveAs(fileInfo);
        }

        Console.WriteLine($"Excel file created: {excelFilePath}");
    }
}