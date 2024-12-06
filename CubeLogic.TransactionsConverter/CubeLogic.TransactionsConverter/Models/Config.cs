namespace CubeLogic.TransactionsConverter.Models;

public record Config
(
    string Timezone,
    List<Instrument> Instruments
);