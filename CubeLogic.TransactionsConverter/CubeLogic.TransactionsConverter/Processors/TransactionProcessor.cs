using System.Collections.Concurrent;
using CubeLogic.TransactionsConverter.Models;
using CubeLogic.TransactionsConverter.Services;
using FluentResults;
using TimeZoneConverter;

namespace CubeLogic.TransactionsConverter.Processors;

public class TransactionProcessor
{


    public Result ProcessTransactions(string inputPath, string outputPath, Config config)
    {
        var instrumentsDict = config.Instruments.ToDictionary(i => i.InstrumentId);
        var timeZoneInfo = TZConvert.GetTimeZoneInfo(config.Timezone);
        var revisionCounters = new ConcurrentDictionary<string, int>();
        var lastProcessedPrice = new ConcurrentDictionary<string, decimal?>();

        try
        {
            using var csvReader = new CsvService().GetCsvReader(inputPath);
            using var csvWriter = new CsvService().GetCsvWriter(outputPath);

            csvReader.Read();
            csvReader.ReadHeader();
            csvWriter.WriteHeader<OutputTransaction>();
            csvWriter.NextRecord();

            foreach (var record in csvReader.GetRecords<InputTransaction>())
            {
                var utcDateTimeResult = ConvertToUtc(record.DateTime, timeZoneInfo);
                if (utcDateTimeResult.IsFailed)
                {
                    Console.WriteLine(utcDateTimeResult.Errors[0].Message);
                    continue;
                }

                var utcDateTime = utcDateTimeResult.Value;

                instrumentsDict.TryGetValue(record.InstrumentId, out var instrument);
                string country = instrument?.Country ?? "Error";
                string instrumentName = instrument?.InstrumentName ?? "Error";

                if (instrument == null)
                {
                    Console.WriteLine($"Warning: InstrumentId {record.InstrumentId} not found in config.json.");
                }

                var typeResult = MapType(record.Type);
                if (typeResult.IsFailed)
                {
                    Console.WriteLine(typeResult.Errors[0].Message);
                    continue;
                }

                var type = typeResult.Value;

                if (type == "UPDATE")
                {
                    var key = record.OrderId;

                    // Check if price has changed
                    var lastPrice = lastProcessedPrice.GetOrAdd(key, _ => null);
                    if (lastPrice == record.Price)
                    {
                        continue; // Skip if price has not changed
                    }

                    // Update the revision counter and last processed price
                    lastProcessedPrice[key] = record.Price;
                    revisionCounters.AddOrUpdate(key, 1, (key, current) => current + 1);
                }

                var revision = revisionCounters.GetOrAdd(record.OrderId, 1);

                var outputTransaction = new OutputTransaction
                (
                    record.OrderId,
                     type,
                     revision,
                     utcDateTime.ToString("o"),
                    record.Price,
                     country,
                     instrumentName
                );

                csvWriter.WriteRecord(outputTransaction);
                csvWriter.NextRecord();
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to process transactions: {ex.Message}");
        }
    }

    private Result<DateTime> ConvertToUtc(string dateTime, TimeZoneInfo timeZoneInfo)
    {
        try
        {
            var parsedDateTime = DateTime.Parse(dateTime);
            var localDateTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Unspecified);
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZoneInfo);
            return Result.Ok(utcDateTime);
        }
        catch (Exception ex)
        {
            return Result.Fail<DateTime>($"Failed to convert DateTime to UTC: {ex.Message}");
        }
    }
    
    private Result<string> MapType(string type)
    {
        try
        {
            return type switch
            {
                "AddOrder" => Result.Ok("ADD"),
                "UpdateOrder" => Result.Ok("UPDATE"),
                "DeleteOrder" => Result.Ok("DELETE"),
                _ => Result.Fail<string>($"Unknown Type value: {type}")
            };
        }
        catch (Exception ex)
        {
            return Result.Fail<string>($"Error mapping type: {ex.Message}");
        }
    }
}