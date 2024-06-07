import { Booking } from "./booking";
import { Room } from "./room";

export class RoomBooking
{
    public constructor(
        public id:          number,
        public startDate:   string,
        public endDate:     string,
        public cost:        number,
        public discount:    number,
        public fees:        number,
        public status:      string,
        public bookingId:   string,
        public lodgingId:   number,
        public roomNumber:  number,
        public booking:     Booking,
        public room:        Room
    ) { }
}