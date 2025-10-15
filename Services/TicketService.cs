
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

public class TicketService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TicketService> _logger;
    private readonly AuditService _audit;

    public TicketService(AppDbContext context, ILogger<TicketService> logger, AuditService audit)
    {
        _context = context;
        _logger = logger;
        _audit = audit;
    }

    public async Task<Ticket> BuyTicket(int spectacleId, int seat, int userId, int? ticketId = null)
    {
        
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");
        
        if (user.is_blocked)
            throw new Exception("Пользователь заблокирован и не может покупать билеты.");

        var spectacle = await _context.Spectacles.FindAsync(spectacleId);
        if (spectacle == null)
            throw new Exception("Спектакль не найден.");

        
        if (spectacle.start_time - DateTime.UtcNow < TimeSpan.FromMinutes(10))
            throw new Exception("Нельзя купить билет за 10 минут до начала спектакля.");

        Ticket ticket;

        if (ticketId.HasValue)
        {           
            ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.id == ticketId.Value && t.user_id == userId && t.status == "reserved");

            if (ticket == null)
                throw new Exception("Забронированный билет не найден или уже истёк.");

            if (ticket.reserved_until <= DateTime.UtcNow)
                throw new Exception("Время бронирования истекло.");

            spectacle.seats_booked = Math.Max(0, spectacle.seats_booked - 1);
            spectacle.seats_bought++;
        }
        else
        {
            
            ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.spectacle_id == spectacleId && t.seat == seat && t.status == "available");

            if (ticket == null)
                throw new Exception("Место уже занято или не существует.");

            spectacle.seats_bought++;
        }

        ticket.status = "purchased";
        ticket.user_id = userId;
        ticket.reserved_until = null;

        spectacle.money_earned = Math.Max(0, spectacle.money_earned + ticket.price);

        //userVisits created automatically in background service

        await _context.SaveChangesAsync();
        await _audit.LogAction(userId, "BUY", "ticket", ticket.id, null, new { ticket.spectacle_id, ticket.seat, ticket.price });
        return ticket;
    }

    public async Task<Ticket> BookTicket(int spectacleId, int seat, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");
        
        if (user.is_blocked)
            throw new Exception("Пользователь заблокирован и не может бронировать билеты.");

        var spectacle = await _context.Spectacles.FindAsync(spectacleId);
        if (spectacle == null)
            throw new Exception("Спектакль не найден.");

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.spectacle_id == spectacleId && t.seat == seat && t.status == "available");

        if (ticket == null)
            throw new Exception("Место уже занято или не существует.");

        ticket.status = "reserved";
        ticket.user_id = userId;
        ticket.reserved_until = DateTime.UtcNow.AddMinutes(15);

        var hall = await _context.Halls.FirstOrDefaultAsync(h => h.id == spectacle.hall_id);
        if (hall == null)
            throw new Exception("Зал не найден для указанного спектакля.");
        var totalCapacity = hall.capacity;
        var currentBooked = spectacle.seats_booked;
        var currentBought = spectacle.seats_bought;
        
        if (currentBooked + currentBought < totalCapacity)
        {
            spectacle.seats_booked++;
        }
        else
        {
            throw new Exception("Нет свободных мест для бронирования.");
        }

        await _context.SaveChangesAsync();
        await _audit.LogAction(userId, "BOOK", "ticket", ticket.id, null, new { ticket.spectacle_id, ticket.seat, reserved_until = ticket.reserved_until });
        return ticket;
    }
}
