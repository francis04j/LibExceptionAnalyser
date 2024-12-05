using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;

namespace CubeLogic.TransactionsConverter.FileValidators;

public class CsvFileValidator : IFileValidator
{
    public Result ValidateFile(string filePath)
    {
        try
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Read();
                csv.ReadHeader();
            }
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Invalid CSV format in file: {filePath}. Details: {ex.Message}");
        }
    }
}