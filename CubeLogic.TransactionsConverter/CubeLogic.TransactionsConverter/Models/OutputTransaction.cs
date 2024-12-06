namespace CubeLogic.TransactionsConverter.Models;

 public record OutputTransaction
(
     string OrderId,
     string Type,
     int Revision,
     string DateTimeUtc,
     decimal Price,
     string Country,
     string InstrumentName
);