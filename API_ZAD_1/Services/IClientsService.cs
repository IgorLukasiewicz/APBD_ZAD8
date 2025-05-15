using API_ZAD_1.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API_ZAD_1.Services;

public interface IClientsService
{
    public Task<IActionResult> getAllTripsForClient(int id);
    public  Task<bool> DoesClientExist(int id);

    public Task<ClientsDTO> AddClientRawAsync(ClientsDTO client); 
    
}