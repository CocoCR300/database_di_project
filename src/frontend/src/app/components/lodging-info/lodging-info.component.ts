import { Component, OnInit, ViewChild } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { Lodging } from '../../models/lodging';
import { LodgingService } from '../../services/lodging.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AsyncPipe, NgIf } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { NotificationService } from '../../services/notification.service';
import { AppResponse } from '../../models/app_response';
import Swal from 'sweetalert2';
import { MatButtonModule } from '@angular/material/button';
import { server } from '../../services/global';
import { UserService } from '../../services/user.service';
import { AppState } from '../../models/app_state';

@Component({
  selector: 'app-lodging-info',
  standalone: true,
  imports: [AsyncPipe, FormsModule, MatButtonModule, MatInputModule, NgIf, ReactiveFormsModule],
  templateUrl: './lodging-info.component.html',
  styleUrl: './lodging-info.component.css'
})
export class LodgingInfoComponent implements OnInit
{
  create = true;
  emptyTitle!: string;
  lodgingImageFile!: File | null;
  lodgingImageData: any; 
  lodgingFormGroup: FormGroup = this.buildFormGroup();
  lodging!: Lodging | null;

  public constructor(
    private _appState: AppState,
    private _route: ActivatedRoute,
    private _lodgingService: LodgingService,
    private _notificationService: NotificationService,
    private _router: Router,
    private _userService: UserService
  )
  { }

  get newLodgingImageSubmitted() {
    return this.lodgingImageData != null;
  }

  get lodgingName() {
    let lodgingName;
    
    if (this.lodgingFormGroup?.get("name") !== null) {
      lodgingName = this.lodgingFormGroup.get("name")!.value;
    } 
    
    if (lodgingName == "") {
      lodgingName = this.emptyTitle;
    }

    return lodgingName;
  }

  prependImagesRoute(lodging: Lodging | null) {
    let imageSrc = "";
    if (lodging != null) {
      imageSrc = `${server.lodgingImages}${lodging?.image}`;
    }

    return imageSrc;
  }

  onLodgingImageChanged(event: any) {
    this.lodgingImageFile = event.target.files[0];

    const reader = new FileReader();
    reader.onload = e => this.lodgingImageData = reader.result;

    reader.readAsDataURL(this.lodgingImageFile!);
  }

  undoImageChange() {
    this.lodgingImageFile = null;
    this.lodgingImageData = null;
  }

  async submitLodging() {
    const lodgingName = this.lodgingFormGroup.get<string>("name")!;
    const address = this.lodgingFormGroup.get<string>("address")!;
    const description = this.lodgingFormGroup.get<string>("description")!;
    const availableRooms = this.lodgingFormGroup.get<string>("availableRooms")!;
    const perNightPrice = this.lodgingFormGroup.get<string>("perNightPrice")!;

    if (this.lodgingFormGroup.invalid) {
      if (lodgingName.hasError("required")) {
        this._notificationService.show("El nombre del alojamiento es obligatorio.");
      }
      if (description.hasError("required")) {
        this._notificationService.show("La descripción del alojamiento es obligatoria.");
      }
      if (address.hasError("required")) {
        this._notificationService.show("La dirección del alojamiento es obligatoria.");
      }
      if (availableRooms.hasError("required")) {
        this._notificationService.show("La cantidad de habitaciones disponibles del alojamiento es obligatoria.");
      }
      if (perNightPrice.hasError("required")) {
        this._notificationService.show("El precio por noche del alojamiento es obligatorio.");
      }
      
      return;
    }

    const newLodging = new Lodging(
      0,
      0,
      lodgingName.value.trim(),
      description.value.trim(),
      "",
      address.value.trim(),
      null,
      parseFloat(perNightPrice.value),
      parseInt(availableRooms.value)
    );

    if (this.lodging) {
      newLodging.lodging_id = this.lodging.lodging_id;
      newLodging.lessor_id = this.lodging.lessor_id;
      newLodging.image = this.lodging.image;
      newLodging.lessor = this.lodging.lessor;
    }

    let observable;
    if (this.create) {
      const response = await firstValueFrom(this._userService.getUser(this._appState.userName!));
      newLodging.lessor_id = response.person_id;
      observable = this._lodgingService.saveLodging(newLodging);
    }
    else {
      observable = this._lodgingService.updateLodging(newLodging);
    }

    observable.subscribe(async response => {
      if (response.ok) {
        if (this.create) {
          await Swal.fire({
            icon: "success",
            title: "El alojamiento ha sido creado con éxito."
          });

          this._router.navigate(["lodging", response.body!.lodging_id]);
        }
        else {
          this.lodging = newLodging;
          this.lodgingFormGroup = this.buildFormGroup();
          Swal.fire({
            icon: "success",
            title: "El alojamiento ha sido modificado con éxito."
          });
        }
      }
      else {
        for (const message of AppResponse.getErrors(response)) {
            this._notificationService.show(message);
        }
      }
    });
  }

  submitLodgingImage() {
    if (this.lodging != null && this.lodgingImageFile != null) {
      this._lodgingService.saveLodgingImage(this.lodging.lodging_id, this.lodgingImageFile).subscribe(
        response => {
          if (response.ok) {
            this.undoImageChange();
            this.lodging!.image = response.body;
            Swal.fire({
              icon: "success",
              title: "El cambio de imagen se ha realizado con éxito."
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

  resetForm() {
    this.lodgingFormGroup.reset();
  }

  private buildFormGroup() {
    return new FormGroup({
      name: new FormControl(this.lodging?.name, { nonNullable: true, validators: Validators.required }),
      description: new FormControl(this.lodging?.description, { nonNullable: true, validators: Validators.required }),
      address: new FormControl(this.lodging?.address, { nonNullable: true, validators: Validators.required }),
      availableRooms: new FormControl(this.lodging?.available_rooms, { nonNullable: true, validators: Validators.required }),
      perNightPrice: new FormControl(this.lodging?.per_night_price, { nonNullable: true, validators: Validators.required })
    });
  }

  async ngOnInit() {
    this.emptyTitle = "Nuevo alojamiento";

    const lodgingIdString = this._route.snapshot.paramMap.get("id");
    if (lodgingIdString !== null) {
      this.create = false;
      this.emptyTitle = "Alojamiento";
      const lodgingId = parseInt(lodgingIdString);
      this.lodging = await firstValueFrom(this._lodgingService.getLodging(lodgingId));
    }

    this.lodgingFormGroup = this.buildFormGroup();
  }
}
