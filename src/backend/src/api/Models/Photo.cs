namespace Restify.API.Models;

public class Photo
{
    public byte     Ordering { get; set; }
    public string   FileName { get; set; }
}

public class LodgingPhoto : Photo
{
    public int LodgingId { get; set; }
}

public class RoomTypePhoto : Photo
{
    public int RoomTypeId { get; set; }
}
