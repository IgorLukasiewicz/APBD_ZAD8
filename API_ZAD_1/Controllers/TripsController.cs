using API_ZAD_1.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_ZAD_1.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";
    private readonly ITripsService _service;
    
    public TripsController(ITripsService service)
    {
        _service = service;
    }
    
    // ENDPOINT ZWRACA INFORMACJE O WSZYSTKICH WYCIECZKACH ORAZ KRAJACH W KTÓRYCH SIĘ ODBYWAJĄ
    [HttpGet]
    public async Task<IActionResult> getAllTrips()
    {
        return await _service.getAllTrips();
    }
    
    
    // ENDPOINT DODAJE DO CLIENT_TRIP KLIENTA O PODANYM ID ORAZ WYCIECZKE O PODANYM ID
    [HttpPut("/api/clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientToTrip(int id, int tripId, [FromServices] IClientsService clientsService)
    {
        if (!await clientsService.DoesClientExist(id))
            return NotFound("Klient z tym ID nie istnieje.");

        if (!await _service.DoesTripExist(tripId))
            return NotFound("Taka wycieczka nie istnieje.");

        if (await _service.IsTripFull(tripId))
            return BadRequest("Ta wycieczka jest już pełna");

        if (await _service.IsClientAlreadyRegistered(id, tripId))
            return Conflict("Ten Klient już jest zarejestrowany na tą wycieczkę");

        await _service.assignClientToTrip(id, tripId);
        return Ok($"Klient o ID: {id}, Został dodany na wycieczkę o ID: {tripId}");
    }

    
    // ENDPOINT USUWA KLIENTA Z WYCIECZKI
    [HttpDelete("/api/clients/{id}/trips/{tripId}")]
    public async Task<IActionResult> deleteClientFromTrip(int id, int tripId, [FromServices] IClientsService clientsService)
    {
        if (!await clientsService.DoesClientExist(id))
            return NotFound("Klient z tym ID nie istnieje.");

        if (!await _service.DoesTripExist(tripId))
            return NotFound("Taka wycieczka nie istnieje.");
        
        if (!await _service.IsClientAlreadyRegistered(id, tripId))
            return Conflict("Ten Klient NIE jest zarejestrowany na tą wycieczkę");

        await _service.deleteClientFromTrip(id, tripId);
        return Ok($"Klient o ID: {id}, Został usunięty z wycieczki o ID: {tripId}");
    }
    
}