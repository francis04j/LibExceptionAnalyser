using CubeLogic.TransactionsConverter.FileValidators;
using CubeLogic.TransactionsConverter.Models;
using FluentResults;
using Newtonsoft.Json;

namespace CubeLogic.TransactionsConverter.Services;

public class ConfigService
{
    private readonly IFileValidator _fileValidator;
    
    public ConfigService(IFileValidator fileValidator)
    {
        _fileValidator = fileValidator;
    }
    
    public Result<Config> LoadConfig(string filePath)
    {
        var validationResult = _fileValidator.ValidateFile(filePath);
        if (validationResult.IsFailed)
            return validationResult;

        try
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));
            return Result.Ok(config);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to parse config file: {ex.Message}");
        }
    }

}