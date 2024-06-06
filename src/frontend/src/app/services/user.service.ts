import { HttpClient, HttpHeaders } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { server } from "./global";
import { User } from "../models/user";
import { appsettings} from "../settings/appsettings";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private http = inject(HttpClient);
    private apiUrl:string = appsettings.apiUrl + "User"
    constructor() {
        
    }

    lista(){
        return this.http.get<User[]>(this.apiUrl);
    }
}