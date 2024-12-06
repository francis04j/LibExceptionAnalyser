using CubeLogic.TransactionsConverter.Models;
using CubeLogic.TransactionsConverter.Processors;
//using Instrument = System.Diagnostics.Metrics.Instrument;

namespace CubeLogic.TransactionsConverter.UnitTests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using FluentResults;
using Newtonsoft.Json;
using Xunit;

public class TransactionProcessorTests
{
    [Fact]
    public void ProcessTransactions_ShouldHandleValidInputCorrectly()
    {
        // Arrange
        Config config = new( "America/New_York", new List<Instrument>
        {
            new(1, "StockA",  "USA" ),
            new (  2, "StockB", "Canada" )
        });
        var inputCsv = @"OrderId,Type,DateTime,Price,InstrumentId
1,AddOrder,2024-12-01T10:00:00,100.5,1
1,UpdateOrder,2024-12-01T11:00:00,100.5,1
1,UpdateOrder,2024-12-01T12:00:00,101.5,1
2,AddOrder,2024-12-01T10:30:00,200.0,2
2,UpdateOrder,2024-12-01T11:30:00,200.0,2
2,UpdateOrder,2024-12-01T12:30:00,201.0,2";

        var inputPath = "test_input.csv";
        var outputPath = "test_output.csv";

        File.WriteAllText(inputPath, inputCsv);
        var transactionProcessor = new TransactionProcessor();

        // Act
        var result = transactionProcessor.ProcessTransactions(inputPath, outputPath, config);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var outputLines = File.ReadAllLines(outputPath);
        outputLines.Length.Should().Be(7); // Header + 6 transactions
        outputLines[1].Should().Contain("1,ADD,1");
        outputLines[2].Should().Contain("1,UPDATE,2");
        outputLines[3].Should().Contain("2,ADD,1");
        outputLines[4].Should().Contain("2,UPDATE,2");

        // Clean up
        File.Delete(inputPath);
        File.Delete(outputPath);
    }

    [Fact]
    public void ProcessTransactions_ShouldSkipDuplicateUpdates()
    {
        // Arrange
        Config config = new( "America/New_York", new List<Instrument>
        {
            new(1, "StockA",  "USA" )
        });
        var inputCsv = @"OrderId,Type,DateTime,Price,InstrumentId
1,AddOrder,2024-12-01T10:00:00,100.5,1
1,UpdateOrder,2024-12-01T11:00:00,100.5,1
1,UpdateOrder,2024-12-01T12:00:00,101.5,1
1,UpdateOrder,2024-12-01T13:00:00,101.5,1";

        var inputPath = "test_input.csv";
        var outputPath = "test_output.csv";

        File.WriteAllText(inputPath, inputCsv);
        var transactionProcessor = new TransactionProcessor();

        // Act
        var result = transactionProcessor.ProcessTransactions(inputPath, outputPath, config);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var outputLines = File.ReadAllLines(outputPath);
        outputLines.Length.Should().Be(4); // Header + 3 transactions
        outputLines[1].Should().Contain("1,ADD,1");
        outputLines[2].Should().Contain("1,UPDATE,2");

        // Clean up
        File.Delete(inputPath);
        File.Delete(outputPath);
    }

    [Fact]
    public void ProcessTransactions_ShouldHandleMissingInstrumentGracefully()
    {
        // Arrange
        Config config = new( "America/New_York", new List<Instrument>()
        );
        
        var inputCsv = @"OrderId,Type,DateTime,Price,InstrumentId
1,AddOrder,2024-12-01T10:00:00,100.5,1";

        var inputPath = "test_input.csv";
        var outputPath = "test_output.csv";

        File.WriteAllText(inputPath, inputCsv);
        var transactionProcessor = new TransactionProcessor();

        // Act
        var result = transactionProcessor.ProcessTransactions(inputPath, outputPath, config);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var outputLines = File.ReadAllLines(outputPath);
        outputLines.Length.Should().Be(2); // Header + 1 transaction
        outputLines[1].Should().Contain("Error");

        // Clean up
        File.Delete(inputPath);
        File.Delete(outputPath);
    }

    [Fact]
    public void ProcessTransactions_PerformanceTest()
    {
        // Arrange
        Config config = new( "America/New_York", new List<Instrument>
        {
            new(1, "StockA",  "USA" ),
            new (  2, "StockB", "Canada" )
        });

        var inputLines = new List<string> { "OrderId,Type,DateTime,Price,InstrumentId" };
        for (int i = 1; i <= 1000000; i++)
        {
            inputLines.Add($"{i},AddOrder,2024-12-01T10:00:00,{i * 10},1");
            inputLines.Add($"{i},UpdateOrder,2024-12-01T11:00:00,{i * 10 + 1},1");
        }

        var inputCsv = string.Join(Environment.NewLine, inputLines);
        var inputPath = "test_input_large.csv";
        var outputPath = "test_output_large.csv";

        File.WriteAllText(inputPath, inputCsv);
        var transactionProcessor = new TransactionProcessor();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = transactionProcessor.ProcessTransactions(inputPath, outputPath, config);
        stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // Ensure processing takes less than 60 seconds

        // Clean up
        File.Delete(inputPath);
        File.Delete(outputPath);
    }
}
