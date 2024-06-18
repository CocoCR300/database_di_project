import { Component, OnInit, ViewChild } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { Lodging } from '../../models/lodging';
import { LodgingService } from '../../services/lodging.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { server } from '../../services/global';
import { AppState } from '../../models/app_state';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { Perk } from '../../models/perk';
import { CarouselModule } from '@coreui/angular';
import { MatDialog } from '@angular/material/dialog';
import { UserRoleEnum } from '../../models/user';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { BookingSidebarComponent } from "../booking-sidebar/booking-sidebar.component";

@Component({
    selector: 'app-lodging-view',
    standalone: true,
    templateUrl: './lodging-view.component.html',
    styleUrl: './lodging-view.component.css',
    imports: [AsyncPipe, CarouselModule, FormsModule, MatAutocompleteModule, MatButtonModule,
        MatChipsModule, MatSidenavModule, MatIconModule, MatInputModule, MatOptionModule, MatSelectModule, NgFor,
        NgIf, ReactiveFormsModule, RouterLink, BookingSidebarComponent]
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

  @ViewChild(BookingSidebarComponent)
  bookingSidebarComponent!: BookingSidebarComponent;
  @ViewChild(MatDrawer)
  sidebar!: MatDrawer;

  hasPhotos = false;
  isLessor = false;
  lodgingOffersRooms = false;
  lodgingImagesData: any[] = []; 
  lodging!: Lodging | null;

  public constructor(
    private _appState: AppState,
    private _dialog: MatDialog,
    private _route: ActivatedRoute,
    private _lodgingService: LodgingService,
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

  get perks() {
    return this.lodging?.perks ?? [];
  }

  get phoneNumbers() {
    return this.lodging?.phoneNumbers ?? [];
  }

  get lodgingOwnerName() {
    let lodgingOwnerName = "";
    if (this.lodging && this.lodging.owner) {
      lodgingOwnerName = `${this.lodging.owner.firstName} ${this.lodging.owner.lastName}`;
    }

    return lodgingOwnerName;
  }

  public bookingDrawerClosed() {
    this.bookingSidebarComponent.bookingDrawerClosed();
  }

  public openBookingSidebar(lodging: Lodging) {
    this.bookingSidebarComponent.selectedLodging = lodging;
    this.sidebar.open();
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
    }
  }
}
