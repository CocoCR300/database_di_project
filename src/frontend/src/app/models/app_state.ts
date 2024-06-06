import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BaseService } from '../services/base.service';
import { AppResponse } from './app_response';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})
export class AppState
{   
    httpService!:BaseService;
    public constructor(private router: Router) {
        
    }

    public get isUserLogged() {
        return this.token !== null;
    }

    public get userName() {
        return sessionStorage.getItem('user_name');
    }

    public set userName(userName: string | null) {
        this.setOrRemoveFromSessionStorage('user_name', userName);
    }

    public get role() {
        const role = sessionStorage.getItem('role');

        if (role === null) {
            return null;
        }

        return parseInt(role);
    }

    public set role(role: number | null) {
        this.setOrRemoveFromSessionStorage('role', role);
    }

    public get token() {
        return sessionStorage.getItem('token');
    }

    public set token(token: string | null) {
        this.setOrRemoveFromSessionStorage('token', token);
    }

    logIn(token: string, userName: string, role: number): void {
        this.token = token;
        this.userName = userName;
        this.role = role;

        this.router.navigate(["/home"]);
    }

    logOut(): void {
        this.role = null;
        this.token = null;
        this.userName = null;

        this.router.navigate(["/login"]);
    }

    private setOrRemoveFromSessionStorage(name: string, value: any) {
        if (value !== null) {
            sessionStorage.setItem(name, value);
        }
        else {
            sessionStorage.removeItem(name);
        }
    }
}