 namespace CubeLogic.TransactionsConverter.Models;

 public record InputTransaction
 (
     string OrderId,
     string Type,
     string DateTime, 
     decimal Price,
     int InstrumentId 
 );