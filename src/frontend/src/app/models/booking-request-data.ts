import { RoomRequest } from "./room-request";

export interface BookingRequestData {
    customerId: number;
    lodgingId: number;
    rooms: RoomRequest[];
}