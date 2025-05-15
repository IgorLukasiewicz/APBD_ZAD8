using API_ZAD_1.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API_ZAD_1.Services;

public interface ITripsService
{
    Task<IActionResult> getAllTrips();
    Task<bool> DoesTripExist(int idTrip);
    Task<bool> IsTripFull(int idTrip);
    Task<bool> IsClientAlreadyRegistered(int idClient, int idTrip);
    public Task assignClientToTrip(int idClient, int idTrip);
    public Task deleteClientFromTrip(int idClient, int idTrip);
}
