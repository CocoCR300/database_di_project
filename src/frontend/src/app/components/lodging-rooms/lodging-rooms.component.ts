import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { Component, OnInit } from '@angular/core';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
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
import { AppResponse } from '../../models/app_response';
import Swal from 'sweetalert2';
import { MatButtonModule } from '@angular/material/button';
import { server } from '../../services/global';
import { UserService } from '../../services/user.service';
import { AppState } from '../../models/app_state';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { Perk } from '../../models/perk';
import { CarouselModule } from '@coreui/angular';
import { MatDialog } from '@angular/material/dialog';
import { ImagesUploadDialogComponent, ImagesUploadDialogData, ImagesUploadDialogResult } from '../images-upload-dialog/images-upload-dialog.component';
import { AppImageData } from '../../models/app_image_data';
import { RoomType } from '../../models/room_type';
import { Room } from '../../models/room';

@Component({
  selector: 'app-lodging-rooms',
  standalone: true,
  imports: [AsyncPipe, CarouselModule, FormsModule, MatAutocompleteModule, MatButtonModule,
    MatChipsModule, MatIconModule, MatInputModule, MatListModule, MatOptionModule, MatSelectModule, NgFor,
    NgIf, ReactiveFormsModule, RouterLink],
  templateUrl: './lodging-rooms.component.html',
  styleUrl: './lodging-rooms.component.css'
})
export class LodgingRoomsComponent implements OnInit
{
  separatorKeysCodes: number[] = [ENTER, COMMA];
  create = true;
  emptyTitle!: string;
  lodgingImageFiles!: File[] | null;
  lodgingImagesData: any[] = []; 
  lodgingFormGroup: FormGroup = this.buildFormGroup();
  lodging!: Lodging | null;
  _filteredPerks: Perk[] = [];
  rooms: Room[] = [];
  roomTypes: RoomType[] = [];
  selectedRoomType!: RoomType;

  public constructor(
    private _appState: AppState,
    private _dialog: MatDialog,
    private _route: ActivatedRoute,
    private _lodgingService: LodgingService,
    private _notificationService: NotificationService,
    private _router: Router,
    private _userService: UserService
  ) { }

  get selectedRoomTypeHasPhotos() {
    return this.selectedRoomType && this.selectedRoomType.photos.length > 0;
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

  hasPhotos(roomType: RoomType): boolean {
    return roomType.photos.length > 0;
  }

  prependImagesRoute(imageFileName: string) {
    let imageSrc = "";
    if (imageFileName) {
      imageSrc = `${server.roomTypeImages}${imageFileName}`;
    }

    return imageSrc;
  }

  addRoom(event: any) {

  }

  editRoomType(roomType: RoomType) {
    this.selectedRoomType = roomType;
  }

  removeRoom(room: Room) {

  }

  openImagesDialog() {
    const images: string[] = this.selectedRoomType?.photos?.slice() ?? [];

    this._dialog.open(ImagesUploadDialogComponent,
      { data: new ImagesUploadDialogData(false, "Fotos de la habitación", images) })
      .afterClosed().subscribe(
        async (dialogResult: ImagesUploadDialogResult) => {
          if (dialogResult.confirmed) {
            if (dialogResult.imagesToDelete.length > 0) {
              const deletedImagesResponse = await firstValueFrom(this._lodgingService.deleteLodgingImages(this.lodging!.id, dialogResult.imagesToDelete));

              if (!deletedImagesResponse.ok) {
                Swal.fire({
                  icon: "error",
                  title: "Ha ocurrido un error al eliminar las imagenes",
                });

                return;
              }
            }
            
            let newImagesFileNames: string[];
            if (dialogResult.newImages.length > 0) {
              const savedImagesResponse = await firstValueFrom(this._lodgingService.saveLodgingImages(this.lodging!.id, dialogResult.newImages));

              if (savedImagesResponse.ok) {
                newImagesFileNames = savedImagesResponse.body;
              }
              else {
                Swal.fire({
                  icon: "error",
                  title: "Ha ocurrido un error al subir las imagenes",
                });

                return;
              }
            }

            const updatedImagesData: AppImageData[] = [];
            let updatedImagesOrder = 0;
            let newImagesIndex = 0;
            for (let image of dialogResult.updatedImages) {
              let fileName = image.fileName;

              if (!fileName) {
                fileName = newImagesFileNames![newImagesIndex]; 
                newImagesIndex += 1;
              }

              updatedImagesData.push(new AppImageData(fileName, updatedImagesOrder));
              updatedImagesOrder += 1;
            }

            const updatedImagesResponse = await firstValueFrom(this._lodgingService.modifyLodgingImages(this.lodging!.id, updatedImagesData));

            if (updatedImagesResponse.ok) {
              await Swal.fire({
                icon: "success",
                title: "Las fotos han sido modificadas con éxito"
              });

              window.location.reload();
            }
            else {
              Swal.fire({
                icon: "error",
                title: "Ha ocurrido un error al modificar las fotos"
              });
            }
          }
        }
      );
  }

  resetForm() {
    this.lodgingFormGroup.reset();
    
    if (this.create) {
    }
    else {
    }
  }

  private buildFormGroup() {
    const formGroup = new FormGroup({
      name: new FormControl(this.lodging?.name, { nonNullable: true, validators: Validators.required }),
      description: new FormControl(this.lodging?.description, { nonNullable: true, validators: Validators.required }),
      address: new FormControl(this.lodging?.address, { nonNullable: true, validators: Validators.required }),
      emailAddress: new FormControl(this.lodging?.emailAddress, { nonNullable: true, validators: [Validators.required, Validators.email] }),
      lodgingType: new FormControl(this.lodging?.type, { nonNullable: true, validators: Validators.required }),
      phoneNumber: new FormControl(""),
      perkName: new FormControl(""),
    });

    return formGroup;
  }

  async ngOnInit() {
    const lodgingIdString = this._route.snapshot.paramMap.get("id");

    if (lodgingIdString !== null) {
      const lodgingId = parseInt(lodgingIdString);
      this.lodging = await firstValueFrom(this._lodgingService.getLodging(lodgingId));
      this.roomTypes = await firstValueFrom(this._lodgingService.getLodgingRoomTypes(lodgingId));
      this.rooms = await firstValueFrom(this._lodgingService.getLodgingRooms(lodgingId));

      if (this.roomTypes.length > 0) {
        this.selectedRoomType = this.roomTypes[0];
      }
    }

    this.lodgingFormGroup = this.buildFormGroup();
  }
}
