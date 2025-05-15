using API_ZAD_1.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API_ZAD_1.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";

    // Wszystkie wycieczki
    public async Task<IActionResult> getAllTrips()
    {
        var toBeReturned = new Dictionary<int, TripsDTO>();

        // Wyszukuje wszystkie wycieczki wraz z krajami w których się odbywają
        string query = @"
            SELECT T.IdTrip, T.Name, T.Description, T.DateFrom, T.DateTo, T.MaxPeople, 
                   C.IdCountry, C.Name AS CountryName
            FROM Trip T
            JOIN Country_Trip CT ON T.IdTrip = CT.IdTrip
            JOIN Country C ON CT.IdCountry = C.IdCountry";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int tripId = reader.GetInt32(reader.GetOrdinal("IdTrip"));
            if (!toBeReturned.ContainsKey(tripId))
            {
                toBeReturned[tripId] = new TripsDTO
                {
                    id = tripId,
                    name = reader.GetString(reader.GetOrdinal("Name")),
                    description = reader.GetString(reader.GetOrdinal("Description")),
                    start_date = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                    end_date = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                    max_people = reader.GetInt32(reader.GetOrdinal("MaxP+eople")),
                    countries = new List<CountriesDTO>()
                };
            }

            var country = new CountriesDTO
            {
                country_code = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                country_name = reader.GetString(reader.GetOrdinal("CountryName"))
            };

            toBeReturned[tripId].countries.Add(country);
        }
        
        return new OkObjectResult(toBeReturned.Values.ToList());
    }
    
    // SPRAWDZA CZY WYCIECZK ISTNIEJE
    public async Task<bool> DoesTripExist(int idTrip)
{
    const string query = "SELECT COUNT(1) FROM Trip WHERE IdTrip = @IdTrip"; // SPRAWDZA CZY WYCIECZK ISTNIEJE
    using var conn = new SqlConnection(_connectionString);
    using var cmd = new SqlCommand(query, conn);
    cmd.Parameters.AddWithValue("@IdTrip", idTrip);
    await conn.OpenAsync();
    return (int)await cmd.ExecuteScalarAsync() > 0;
}

    
    // SPRAWDZA CZY WYCIECZKA JEST PEŁNA
public async Task<bool> IsTripFull(int idTrip)
{
    const string query = @"
        SELECT T.MaxPeople, COUNT(CT.IdClient) AS CurrentCount
        FROM Trip T
        LEFT JOIN Client_Trip CT ON T.IdTrip = CT.IdTrip
        WHERE T.IdTrip = @IdTrip
        GROUP BY T.MaxPeople"; //POBIERA MAX LICZBĘ OSÓB ORAZ ILOŚC OBECNIE ZAREJESTROWANYCH

    using var conn = new SqlConnection(_connectionString);
    using var cmd = new SqlCommand(query, conn);
    cmd.Parameters.AddWithValue("@IdTrip", idTrip);
    await conn.OpenAsync();

    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return false;

    int maxPeople = reader.GetInt32(0);
    int currentCount = reader.GetInt32(1);
    return currentCount >= maxPeople;
}

public async Task<bool> IsClientAlreadyRegistered(int idClient, int idTrip)
{
    const string query = "SELECT COUNT(1) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip"; // SPRAWDZA CZY ISTNIEJE KLIENT ZAREJESTROWANY NA TĄ WYCIECZKĘ

    using var conn = new SqlConnection(_connectionString);
    using var cmd = new SqlCommand(query, conn);
    cmd.Parameters.AddWithValue("@IdClient", idClient);
    cmd.Parameters.AddWithValue("@IdTrip", idTrip);
    await conn.OpenAsync();
    return (int)await cmd.ExecuteScalarAsync() > 0;
}

public async Task assignClientToTrip(int idClient, int idTrip)
{
    string query = "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate) VALUES (@IDClient, @IDTrip, @RegisteredAt, @PaymentDate)";
        
    using var conn = new SqlConnection(_connectionString);
    using var cmd = new SqlCommand(query, conn);
    
    cmd.Parameters.AddWithValue("@IDClient", idClient);
    cmd.Parameters.AddWithValue("@IDTrip", idTrip);
    
    int registeredAt = (int)((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
    cmd.Parameters.AddWithValue("@RegisteredAt", registeredAt);
    cmd.Parameters.AddWithValue("@PaymentDate", DBNull.Value);
    
    await conn.OpenAsync();
    await cmd.ExecuteNonQueryAsync();
}

public async Task deleteClientFromTrip(int idClient, int idTrip)
{
    string query = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";

    using var conn = new SqlConnection(_connectionString);
    using var cmd = new SqlCommand(query, conn);

    cmd.Parameters.AddWithValue("@IdClient", idClient);
    cmd.Parameters.AddWithValue("@IdTrip", idTrip);

    await conn.OpenAsync();
    await cmd.ExecuteNonQueryAsync();
}

}
