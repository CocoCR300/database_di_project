import { Lodging } from "./lodging";
import { RoomType } from "./room_type";

export class Room
{
    public constructor(
        public lodgingId:   number,
        public number:  number,
        public typeId:      number,
        public lodging:     Lodging | null,
        public type:        RoomType | null
    ) { }
}