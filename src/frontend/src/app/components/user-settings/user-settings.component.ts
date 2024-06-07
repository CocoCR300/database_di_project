import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { ActivatedRoute } from '@angular/router';
import { User } from '../../models/user';
import { firstValueFrom, merge } from 'rxjs';
import { FormsModule, ReactiveFormsModule, FormControl, Validators, NgForm, NgModel, FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { NotificationService } from '../../services/notification.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-user-settings',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatButton],
  templateUrl: './user-settings.component.html',
  styleUrl: './user-settings.component.css'
})
export class UserSettingsComponent implements OnInit {
  user!:User;
  email = new FormControl('', [Validators.required, Validators.email]);
  errorMessage = '';
  userModifyFormGroup!:FormGroup;

  public constructor(
    private _userService:UserService,
    private _route:ActivatedRoute,
    private _notificationService: NotificationService
  ){
    merge(this.email.statusChanges, this.email.valueChanges)
    .pipe(takeUntilDestroyed())
    .subscribe(() => this.updateErrorMessage())
  }

  updateErrorMessage(){
    if(this.email.hasError('required')) {
      this.errorMessage = 'Debe ingresar un correo';
    } else if(this.email.hasError('email')) {
      this.errorMessage = 'Ingrese un correo valido';
    } else{
      this.errorMessage = '';
    }
  }

  async ngOnInit() {
    let userName = this._route.snapshot.paramMap.get('name');
    if (userName !== null) {
      this.user = await firstValueFrom(this._userService.getUser(userName));
    }
    this.email.setValue(this.user.person!.emailAddress);
  }

  public onSubmitUserSettings(){
    let userName = this._route.snapshot.paramMap.get('name');
    let first_name = this.userModifyFormGroup.get<string>("first_name")!;
    let last_name = this.userModifyFormGroup.get<string>("last_name")!;
    let email_user = this.userModifyFormGroup.get<string>("email")!;
    let phone_number = this.userModifyFormGroup.get<string>("phone_number")!;

    if(this.userModifyFormGroup.invalid){
      if(first_name.hasError("required")) {
        this._notificationService.show("El nombre debe tener un valor");
      }
      if(last_name.hasError("required")) {
        this._notificationService.show("El apellido debe tener un valor");
      }
      if(phone_number.hasError("required")) {
        this._notificationService.show("El numero de telefono debe tener un valor");
      }

      return;
    }

    let data = [];
    if(this.user.firstName===first_name.value){
      data[0] = '';
    }else{
      data[0] = first_name.value;
    }
    if(this.user.lastName===last_name.value){
      data[1] = '';
    }else{
      data[1] = last_name.value;
    }
    if(this.user.emailAddress===email_user.value){
      data[2] = '';
    }else{
      data[2] = email_user.value;
    }
    // TODO
    //if(this.user.phone_number===phone_number.value){
    //  data[3] = 0;
    //}else{
    //  data[3] = phone_number.value;
    //}

    this._userService.updateUser(data,userName!).subscribe(
      response => {
        if(response.ok){
          Swal.fire({
            icon: "success",
            title: "Se modifico al usuario con exito"
          });
        }
        else {
          Swal.fire({
            icon: "error",
            title: "Ha ocurrido un error"
          });
        }
      }
    )
  }
}
