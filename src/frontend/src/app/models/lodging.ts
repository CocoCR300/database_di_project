import { Perk } from "./perk";
import { Person } from "./person";
import { PhoneNumber } from "./phone_number";
import { Photo } from "./photo";
import { Room } from "./room";
import { RoomType } from "./room_type";

export class Lodging {
    constructor(
        public id:              number,
        public ownerId:         number,
        public name:            string,
        public description:     string,
        public address:         string,
        public type:            string,
        public emailAddress:    string,
        public phoneNumbers:    PhoneNumber[] | null,
        public photos:          Photo[] | null,
        public perks:           Perk[] | null,
        public rooms:           Room[] | null,
        public roomTypes:       RoomType[] | null,
        public owner:           Person | null)
        { }
}