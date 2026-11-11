namespace HouseBroker.Application.Listings;

/// <summary>Looks up broker contact details (stored with the Identity user in Infrastructure).</summary>
public interface IBrokerDirectory
{
    Task<BrokerContactDto?> GetContactAsync(string brokerId, CancellationToken ct);
}
