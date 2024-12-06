using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;

namespace CubeLogic.TransactionsConverter.Services;

public class CsvService
{
    public CsvReader GetCsvReader(string filePath)
    {
        var reader = new StreamReader(filePath);
        return new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });
    }

    public CsvWriter GetCsvWriter(string filePath)
    {
        var writer = new StreamWriter(filePath);
        return new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
    }
}