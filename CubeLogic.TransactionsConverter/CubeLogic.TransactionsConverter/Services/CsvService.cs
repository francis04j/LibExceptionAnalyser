using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;

namespace CubeLogic.TransactionsConverter.Services;

public class CsvService
{
    public Result<List<T>> ReadCsv<T>(string filePath)
    {
        try
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = new List<T>(csv.GetRecords<T>());
                return Result.Ok(records);
            }
        }
        catch (Exception ex)
        {
            return Result.Fail<List<T>>($"Failed to read CSV file: {ex.Message}");
        }
    }

    public Result WriteCsv<T>(string filePath, List<T> records)
    {
        try
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(records);
            }
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to write CSV file: {ex.Message}");
        }
    }
}