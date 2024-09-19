import { RoomRequest } from "./room-request";

export interface BookingRequestData {
    userName: string;
    lodgingId: number;
    rooms: RoomRequest[];
}