namespace Pedidos.Infrastructure.Options;

public sealed class FeatureFlagOptions
{
    public const string SectionName = "FeatureFlags";

    /// <summary>
    /// Quando true, usa a calculadora da reforma tributária (20%).
    /// Quando false, usa a calculadora atual (30%).
    /// </summary>
    public bool UsarReformaTributaria { get; set; } = false;
}
