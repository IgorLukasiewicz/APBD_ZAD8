using API_ZAD_1.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API_ZAD_1.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";

    public async Task<IActionResult> getAllTripsForClient(int id)
    {
        var toBeReturned = new Dictionary<int, TripsDTO>();
        
        string query = @"SELECT 
                    T.IdTrip, T.Name AS TripName, T.Description, T.DateFrom, T.DateTo, T.MaxPeople,
                    C.IdCountry,ClT.RegisteredAt, ClT.PAYMENTDATE, C.Name as CountryName
                 FROM Trip T
                 JOIN Country_Trip CT ON T.IdTrip = CT.IdTrip
                 JOIN Country C ON CT.IdCountry = C.IdCountry
                 JOIN Client_Trip CLT ON T.IdTrip = CLT.IdTrip
                 JOIN Client CL ON CLT.IdClient = CL.IdClient
                 WHERE CL.IdClient = @id";  // WYSZUKUJE WSZYSTKIE WYCIECZKI DLA DANEGO KLIENTA

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);

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
                    name = reader.GetString(reader.GetOrdinal("TripName")),
                    description = reader.GetString(reader.GetOrdinal("Description")),
                    start_date = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                    end_date = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                    max_people = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                    registeredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                    paymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) ? null : reader.GetInt32(reader.GetOrdinal("PaymentDate")),
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
    public async Task<bool> DoesClientExist(int id)
    {
        string query = @"SELECT COUNT(1) FROM Client WHERE IdClient = @id";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await conn.OpenAsync();
        var result = (int)await cmd.ExecuteScalarAsync();
        
        return result > 0;
    }
    public async Task<ClientsDTO> AddClientRawAsync(ClientsDTO client)
    {
        string query = @"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        OUTPUT INSERTED.IdClient
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
        cmd.Parameters.AddWithValue("@LastName", client.LastName);
        cmd.Parameters.AddWithValue("@Email", client.Email);
        cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", client.Pesel);

        await conn.OpenAsync();
        client.idClient = (int)await cmd.ExecuteScalarAsync();

        return client;
    }

}