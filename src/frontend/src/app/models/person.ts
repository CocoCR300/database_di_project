import { PhoneNumber } from "./phone_number";

export class Person
{
    constructor(
        public id:              number,
        public userName:        string,
        public administratorId: number,
        public firstName:       string,
        public lastName:        string,
        public emailAddress:    string,
        public phoneNumber:     PhoneNumber[] 
    ) { }
}