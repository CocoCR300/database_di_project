import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { Component, OnInit, ViewChild } from '@angular/core';
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
      [0, "Apartmento"],
      [1, "Casa de invitados"],
      [2, "Hotel"],
      [3, "Albergue"],
      [4, "Casa de huéspedes"],
      [5, "Alquiler vacacional"]
  ];

  separatorKeysCodes: number[] = [ENTER, COMMA];
  create = true;
  emptyTitle!: string;
  lodgingImageFile!: File | null;
  lodgingImageData: any; 
  lodgingFormGroup: FormGroup = this.buildFormGroup();
  lodging!: Lodging | null;
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

    // Add our fruit
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

    //this.lodgingFormGroup.get("perkName")!.nativeElement.value = '';
    this.lodgingFormGroup.get("perkName")!.setValue(null);
  }

  removePerk(perk: Perk) {
    const index = this.selectedPerkIds.indexOf(perk.id);

    if (index >= 0) {
      this.selectedPerkIds.splice(index, 1);
      const perks = this.selectedPerks.splice(index, 1);
      this.perksToDelete.push(perks[0]);
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
    const phoneNumbers = this.lodgingFormGroup.get<string>("phoneNumbers")!;

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

    // TODO
    const newLodging = new Lodging(
      0,
      0,
      lodgingName.value.trim(),
      description.value.trim(),
      address.value.trim(),
      "",
      "",
      null,
      null,
      null,
      null,
      null,
      null
    );

    if (this.lodging) {
      newLodging.id = this.lodging.id;
      newLodging.ownerId = this.lodging.ownerId;
      newLodging.owner = this.lodging.owner;
    }

    let observable;
    if (this.create) {
      const response = await firstValueFrom(this._userService.getUser(this._appState.userName!));
      newLodging.ownerId = response.personId;
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

  resetForm() {
    this.lodgingFormGroup.reset();
    
    if (this.create) {
      this.selectedPerks = [];
    }
    else {
      this.selectedPerks = this.lodging!.perks!.slice();
    }

    this.perksToAdd = [];
    this.perksToDelete = [];
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
      phoneNumbers: new FormControl(this.lodging?.phoneNumbers, { nonNullable: true }),
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
    }

    this.lodgingFormGroup = this.buildFormGroup();
  }
}
