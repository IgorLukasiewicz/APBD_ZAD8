namespace API_ZAD_1.Models.DTOs;

public class TripsDTO
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public DateTime start_date { get; set; }
    public DateTime end_date { get; set; }
    public int max_people { get; set; }
    public List<CountriesDTO> countries { get; set; }
    public int? registeredAt { get; set; }
    public int? paymentDate { get; set; }
}