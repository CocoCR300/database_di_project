export class User {
    constructor(
        public name: string,
        public first_name: string,
        public last_name: string,
        public password: string,
        public role_id: string,
        public email_address: string,
        public phone_number: string,
        public image: string,
        public person_id: number | null = null) {
    }
}

export enum UserRole {
    Administrator = 1,
    Customer = 2, 
    Lessor = 3,
}