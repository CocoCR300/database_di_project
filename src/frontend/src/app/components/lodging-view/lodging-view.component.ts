import { Component, OnInit } from '@angular/core';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { Lodging } from '../../models/lodging';
import { LodgingService } from '../../services/lodging.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Observable, firstValueFrom, map, of, startWith } from 'rxjs';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { NotificationService } from '../../services/notification.service';
import { MatButtonModule } from '@angular/material/button';
import { server } from '../../services/global';
import { UserService } from '../../services/user.service';
import { AppState } from '../../models/app_state';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { Perk } from '../../models/perk';
import { CarouselModule } from '@coreui/angular';
import { MatDialog } from '@angular/material/dialog';
import { UserRole } from '../../models/user_role';
import { UserRoleEnum } from '../../models/user';

@Component({
  selector: 'app-lodging-view',
  standalone: true,
  imports: [AsyncPipe, CarouselModule, FormsModule, MatAutocompleteModule, MatButtonModule,
    MatChipsModule, MatIconModule, MatInputModule, MatOptionModule, MatSelectModule, NgFor,
    NgIf, ReactiveFormsModule, RouterLink],
  templateUrl: './lodging-view.component.html',
  styleUrl: './lodging-view.component.css'
})
export class LodgingViewComponent implements OnInit
{
  lodgingTypes = [
      { number: 0, name: "Apartmento" },
      { number: 1, name: "Casa de invitados" },
      { number: 2, name: "Hotel" },
      { number: 3, name: "Albergue" },
      { number: 4, name: "Casa de huéspedes" },
      { number: 5, name: "Alquiler vacacional" }
  ];

  hasPhotos = false;
  isLessor = false;
  lodgingOffersRooms = false;
  lodgingImagesData: any[] = []; 
  lodging!: Lodging | null;
  phoneNumbers: string[] = [];
  perks: Perk[] = [];

  public constructor(
    private _appState: AppState,
    private _dialog: MatDialog,
    private _route: ActivatedRoute,
    private _lodgingService: LodgingService,
    private _notificationService: NotificationService,
    private _router: Router,
    private _userService: UserService
  ) { }

  get emailAddress() {
    return this.lodging?.emailAddress ?? "";
  }

  get lodgingAddress() {
    return this.lodging?.address ?? "";
  }

  get lodgingDescription() {
    return this.lodging?.description ?? "";
  }

  get lodgingName() {
    return this.lodging?.name ?? "";
  }

  get lodgingOwnerName() {
    let lodgingOwnerName = "";
    if (this.lodging && this.lodging.owner) {
      lodgingOwnerName = `${this.lodging.owner.firstName} ${this.lodging.owner.lastName}`;
    }

    return lodgingOwnerName;
  }

  prependImagesRoute(imageFileName: string | null) {
    let imageSrc = "";
    if (imageFileName) {
      imageSrc = `${server.lodgingImages}${imageFileName}`;
    }

    return imageSrc;
  }

  async ngOnInit() {
    this.isLessor = this._appState.role == UserRoleEnum.Lessor;

    const lodgingIdString = this._route.snapshot.paramMap.get("id");
    if (lodgingIdString !== null) {
      const lodgingId = parseInt(lodgingIdString);
      this.lodging = await firstValueFrom(this._lodgingService.getLodging(lodgingId));

      this.lodgingOffersRooms = Lodging.offersRooms(this.lodging);
      this.hasPhotos = this.lodging.photos!.length > 0;
      this.perks = this.lodging.perks!.slice();
      this.phoneNumbers = this.lodging.phoneNumbers!.slice();
    }
  }
}
