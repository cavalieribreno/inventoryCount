namespace Csinv.InventorySessions.DTOs;
// start inventory session request
public class SessionStartRequest
{
    public int Year { get; set; }
    public int? Month { get; set; }
}
// filter request for inventory sessions with pagination
public class SessionFilterRequest
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string? Status { get; set; }

    // Pagination properties
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
// response for inventory session details
public class SessionResponse
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public String? Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public DateTime? CancelDate { get; set; }
    public int TotalItems { get; set; }
}