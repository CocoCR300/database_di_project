import { Person } from "./person";
import { UserRole } from "./user_role";

export class User {
    constructor(
        public name:            string,
        public password:        string | null,
        public roleId:          number,
        public firstName:       string,
        public lastName:        string,
        public emailAddress:    string,
        public person:          Person | null,
        public role:            UserRole | null)
        { }
}

export enum UserRoleEnum {
    Administrator = 1,
    Customer = 2, 
    Lessor = 3,
}