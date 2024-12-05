using CubeLogic.TransactionsConverter;
using CubeLogic.TransactionsConverter.FileValidators;
using CubeLogic.TransactionsConverter.Services;

// PLEASE NOTE: the input and config files are always copied into bin directory. This sis a build action
//CAN UPDATE TO WRITE/READ in Windows special Data folders
//https://stackoverflow.com/questions/57638466/how-to-create-text-file-in-net-project-folder-not-in-bin-debug-folder-where-is

/*
 *var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                 "log1.txt");
   using (StreamWriter file = File.AppendText(fileName))
   {
       file.WriteLine("Hello from the text file");
   }
 */

public class Program
{
    
    static void Main(string[] args)
    {
        string configPath = "config.json";
        string inputPath = "inputTransactions.csv";
        string outputPath = "output.csv";

        var configService = new ConfigService(new JsonFileValidator());
        var csvFileValidator = new CsvFileValidator();
        var csvService = new CsvService();
        var transactionProcessor = new TransactionProcessor();

        // Validate and load config.json
        var configResult = configService.LoadConfig(configPath);
        if (configResult.IsFailed)
        {
            Console.WriteLine(configResult.Errors[0].Message);
            return;
        }

        var config = configResult.Value;

        // Validate and load CSV input
        var csvValidationResult = csvFileValidator.ValidateFile(inputPath);
        if (csvValidationResult.IsFailed)
        {
            Console.WriteLine(csvValidationResult.Errors[0].Message);
            return;
        }

        var inputRecordsResult = csvService.ReadCsv<InputTransaction>(inputPath);
        if (inputRecordsResult.IsFailed)
        {
            Console.WriteLine(inputRecordsResult.Errors[0].Message);
            return;
        }

        var inputRecords = inputRecordsResult.Value;
        var processResult = transactionProcessor.ProcessTransactions(inputRecords, config);
        if (processResult.IsFailed)
        {
            Console.WriteLine(processResult.Errors[0].Message);
            return;
        }

        var outputRecords = processResult.Value;

        // Write output CSV
        var writeResult = csvService.WriteCsv(outputPath, outputRecords);
        if (writeResult.IsFailed)
        {
            Console.WriteLine(writeResult.Errors[0].Message);
            return;
        }

        Console.WriteLine("Processing complete. Output saved to " + outputPath);

    }
}
