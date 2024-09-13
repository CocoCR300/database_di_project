using System.Diagnostics.Contracts;
using Restify.API.Models;

namespace Restify.API.Util;

public static class StandardFilters
{
    [Pure]
    public static IQueryable<Booking> BookingByLodging(IQueryable<Booking> bookings, int? lodgingId)
    {
        if (lodgingId.HasValue)
        {
            bookings = bookings.Where(b => b.LodgingId == lodgingId);
        }

        return bookings;
    }
    
    [Pure]
    public static IQueryable<Booking> BookingByDates(IQueryable<Booking> bookings, DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue)
        {
            bookings = bookings.Where(b => b.RoomBookings.Any(r => r.StartDate >= startDate));
        }
        
        if (endDate.HasValue)
        {
            bookings = bookings.Where(b => b.RoomBookings.Any(r => r.EndDate <= endDate));
        }

        return bookings;
    }
}