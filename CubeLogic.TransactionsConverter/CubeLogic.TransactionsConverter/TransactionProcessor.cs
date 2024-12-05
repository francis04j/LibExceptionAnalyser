using CubeLogic.TransactionsConverter.Models;
using FluentResults;
using TimeZoneConverter;

namespace CubeLogic.TransactionsConverter;

public class TransactionProcessor
{
    
    public Result<List<OutputTransaction>> ProcessTransactions(List<InputTransaction> inputTransactions, Config config)
    {
        var outputTransactions = new List<OutputTransaction>();
        var revisionCounters = new Dictionary<string, int>();

        foreach (var record in inputTransactions)
        {
            if (!revisionCounters.ContainsKey(record.OrderId))
            {
                revisionCounters[record.OrderId] = 0;
            }

            revisionCounters[record.OrderId]++;

            Result<DateTime> utcDateTimeResult = ConvertToUtc(record.DateTime, config.Timezone);
            if (utcDateTimeResult.IsFailed)
            {
                return Result.Fail<List<OutputTransaction>>(utcDateTimeResult.Errors[0].Message);
            }

            var utcDateTime = utcDateTimeResult.Value;

            var instrument = config.Instruments.Find(i => i.InstrumentId == record.InstrumentId);

            string country = "Error";
            string instrumentName = "Error";

            if (instrument != null)
            {
                country = instrument.Country;
                instrumentName = instrument.InstrumentName;
            }
            else
            {
                Console.WriteLine($"Warning: InstrumentId {record.InstrumentId} not found in config.json.");
            }

            Result<string> typeResult = MapType(record.Type);
            if (typeResult.IsFailed)
            {
                return Result.Fail<List<OutputTransaction>>(typeResult.Errors[0].Message);
            }

            var type = typeResult.Value;

            outputTransactions.Add(new OutputTransaction
            {
                OrderId = record.OrderId,
                Type = type,
                Revision = revisionCounters[record.OrderId],
                DateTimeUtc = utcDateTime.ToString("o"),
                Price = record.Price,
                Country = country,
                InstrumentName = instrumentName
            });
        }

        return Result.Ok(outputTransactions);
    }

    private Result<DateTime> ConvertToUtc(string dateTime, string timezone)
    {
        try
        {
            var parsedDateTime = DateTime.Parse(dateTime);
            var localDateTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Unspecified);
            var timeZoneInfo = TZConvert.GetTimeZoneInfo(timezone);
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