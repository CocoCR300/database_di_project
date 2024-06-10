import { UserRole } from "./user_role";

export class User {
    constructor(
        public userName:        string,
        public password:        string | null,
        public roleId:          number,
        public personId:        number,
        public firstName:       string,
        public lastName:        string,
        public emailAddress:    string,
        public roleName:         string,
        public role:            UserRole | null,
        public phoneNumbers:    string[] | null)
        { }
}

export enum UserRoleEnum {
    Administrator = 1,
    Customer = 2, 
    Lessor = 3,
}