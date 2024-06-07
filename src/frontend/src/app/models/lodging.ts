import { Perk } from "./perk";
import { Person } from "./person";
import { PhoneNumber } from "./phone_number";
import { Photo } from "./photo";
import { Room } from "./room";
import { RoomType } from "./room_type";

export class Lodging {
    constructor(
        public lodgingId: number,
        public ownerId: number,
        public name: string,
        public description: string,
        public address: string,
        public type: string,
        public emailAddress: string,
        public phoneNumbers: PhoneNumber[],
        public photos: Photo[],
        public perks: Perk[],
        public rooms: Room[],
        public roomTypes: RoomType[],
        public owner: Person)
        { }
}