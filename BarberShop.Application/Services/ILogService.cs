namespace BarberShop.Application.Services
{
    public interface ILogService
    {
        Task SaveLogAsync(string logLevel, string source, string message, string? data = null, string? resourceId = null);
    }
}


