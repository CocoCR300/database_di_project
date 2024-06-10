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
import { DialogImageData, ImagesUploadDialogComponent, ImagesUploadDialogData, ImagesUploadDialogResult } from '../images-upload-dialog/images-upload-dialog.component';
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
  inputDisabled = true;
  emptyTitle!: string;
  lodgingImageFiles!: File[] | null;
  lodgingImagesData: any[] = []; 
  roomTypeFormGroup: FormGroup = this.buildFormGroup();
  lodging!: Lodging | null;
  _filteredPerks: Perk[] = [];
  roomNumbers: number[] = [];
  roomTypes: RoomType[] = [];
  selectedRoomType: RoomType | null = null;
  selectedRoomTypePhotos: DialogImageData[] = [];
  newRoomTypeImages: File[] = [];
  roomTypeImagesToDelete: string[] = [];

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
    return this.selectedRoomTypePhotos.length > 0;
  }

  get lodgingName() {
    let lodgingName;
    
    if (this.roomTypeFormGroup?.get("name") !== null) {
      lodgingName = this.roomTypeFormGroup.get("name")!.value;
    } 
    
    if (lodgingName == "") {
      lodgingName = this.emptyTitle;
    }

    return lodgingName;
  }

  hasPhotos(roomType: RoomType): boolean {
    return roomType.photos.length > 0;
  }

  prepareImageForDisplay(image: DialogImageData) {
    let imageSrc = "";

    if (image.fileName) {
      imageSrc = this.prependImagesRoute(image.fileName);
    }
    else {
      imageSrc = image.data;
    }

    return imageSrc;
  }

  prependImagesRoute(imageFileName: string) {
    let imageSrc = "";
    if (imageFileName) {
      imageSrc = `${server.roomTypeImages}${imageFileName}`;
    }

    return imageSrc;
  }

  createRoomType() {
    this.inputDisabled = false;
    this.create = true;
  }

  addRoom(event: any) {
    if (!this.roomNumbers.includes(event.value)) {
      this.roomNumbers.push(event.value);
    }

    event.chipInput!.clear();
    this.roomTypeFormGroup.get("roomNumber")!.setValue(null);
  }

  cancelEdit() {
    this.resetForm();
  }
  
  editRoomType(roomType: RoomType) {
    this.inputDisabled = false;
    this.create = false;
    this.selectedRoomType = roomType;
    this.roomTypeFormGroup = this.buildFormGroup();
  }

  async saveRoomType() {
    const roomTypeName = this.roomTypeFormGroup.get<string>("roomTypeName")!;
    const perNightPrice = this.roomTypeFormGroup.get<string>("perNightPrice")!;
    const capacity = this.roomTypeFormGroup.get<string>("roomTypeName")!;
    const fees = this.roomTypeFormGroup.get<string>("fees")!;
    const roomNumbers = this.roomNumbers;

    if (this.roomTypeFormGroup.invalid) {
      if (roomTypeName.hasError("required")) {
        this._notificationService.show("El nombre de la habitación es obligatorio.");
      }
      if (perNightPrice.hasError("required")) {
        this._notificationService.show("El precio por noche de la habitación es obligatorio.");
      }
      if (capacity.hasError("required")) {
        this._notificationService.show("La capacidad de la habitación es obligatoria.");
      }
      if (fees.hasError("required")) {
        this._notificationService.show("El impuesto aplicado de la habitación es obligatorio.");
      }

      return;
    }

    const roomType = new RoomType(
      0,
      fees.value,
      perNightPrice.value,
      capacity.value,
      this.lodging!.id,
      roomTypeName.value.trim(),
      []
    );

    if (this.create && this.selectedRoomType != null) {
      roomType.id = this.selectedRoomType.id;
      roomType.photos = this.selectedRoomType.photos;
    }

    if (this.create) {
      const addRoomTypeResponse = await firstValueFrom(this._lodgingService.addRoomType(this.lodging!.id, roomType));

      if (addRoomTypeResponse.ok) {
        await Swal.fire({
          icon: "success",
          title: "La habitación ha sido creada con éxito"
        });
      }

      let allOk = true;
      if (this.newRoomTypeImages.length > 0) {
        const response = await firstValueFrom(this._lodgingService.addRoomTypePhotos(this.lodging!.id, roomType.id, this.newRoomTypeImages));

        if (!response.ok) {
          allOk = false;
          Swal.fire({
            icon: "error",
            title: "Ha ocurrido un error al subir las fotos de la habitación"
          });
        }
      }

      if (allOk) {
        this.roomTypes.push(roomType);
        this.resetForm();
      }
    }
    else {
      if (this.roomTypeImagesToDelete.length > 0) {
        const response = await firstValueFrom(this._lodgingService.deleteRoomTypePhotos(this.lodging!.id, roomType.id, this.roomTypeImagesToDelete));

        if (response.ok) {
          this.roomTypeImagesToDelete = [];
        }
        else {
          Swal.fire({
            icon: "error",
            title: "Ha ocurrido un error al eliminar las fotos de la habitación"
          });

          return;
        }
      }

      if (this.newRoomTypeImages.length > 0) {
        const response = await firstValueFrom(this._lodgingService.addRoomTypePhotos(this.lodging!.id, roomType.id, this.newRoomTypeImages));

        if (response.ok) {
          this.newRoomTypeImages = [];
        }
        else {
          Swal.fire({
            icon: "error",
            title: "Ha ocurrido un error al subir las fotos de la habitación"
          });

          return;
        }
      }

      const response = await firstValueFrom(this._lodgingService.updateRoomType(this.lodging!.id, roomType));

      if (response.ok) {
        await Swal.fire({
          icon: "success",
          title: "La habitación ha sido actualizada con éxito"
        });

        window.location.reload();
      }
      else {
        await Swal.fire({
          icon: "error",
          title: "Ha ocurrido un error al actualizar la habitación"
        });
      }
    }
  }

  removeRoom(roomNumber: number) {
    const index = this.roomNumbers.indexOf(roomNumber);
    this.roomNumbers.splice(index, 1);
  }

  openImagesDialog() {
    const images: string[] = this.selectedRoomType?.photos?.slice() ?? [];

    this._dialog.open(ImagesUploadDialogComponent,
      { data: new ImagesUploadDialogData(false, "Fotos de la habitación", server.roomTypeImages, images) })
      .afterClosed().subscribe((dialogResult: ImagesUploadDialogResult) => {
          if (dialogResult.confirmed) {
            this.selectedRoomTypePhotos = dialogResult.updatedImages;
            for (const image of dialogResult.imagesToDelete) {
              this.roomTypeImagesToDelete.push(image);
            }

            this.newRoomTypeImages = [];
            for (const imageFile of dialogResult.newImages) {
              const imageFileName = imageFile.name;
              if (dialogResult.updatedImages.find(image => image.fileName === imageFileName)) {
                this.newRoomTypeImages.push(imageFile);
              }
            }
          }
        }
      );
  }

  resetForm() {
    this.inputDisabled = true;
    this.selectedRoomType = null;
    this.selectedRoomTypePhotos = [];
    this.roomTypeFormGroup = this.buildFormGroup();
    
    this.roomTypeImagesToDelete = [];
    this.newRoomTypeImages = [];
    if (this.create) {
    }
    else {
    }
  }

  private buildFormGroup() {
    const disabled = this.inputDisabled;
    this.roomTypeFormGroup = new FormGroup({
      roomTypeName: new FormControl({ disabled: disabled, value: this.selectedRoomType?.name }, { nonNullable: true, validators: Validators.required }),
      capacity: new FormControl({ disabled: disabled, value: this.selectedRoomType?.capacity }, { nonNullable: true, validators: Validators.required }),
      perNightPrice: new FormControl({ disabled: disabled, value: this.selectedRoomType?.perNightPrice }, { nonNullable: true, validators: Validators.required }),
      fees: new FormControl({ disabled: disabled , value: this.selectedRoomType?.fees }, { nonNullable: true, validators: Validators.required }),
      roomNumber: new FormControl({ disabled: disabled, value: "" }, { nonNullable: true, validators: Validators.required }),
    });

    if (this.lodging && !disabled) {
      if (this.lodging.rooms) {
        this.roomNumbers = this.lodging!.rooms!.filter(room => room.typeId == this.selectedRoomType!.id).map(room => room.roomNumber);
      }

      this.selectedRoomTypePhotos = this.selectedRoomType!.photos.map(photo => new DialogImageData(photo, null));
    }

    return this.roomTypeFormGroup;
  }

  async ngOnInit() {
    const lodgingIdString = this._route.snapshot.paramMap.get("id");

    if (lodgingIdString !== null) {
      const lodgingId = parseInt(lodgingIdString);
      this.lodging = await firstValueFrom(this._lodgingService.getLodging(lodgingId));
      this.roomTypes = await firstValueFrom(this._lodgingService.getLodgingRoomTypes(lodgingId));
      this.roomNumbers = (await firstValueFrom(this._lodgingService.getLodgingRooms(lodgingId))).map(room => room.roomNumber);
    }

    this.roomTypeFormGroup = this.buildFormGroup();
  }
}
