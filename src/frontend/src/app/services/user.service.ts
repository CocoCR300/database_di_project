import { HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { User } from "../models/user";
import { Observable } from "rxjs";
import { BaseService } from "./base.service";
import { AppResponse } from "../models/app_response";
import { AppState } from "../models/app_state";
import { TokenIdentity } from "../models/token_identity";

@Injectable({
    providedIn: 'root'
})
export class UserService extends BaseService{
    private users!: User[];
    protected override _appState!: AppState;


    getUsers(): Observable<User[]> {
        return this.get<User[]>("User");
    }

    getUser(name: string): Observable<User> {
        return this.get(`user/${name}`);
    }

    signup(user: User): Observable<any> {
        let headers = new HttpHeaders().set('Content-Type', 'application/json');
        return this._http.post(this.urlAPI + 'User/signup', user, {headers})
      }

      uploadProfilePhoto(file: File, userName: string): Observable<any> {
        const formData = new FormData();
        formData.append('file', file);
        return this._http.post(`${this.urlAPI}user/${userName}/image`, formData);
      }

      login(user: User): Observable<any> {
        let headers = new HttpHeaders().set('Content-Type', 'application/json');
        return this._http.post<any>(this.urlAPI + 'User/login', user, { headers });
      }


      getIdentityFromApi(token: string): Observable<TokenIdentity> {
        let headers = new HttpHeaders().set('Content-Type', 'application/json');
        return this._http.post<TokenIdentity>(this.urlAPI + 'User/identity', { token }, { headers });
      }

    deleteUser(userName: string): Observable<AppResponse>{
        return this.delete(`user/${userName}`, true, null);
    }

    updateUser(data:string[], username:string): Observable<AppResponse>{
        let user = {
            userName: username,
            firstName: data[0],
            lastName: data[1],
            emailAddress: data[2],
        }

        return this.patch(`user/${username}`, true, user);
    }

    addPhoneNumbers(username: string, phoneNumbers: string[]) {
        return this.post(`user/${username}/phone_number`, true, phoneNumbers);
    }

    deletePhoneNumbers(username: string, phoneNumbers: string[]) {
        return this.delete(`user/${username}/phone_number`, true, phoneNumbers);
    }

    logOut(){
        this.post(`user/${this._appState.userName}/logout`,true,'').subscribe((response : AppResponse) => {
            if(response.ok){
                console.log('Sesion cerrada con exito');
            }
        })
        this._appState.logOut();
    }
}
