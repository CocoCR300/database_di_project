import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { Component, OnInit } from '@angular/core';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { Lodging } from '../../models/lodging';
import { LodgingService } from '../../services/lodging.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Observable, firstValueFrom, map, of, startWith } from 'rxjs';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { NotificationService } from '../../services/notification.service';
import { AppResponse } from '../../models/app_response';
import Swal from 'sweetalert2';
import { MatButtonModule } from '@angular/material/button';
import { server } from '../../services/global';
import { UserService } from '../../services/user.service';
import { AppState } from '../../models/app_state';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { Perk } from '../../models/perk';

@Component({
  selector: 'app-lodging-info',
  standalone: true,
  imports: [AsyncPipe, FormsModule, MatAutocompleteModule, MatButtonModule, MatChipsModule, MatIconModule, MatInputModule, MatOptionModule, MatSelectModule, NgFor, NgIf, ReactiveFormsModule],
  templateUrl: './lodging-info.component.html',
  styleUrl: './lodging-info.component.css'
})
export class LodgingInfoComponent implements OnInit
{
  lodgingTypes = [
      { number: 0, name: "Apartmento" },
      { number: 1, name: "Casa de invitados" },
      { number: 2, name: "Hotel" },
      { number: 3, name: "Albergue" },
      { number: 4, name: "Casa de huéspedes" },
      { number: 5, name: "Alquiler vacacional" }
  ];

  separatorKeysCodes: number[] = [ENTER, COMMA];
  create = true;
  emptyTitle!: string;
  lodgingImageFile!: File | null;
  lodgingImageData: any; 
  lodgingFormGroup: FormGroup = this.buildFormGroup();
  lodging!: Lodging | null;
  phoneNumbers: string[] = [];
  phoneNumbersToAdd: string[] = [];
  phoneNumbersToDelete: string[] = [];
  perks: Perk[] = [];
  perksToAdd: Perk[] = [];
  perksToDelete: Perk[] = [];
  filteredPerks: Observable<Perk[]> = of<Perk[]>([]);
  selectedPerks: Perk[] = [];
  selectedPerkIds: number[] = [];
  _filteredPerks: Perk[] = [];

  public constructor(
    private _appState: AppState,
    private _route: ActivatedRoute,
    private _lodgingService: LodgingService,
    private _notificationService: NotificationService,
    private _router: Router,
    private _userService: UserService
  ) { }

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
    if (lodging != null && lodging.photos != null) {
      imageSrc = `${server.lodgingImages}${lodging.photos[0]}`;
    }

