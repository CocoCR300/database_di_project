import { User } from "./user";

export class Lodging {
    constructor(
        public lodging_id: number,
        public lessor_id: number | null,
        public name: string,
        public description: string,
        public image: string,
        public address: string,
        public lessor: User | null,
        public per_night_price: number,
        public available_rooms: number)
        { }
}