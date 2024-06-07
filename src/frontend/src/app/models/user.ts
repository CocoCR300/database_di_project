import { Person } from "./person";

export class User {
    constructor(
        public name:        string,
        public password:    string | null,
        public roleId:      string,
        public person:      Person,
        public role:        UserRole)
        { }
}

export enum UserRole {
    Administrator = 1,
    Customer = 2, 
    Lessor = 3,
}