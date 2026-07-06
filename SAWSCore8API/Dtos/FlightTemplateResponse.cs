namespace SAWSCore8API.DTOs.FlightTemplates
{
    public class FlightTemplateResponse
    {
        public int flightTemplateId { get; set; }
        public string templateName { get; set; } = string.Empty;
        public string flightNumber { get; set; } = string.Empty;
        public string departureICAO { get; set; } = string.Empty;
        public string? enRouteICAO { get; set; }
        public string destinationICAO { get; set; } = string.Empty;
        public string etd { get; set; } = string.Empty;
        public string ete { get; set; } = string.Empty;
    }
}
