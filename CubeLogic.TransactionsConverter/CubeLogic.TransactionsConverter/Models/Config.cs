namespace CubeLogic.TransactionsConverter.Models;

public class Config
{
    public Config(string timezone, List<Instrument> instruments)
    {
        Timezone = timezone;
        Instruments = instruments;
    }

    public required string Timezone { get; init; }
    public required List<Instrument> Instruments { get; init; }
}