    return imageSrc;
  }

  onLodgingImageChanged(event: any) {
    this.lodgingImageFile = event.target.files[0];

    const reader = new FileReader();
    reader.onload = e => this.lodgingImageData = reader.result;

    reader.readAsDataURL(this.lodgingImageFile!);
  }

  addPerk(event: MatChipInputEvent) {
    const value = (event.value || '').trim();

    if (value) {
      const perk = this._filteredPerks.find(perk => perk.name === value);

      if (perk && !this.selectedPerkIds.includes(perk.id)) {
        this.selectedPerkIds.push(perk.id);
        this.selectedPerks.push(perk);
        this.perksToAdd.push(perk);
      }
    }

    event.chipInput!.clear();
    this.lodgingFormGroup.get("perkName")!.setValue(null);
  }

  perkAutoCompleteSelected(event: MatAutocompleteSelectedEvent) {
    const perk = event.option.value as Perk;

    if (!this.selectedPerkIds.includes(perk.id)) {
      this.selectedPerkIds.push(perk.id)
      this.selectedPerks.push(perk);
      this.perksToAdd.push(perk);
    }

    this.lodgingFormGroup.get("perkName")!.setValue(null);
  }

  removePerk(perk: Perk) {
    const index = this.selectedPerkIds.indexOf(perk.id);

    if (index >= 0) {
      this.selectedPerkIds.splice(index, 1);
      const perks = this.selectedPerks.splice(index, 1);
      this.perksToDelete.push(perks[0]);
      this.lodgingFormGroup.markAsDirty();
    }
  }

  addPhoneNumber(event: MatChipInputEvent) {
    const phoneNumber = event.value as string; 

    if (!this.phoneNumbers.includes(phoneNumber)) {
      this.phoneNumbers.push(phoneNumber);
      this.phoneNumbersToAdd.push(phoneNumber);
    }

    event.chipInput!.clear();
    this.lodgingFormGroup.get("phoneNumber")!.setValue(null);
  }

  removePhoneNumber(phoneNumber: string) {
    const index = this.phoneNumbers.indexOf(phoneNumber);
    
    if (index >= 0) {
      const phoneNumber = this.phoneNumbers.splice(index, 1);
      this.phoneNumbersToDelete.push(phoneNumber[0]);
      this.lodgingFormGroup.markAsDirty();
    }
  }

  undoImageChange() {
    this.lodgingImageFile = null;
    this.lodgingImageData = null;
  }

  async submitLodging() {
    const lodgingName = this.lodgingFormGroup.get<string>("name")!;
    const address = this.lodgingFormGroup.get<string>("address")!;
    const description = this.lodgingFormGroup.get<string>("description")!;
    const emailAddress = this.lodgingFormGroup.get<string>("emailAddress")!;
    const type = this.lodgingFormGroup.get<string>("lodgingType")!;

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
      if (emailAddress.hasError("required")) {
        this._notificationService.show("El correo electrónico del alojamiento es obligatoria.");
      }
      if (type.hasError("required")) {
        this._notificationService.show("El tipo del alojamiento es obligatorio.");
      }
      
      return;
    }

    const lodgingType = this.lodgingTypes.find(lodgingType => lodgingType.number === type.value);
    if (!lodgingType) {
      Swal.fire({
        icon: "error",
        title: "Ha ocurrido un error",
        text: "El tipo de alojamiento es inválido",
      })

      return;
    }

    const newLodging = new Lodging(
      0,
      0,
      lodgingType.number,
      lodgingName.value.trim(),
      description.value.trim(),
      address.value.trim(),
      type.value,
      emailAddress.value.trim(),
      null,
      null,
      null,
      null,
      null
    );

    if (!this.create && this.lodging) {
      newLodging.id = this.lodging.id;
      newLodging.ownerId = this.lodging.ownerId;
      newLodging.owner = this.lodging.owner;
      newLodging.photos = this.lodging.photos;
    }

    if (this.create) {
      const response = await firstValueFrom(this._userService.getUser(this._appState.userName!));
      newLodging.ownerId = response.personId;
      this._lodgingService.saveLodging(newLodging).subscribe(async response => {
        if (response.ok) {
          if (this.create) {
            await Swal.fire({
              icon: "success",
              title: "El alojamiento ha sido creado con éxito."
            });

            this._router.navigate(["lodging", response.body!.id]);
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
    else {
      let perksAddedPromise: Promise<AppResponse> | null = null;
      let perksRemovedPromise: Promise<AppResponse> | null = null;
      let phoneNumbersAddedPromise: Promise<AppResponse> | null = null;
      let phoneNumbersRemovedPromise: Promise<AppResponse> | null = null;

      const updateLodgingPromise = firstValueFrom(this._lodgingService.updateLodging(newLodging));

      if (this.perksToAdd.length > 0) {
        perksAddedPromise = firstValueFrom(this._lodgingService.addPerks(newLodging.id, this.perksToAdd.map(perk => perk.id)));
      }
      if (this.perksToDelete.length > 0) {
        perksRemovedPromise = firstValueFrom(this._lodgingService.removePerks(newLodging.id, this.perksToDelete.map(perk => perk.id)));
      }

      if (this.phoneNumbersToAdd.length > 0) {
        phoneNumbersAddedPromise = firstValueFrom(this._lodgingService.addPhoneNumbers(newLodging.id, this.phoneNumbersToAdd));
      }
      if (this.phoneNumbersToDelete.length > 0) {
        phoneNumbersRemovedPromise = firstValueFrom(this._lodgingService.removePhoneNumbers(newLodging.id, this.phoneNumbersToDelete));
      }


      try {
        const aggregatePromise = Promise.all([updateLodgingPromise, perksAddedPromise, perksRemovedPromise, phoneNumbersAddedPromise, phoneNumbersRemovedPromise]);

        const responses = await aggregatePromise;
        let failingResponse: AppResponse;
        let allOk = true;

        for (const response of responses) {
          if (response && !response.ok) {
            allOk = false;
            failingResponse = response;
          }
        }

        if (allOk) {
          this.lodging = newLodging;
          this.lodgingFormGroup = this.buildFormGroup();

          Swal.fire({
            icon: "success",
            title: "El alojamiento ha sido modificado con éxito."
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
    }
  }

  submitLodgingImage() {
    if (this.lodging != null && this.lodgingImageFile != null) {
      this._lodgingService.saveLodgingImage(this.lodging.id, this.lodgingImageFile).subscribe(
        response => {
          if (response.ok) {
            this.undoImageChange();
            // TODO
            //this.lodging!.image = response.body;
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

  compareLodgingTypes(type0: number | null, type1: number | null): boolean {
    return type0 === type1;
  }

  resetForm() {
    this.lodgingFormGroup.reset();
    
    if (this.create) {
      this.selectedPerks = [];
      this.phoneNumbers = [];
    }
    else {
      this.selectedPerks = this.lodging!.perks!.slice();
      this.selectedPerkIds = this.lodging!.perks!.map(perk => perk.id);
      this.phoneNumbers = this.lodging!.phoneNumbers!.slice();
    }

    this.perksToAdd = [];
    this.perksToDelete = [];
    this.phoneNumbersToAdd = [];
    this.phoneNumbersToDelete = [];
  }

  private filterPerks(value: Perk | string): Perk[] {
    if (!(value instanceof String)) { // Don't know why this happens
      return [];
    }

    const filterValue = value.toLowerCase();

    const filteredPerks: Perk[] = this.perks.filter(perk => perk.name.toLowerCase().includes(filterValue));
    return filteredPerks;
  }

  private buildFormGroup() {
    const formGroup = new FormGroup({
      name: new FormControl(this.lodging?.name, { nonNullable: true, validators: Validators.required }),
      description: new FormControl(this.lodging?.description, { nonNullable: true, validators: Validators.required }),
      address: new FormControl(this.lodging?.address, { nonNullable: true, validators: Validators.required }),
      emailAddress: new FormControl(this.lodging?.emailAddress, { nonNullable: true, validators: Validators.required }),
      lodgingType: new FormControl(this.lodging?.type, { nonNullable: true, validators: Validators.required }),
      phoneNumber: new FormControl(""),
      perkName: new FormControl(""),
    });

    this.filteredPerks = formGroup.get("perkName")!.valueChanges.pipe(
      startWith(null),
      map((perkName: string | null) => {
        if (perkName) {
          this._filteredPerks = this.filterPerks(perkName);
        }
        else {
          this._filteredPerks = this.perks.slice();
        }

        return this._filteredPerks;
      }),
    );

    return formGroup;
  }

  async ngOnInit() {
    this.emptyTitle = "Nuevo alojamiento";

    this._lodgingService.getPerks().subscribe(perks => this.perks = perks);
    const lodgingIdString = this._route.snapshot.paramMap.get("id");
    if (lodgingIdString !== null) {
      this.create = false;
      this.emptyTitle = "Alojamiento";
      const lodgingId = parseInt(lodgingIdString);
      this.lodging = await firstValueFrom(this._lodgingService.getLodging(lodgingId));

      this.selectedPerks = this.lodging.perks!.slice();
      this.selectedPerkIds = this.lodging.perks!.map(perk => perk.id);
      this.phoneNumbers = this.lodging.phoneNumbers!.slice();
    }

    this.lodgingFormGroup = this.buildFormGroup();
  }
}
