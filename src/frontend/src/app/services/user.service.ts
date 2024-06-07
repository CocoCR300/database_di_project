import { HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { User } from "../models/user";
import { Observable, from } from "rxjs";
import { BaseService } from "./base.service";
import { Lessor } from "../models/lessor";
import { Customer } from "../models/customer";
import { Person } from "../models/person";
import { AppResponse } from "../models/app_response";
import { AppState } from "../models/app_state";

@Injectable({
    providedIn: 'root'
})
export class UserService extends BaseService{
    private users!: User[];
    private lessors!: Lessor[];
    private administrators!: Administrator[];
    private customers!: Customer[];
    protected override _appState!: AppState;

    
    async initializeArray(){
        await Promise.all([
            this.get<User[]>("user").toPromise(),
            this.get<Lessor[]>("lessor").toPromise(),
            this.get<Administrator[]>("administrator").toPromise(),
            this.get<Customer[]>("customer").toPromise()
        ]).then((values) =>{
                this.users = values[0] as User[];
                this.lessors = values[1] as Lessor[];
                this.administrators = values[2] as Administrator[];
                this.customers = values[3] as Customer[];
            })
            this.users.forEach(user => {
                if(parseInt(user.role_id) === 1){
                    let administratorAux = this.administrators.find(administratorAux => administratorAux.user_name === user.name);
                    if(administratorAux){
                        user.first_name = administratorAux.first_name;
                        user.last_name = administratorAux.last_name;
                        user.phone_number = administratorAux.phone_number;
                        user.role_id = 'Administrador';
                    }
                }else if(parseInt(user.role_id) === 2){
                    let customerAux = this.customers.find(customerAux => customerAux.user_name === user.name);
                    if(customerAux){
                        user.first_name = customerAux.first_name;
                        user.last_name = customerAux.last_name;
                        user.phone_number = customerAux.phone_number;
                        user.role_id = 'Cliente';
                    }
                }else if(parseInt(user.role_id) === 3){
                    let customerAux = this.lessors.find(customerAux => customerAux.user_name === user.name);
                    if(customerAux){
                        user.first_name = customerAux.first_name;
                        user.last_name = customerAux.last_name;
                        user.phone_number = customerAux.phone_number;
                        user.role_id = 'Arrendador';
                    }
                }
            });
    }

    getUsers(): Observable<User[]> {
        return from(this.initializeArray().then(() => {
            console.log(this.users);
            return this.users;
        }));
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
            console.log(response.message);
        })
        this._appState.logOut();
    }
}