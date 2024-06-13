import { Component, OnInit, inject } from '@angular/core';
import { UserService } from '../../services/user.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { User } from '../../models/user';
import { firstValueFrom, merge } from 'rxjs';
import { FormsModule, ReactiveFormsModule, FormControl, Validators, NgForm, NgModel, FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { NotificationService } from '../../services/notification.service';
import { AppResponse } from '../../models/app_response';
import {MatChipsModule, MatChipInputEvent, MatChipEditedEvent} from '@angular/material/chips';
import Swal from 'sweetalert2';
import { MatIconModule } from '@angular/material/icon';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { Dialogs } from '../../util/dialogs';
import { AppState } from '../../models/app_state';

@Component({
  selector: 'app-user-settings',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatButton, MatChipsModule, MatIconModule, RouterLink],
  templateUrl: './user-settings.component.html',
  styleUrl: './user-settings.component.css'
})
export class UserSettingsComponent implements OnInit {
  addOnBlur = true;
  separatorKeysCodes: number[] = [ENTER, COMMA];
  user!:User;
  isRootUser = false;
  userModifyFormGroup:FormGroup = this.buildFormGroup();
  phoneNumbers: string[] = [];
  phoneNumbersAdded: string[] = [];
  phoneNumbersDeleted: string[] = [];

  public constructor(
    private _appState: AppState,
    private _userService:UserService,
    private _route:ActivatedRoute,
    private _notificationService: NotificationService,
  ){ }

  async ngOnInit() {
    let userName = this._route.snapshot.paramMap.get('name');
    if (userName !== null) {
      this.user = await firstValueFrom(this._userService.getUser(userName));
      this.isRootUser = this.user.userName === "root";
      this.phoneNumbers = this.user.phoneNumbers!.slice();
    }
    this.userModifyFormGroup.get<string>("first_name")!.setValue(this.user.firstName);
    this.userModifyFormGroup.get<string>("last_name")!.setValue(this.user.lastName);
    this.userModifyFormGroup.get<string>("email_address")!.setValue(this.user.emailAddress);
    this.userModifyFormGroup.get<string>("phone_numbers")!.setValue(this.user.phoneNumbers);
  }

  
  async deleteUser() {
    let deleteUser = await Dialogs.showConfirmDialog(
      "¿Está seguro de que desea eliminar su usuario?",
      "Esta acción no se puede revertir."
    );

    if (!deleteUser) {
      return;
    }

    if (this.user.userName === "root") {
      await Swal.fire({
        icon: "error",
        title: "El usuario raíz no puede ser eliminado."
      });

      return;
    }

    const response = await firstValueFrom(this._userService.deleteUser(this.user.userName));

    if (response.ok) {
      this._appState.logOut();
    }
    else {
      await Swal.fire({
        icon: "error",
        title: "Ha ocurrido un error al eliminar su usuario"
      });
    }
  }

  addPhoneNumber(event: MatChipInputEvent) {
    const phoneNumber = (event.value) as string;

    if(phoneNumber===''){
      return;
    }

    if (!this.phoneNumbers.includes(phoneNumber)) {
      this.phoneNumbersAdded.push(phoneNumber);
      this.phoneNumbers.push(phoneNumber);
    }

    event.chipInput!.clear();
  }

  removePhoneNumber(phoneNumber: string): void {
    const index = this.phoneNumbers.indexOf(phoneNumber);

    if (index >= 0) {
      const phoneNumber = this.phoneNumbers.splice(index, 1);
      this.phoneNumbersDeleted.push(phoneNumber[0]);
    }
  }

  async onSubmitUserSettings(){
    let userName = this._route.snapshot.paramMap.get('name');
    let first_name = this.userModifyFormGroup.get<string>("first_name")!;
    let last_name = this.userModifyFormGroup.get<string>("last_name")!;
    let email_user = this.userModifyFormGroup.get<string>("email_address")!;
    let phone_number = this.userModifyFormGroup.get<string>("phone_number")!;

    if(this.userModifyFormGroup.invalid){
      if(first_name.hasError("required")) {
        this._notificationService.show("El nombre debe tener un valor.");
      }
      if(first_name.hasError("minLength")) {
        this._notificationService.show("El nombre debe tener 2 carácteres como mínimo.");
      }
      if(first_name.hasError("maxLength")) {
        this._notificationService.show("El nombre debe tener 50 carácteres como máximo.");
      }
      if(last_name.hasError("required")) {
        this._notificationService.show("El apellido debe tener un valor.");
      }
      if(last_name.hasError("minLength")) {
        this._notificationService.show("El apellido debe tener 2 carácteres como mínimo.");
      }
      if(last_name.hasError("maxLength")) {
        this._notificationService.show("El apellido debe tener 100 carácteres como máximo.");
      }
      if(phone_number.hasError("required")) {
        this._notificationService.show("El numero de teléfono debe tener un valor.");
      }
      if(email_user.hasError("required")) {
        this._notificationService.show("El correo es obligatorio.");
      }
      if(email_user.hasError("email")) {
        this._notificationService.show("El correo es inválido.");
      }
      if(email_user.hasError("maxLength")) {
        this._notificationService.show("El correo debe tener 200 carácteres como máximo.");
      }

      return;
    }

    let data = [];
    if(this.user.firstName===first_name.value){
      data[0] = null;
    }else{
      data[0] = first_name.value;
    }
    if(this.user.lastName===last_name.value){
      data[1] = null;
    }else{
      data[1] = last_name.value;
    }
    if(this.user.emailAddress===email_user.value){
      data[2] = null;
    }else{
      data[2] = email_user.value;
    }
    
    let phoneNumbersAddedPromise: Promise<AppResponse> | null = null;
    let phoneNumbersDeletedPromise: Promise<AppResponse> | null = null;

    const updateUserPromise = firstValueFrom(this._userService.updateUser(data, userName!));

    if(this.phoneNumbersAdded.length > 0) {
      phoneNumbersAddedPromise = firstValueFrom(this._userService.addPhoneNumbers(this.user.userName, this.phoneNumbersAdded));
    }

    if(this.phoneNumbersDeleted.length > 0) {
      phoneNumbersDeletedPromise = firstValueFrom(this._userService.deletePhoneNumbers(this.user.userName, this.phoneNumbersDeleted));
    }

    try{
      let aggregatePromise = Promise.all([updateUserPromise, phoneNumbersAddedPromise, phoneNumbersDeletedPromise]);

      let responses = await aggregatePromise;

      let failingResponse: AppResponse;
      let allOk = true;

      for(let response of responses){
        if(response && !response.ok) {
          allOk = false;
          failingResponse = response;
          console.log(response);
        }
      }

      if(allOk) {
        Swal.fire({
          icon: "success",
          title: "El usuario ha sido modificado con éxito."
        });
      }
      else {
        for (const message of AppResponse.getErrors(failingResponse!)) {
          this._notificationService.show(message);
        }
      }
    }
    catch (error) {
      Swal.fire({
        icon: "error",
        title: "Ha ocurrido un error"
      });
    }
    this.phoneNumbersAdded = [];
    this.phoneNumbersDeleted = [];
  }

  private buildFormGroup() {
    return new FormGroup({
      first_name: new FormControl(this.user?.firstName, { nonNullable: true, validators: [Validators.required, Validators.minLength(2), Validators.maxLength(50)] }),
      last_name: new FormControl(this.user?.lastName, { nonNullable: true, validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)] }),
      email_address: new FormControl(this.user?.emailAddress, { nonNullable: true, validators: [Validators.required, Validators.email, Validators.maxLength(200)] }),
      phone_number: new FormControl("")
    });
  }
}