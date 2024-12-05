using FluentResults;
using Newtonsoft.Json;

namespace CubeLogic.TransactionsConverter.FileValidators;

public class JsonFileValidator : IFileValidator
{
    public Result ValidateFile(string filePath)
    {
        try
        {
            JsonTextReader reader = new JsonTextReader(new StreamReader(filePath));
            while (reader.Read()) { }
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Invalid JSON format in file: {filePath}. Details: {ex.Message}");
        }
    }
}