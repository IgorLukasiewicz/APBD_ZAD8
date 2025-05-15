using System.Text.RegularExpressions;
using API_ZAD_1.Models.DTOs;
using API_ZAD_1.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_ZAD_1.Controllers;


[ApiController]
[Route("api/[controller]")]

public class ClientsController : ControllerBase
{
    
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";
    
    private readonly IClientsService _service;

    public ClientsController(IClientsService service)
    {
        _service = service;
    }

    // ENDPOINT ZWRACA INFORMACJE O WYCIECZKACH OSOBY O PODANYM ID
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetAllTripsForClients(int id)
    {
        return await _service.getAllTripsForClient(id);
    }
    
    // ENDPOINT POZWALAJĄCY DODAĆ NOWEGO KLIENTA
    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] ClientsDTO client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            string.IsNullOrWhiteSpace(client.Email) ||
            string.IsNullOrWhiteSpace(client.Telephone) ||
            string.IsNullOrWhiteSpace(client.Pesel))
        {
            return BadRequest("All fields are required.");
        }

        if (client.Pesel == null || client.Pesel.Length != 11 || !client.Pesel.All(char.IsDigit))
        {
            return BadRequest("PESEL must be exactly 11 digits.");
        }

        if (client.Email == null || !client.Email.Contains('@') || !client.Email.Contains('.'))
        {
            return BadRequest("Invalid email format.");
        }
        
        var newClient = await _service.AddClientRawAsync(client);

        return CreatedAtAction(nameof(AddClient), new { id = newClient.idClient }, newClient);
    }

    
}