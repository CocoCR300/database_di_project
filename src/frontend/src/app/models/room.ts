import { Lodging } from "./lodging";
import { RoomType } from "./room_type";

export class Room
{
    public constructor(
        public lodgingId:   number,
        public roomNumber:  number,
        public typeId:      number,
        public lodging:     Lodging,
        public type:        RoomType
    ) { }
}