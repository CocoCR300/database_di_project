import { HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { User } from "../models/user";
import { Observable } from "rxjs";
import { BaseService } from "./base.service";
import { AppResponse } from "../models/app_response";
import { AppState } from "../models/app_state";

@Injectable({
    providedIn: 'root'
})
export class UserService extends BaseService{
    private users!: User[];
    protected override _appState!: AppState;

    
    async initializeArray() {
    }

    getUsers(): Observable<User[]> {
        return this.get<User[]>("user");
    }

    getUser(name: string): Observable<User> {
        return this.get(`user/${name}`);
    }
    
    register(user: User): Observable<any> {
        let userJson = JSON.stringify(user);
        let params = 'data=' + userJson;
        let headers = new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded');
        return this._http.post(this.urlAPI + 'user', params, { headers });
      }
    
      uploadProfilePhoto(file: File, userName: string): Observable<any> {
        const formData = new FormData();
        formData.append('file', file);
        return this._http.post(`${this.urlAPI}user/${userName}/image`, formData);
      }

    login(user: User): Observable<any> {
        let userJson = JSON.stringify(user);
        let params = 'data=' + userJson
        let headers = new HttpHeaders().set('Content-type', 'application/x-www-form-urlencoded')
        let options = {
            headers
        }
        return this._http.post(this.urlAPI + 'user/login', params, options)
    }

    getIdentityFromApi(token: string): Observable<any> {
        let headers = new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded');
        headers = headers.set('Authorization', `Bearer ${token}`);

        const options = { headers };
        return this._http.get(this.urlAPI + 'user/identity', options);
    }

    deleteUser(userName: string): Observable<AppResponse>{
        return this.delete(`user/${userName}`, true, null);
    }

    updateUser(data:string[], username:string): Observable<AppResponse>{
        return this.patch(`user/${username}`,true,username,data[0],data[1],data[2],parseInt(data[3]));
    }

    logOut(){
        this.post(`user/${this._appState.userName}/logout`,true,'').subscribe((response : AppResponse) => {
            if(response.ok){
                console.log('Sesion cerrada con exito');
            }
            console.log(response.body.message);
        })
        this._appState.logOut();
    }
}