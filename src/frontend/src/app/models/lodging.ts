import { Perk } from "./perk";
import { Room } from "./room";
import { RoomType } from "./room_type";
import { User } from "./user";

export class Lodging {
    constructor(
        public id:              number,
        public ownerId:         number,
        public name:            string,
        public description:     string,
        public address:         string,
        public type:            string,
        public emailAddress:    string,
        public phoneNumbers:    string[] | null,
        public photos:          string[] | null,
        public perks:           Perk[] | null,
        public rooms:           Room[] | null,
        public roomTypes:       RoomType[] | null,
        public owner:           User | null)
        { }
}