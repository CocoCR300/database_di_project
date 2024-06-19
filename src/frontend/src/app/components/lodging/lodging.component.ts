import moment from 'moment';
import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { LodgingService } from '../../services/lodging.service';
import { Dialogs } from '../../util/dialogs';
import { Observable, firstValueFrom, map, of, startWith } from 'rxjs';
import { Lodging } from '../../models/lodging';
import { AsyncPipe, CurrencyPipe, NgFor, NgIf } from '@angular/common';
import { AppResponse } from '../../models/app_response';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSidenavModule, MatDrawer } from '@angular/material/sidenav';
import { provideMomentDateAdapter } from '@angular/material-moment-adapter';
import Swal from 'sweetalert2';
import { AppState } from '../../models/app_state';
import { UserRoleEnum } from '../../models/user';
import { UserService } from '../../services/user.service';
import { FormControl, FormGroup, FormsModule, NgForm, ReactiveFormsModule, Validators } from '@angular/forms';
import { BookingService } from '../../services/booking.service';
import { Router, RouterLink } from '@angular/router';
import { NotificationService } from '../../services/notification.service';
import { MatButtonModule } from '@angular/material/button';
import { server } from '../../services/global';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { BookingRequestData } from '../../models/booking-request-data';
import { RoomRequest } from '../../models/room-request';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { Perk } from '../../models/perk';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { COMMA, ENTER } from '@angular/cdk/keycodes';

@Component({
    selector: 'app-lodging',
    standalone: true,
    imports: [MatAutocompleteModule, MatChipsModule, MatIconModule, MatTableModule, MatSelectModule, AsyncPipe, CurrencyPipe, FormsModule, NgFor, NgIf, MatButtonModule, MatDatepickerModule, MatFormFieldModule, MatInputModule, MatPaginatorModule, MatSidenavModule, ReactiveFormsModule, RouterLink],
    providers: [provideMomentDateAdapter()],
    templateUrl: './lodging.component.html',
    styleUrls: ['./lodging.component.scss']
})
export class LodgingComponent implements OnInit {
    separatorKeysCodes: number[] = [ENTER, COMMA];

    @ViewChild('bookingForm')
    bookingForm!: NgForm;
    @ViewChild('paginator')
    paginator!: MatPaginator;
    @ViewChild(MatDrawer)
    sidebar!: MatDrawer;

    bookingFormGroup!: FormGroup;
    perkFormControl = new FormControl<string>("");
    canBook = false;
    canDelete = false;
    isLessor = false;
    isUserLogged!: boolean;
    _filteredLodgings: Lodging[] | null = null;
    pagedLodgings: Lodging[] = [];
    currentPage = 0;
    pageSize = 10;
    lodgingsDataSource!: MatTableDataSource<Lodging>;
    selectedLodging!: Lodging | null;
    title: string = "Alojamientos";
    searchTerm = "";
    perks: Perk[] = [];
    _filteredPerks: Perk[] = [];
    filteredPerks: Observable<Perk[]> = of<Perk[]>([]);
    selectedPerks: Perk[] = [];
    searchTermCurrentTimeout!: any;
    selectedRoomTypePrice: number = 0;
    selectedTotalPrice: number = 0;
    public temporaryBookings: BookingRequestData[] = [];
    public dataSource: MatTableDataSource<BookingRequestData>;
    displayedColumns: string[] = ['roomTypeId', 'startDate', 'endDate', 'discount', 'actions'];
    lodgings: Lodging[] = [];

    public constructor(
        private _appState: AppState,
        private _bookingService: BookingService,
        private _lodgingService: LodgingService,
        private _notificationService: NotificationService,
        private _userService: UserService,
        private router: Router,
        private cdr: ChangeDetectorRef
    ) {
        this.dataSource = new MatTableDataSource(this.temporaryBookings);
    }

    hasPhotos(lodging: Lodging) {
        return lodging.photos!.length > 0;
    }

    hasRoomTypes(lodging: Lodging) {
        return Lodging.offersRooms(lodging) && lodging.roomTypes!.length > 0;
    }

    prependImagesRoute(lodging: Lodging) {
        let imageSrc = "";
        if (lodging.photos != null) {
            imageSrc = `${server.lodgingImages}${lodging.photos[0]}`;
        }

        return imageSrc;
    }

    public isLodgingWithMultipleRoomTypes(): boolean {
        return [1, 2, 3, 4].includes(this.selectedLodging?.type);
    }

    public isCompleteLodging(): boolean {
        return this.selectedLodging?.type === 0 || this.selectedLodging?.type === 5;
    }

    public getRoomNames(rooms: any[]): string {
        if (rooms && rooms.length > 0) {
            return rooms.map(room => room.type.name).join(', ');
        }
        return '';
    }

    public async createLodging() {
        this.router.navigate(["lodging/create"]);
    }

    public async editLodging(lodgingId: number) {
        this.router.navigate(["lodging/edit", lodgingId]);
    }

    addPerk(event: MatChipInputEvent) {
        const value = (event.value || '').trim();

        if (value) {
            const perk = this._filteredPerks.find(perk => perk.name === value);

            if (perk && !this.selectedPerks.find(perk0 => perk0.id === perk.id)) {
                this.selectedPerks.push(perk);
            }
        }

        event.chipInput!.clear();
        this.perkFormControl.setValue(null);
        this.filterLodgings(this);
    }

    perkAutoCompleteSelected(event: MatAutocompleteSelectedEvent) {
        const perk = event.option.value as Perk;

        if (!this.selectedPerks.find(perk0 => perk0.id === perk.id)) {
            this.selectedPerks.push(perk);
        }

        this.perkFormControl.setValue(null);
        this.filterLodgings(this);
    }

    removePerk(perk: Perk) {
        const index = this.selectedPerks.findIndex(perk0 => perk0.id === perk.id);

        if (index >= 0) {
            this.selectedPerks.splice(index, 1);
            this.filterLodgings(this);
        }
    }

    public async addTemporaryBooking() {
        if (this.bookingFormGroup.valid && this.selectedLodging) {
            const startDate = this.bookingFormGroup.get('startDate')?.value;
            const endDate = this.bookingFormGroup.get('endDate')?.value;
            const roomTypeId = this.bookingFormGroup.get('roomTypeId')?.value;
            const discount = this.bookingFormGroup.get('discount')?.value || 0;

            const roomRequest: RoomRequest = {
                roomTypeId,
                startDate: moment(startDate).format('YYYY-MM-DD'),
                endDate: moment(endDate).format('YYYY-MM-DD'),
                discount
            };

            const user = await firstValueFrom(this._userService.getUser(this._appState.userName!));

            const bookingRequestData: BookingRequestData = {
                customerId: user.personId!,
                lodgingId: this.selectedLodging!.id,
                rooms: [roomRequest]
            };

            this.temporaryBookings.push(bookingRequestData);
            this.dataSource.data = [...this.temporaryBookings];
            this.cdr.detectChanges();
            this.bookingFormGroup.get('roomTypeId')!.reset();
            this.bookingFormGroup.get('roomTypeId')!.setErrors(null);

            this._notificationService.show("La reserva ha sido guardada temporalmente.");
        }
        else {
            Swal.fire({
                icon: "error",
                title: "Formulario inválido",
                text: "Por favor, complete todos los campos requeridos."
            });
        }
    }

    public async submitBooking() {
        this.updateValidators();
    
        if (this.bookingFormGroup.valid && this.selectedLodging) {
            const startDate = this.bookingFormGroup.get('startDate')?.value;
            const endDate = this.bookingFormGroup.get('endDate')?.value;
            const discount = this.bookingFormGroup.get('discount')?.value || 0;
    
            const user = await firstValueFrom(this._userService.getUser(this._appState.userName!));
    
            const bookingRequestData: BookingRequestData = {
                customerId: user.personId!,
                lodgingId: this.selectedLodging!.id,
                rooms: [{
                    roomTypeId: this.isCompleteLodging() ? 0 : this.bookingFormGroup.get('roomTypeId')?.value,
                    startDate: moment(startDate).format('YYYY-MM-DD'),
                    endDate: moment(endDate).format('YYYY-MM-DD'),
                    discount
                }]
            };
    
            const response = await firstValueFrom(this._bookingService.postBooking(bookingRequestData));
    
            if (response.ok) {
                Swal.fire({
                    icon: "success",
                    title: "La reserva ha sido creada con éxito."
                });
                this.sidebar.close();
            } else {
                for (const message of AppResponse.getErrors(response)) {
                    this._notificationService.show(message);
                }
            }
        } else {
            Swal.fire({
                icon: "error",
                title: "Formulario inválido",
                text: "Por favor, complete todos los campos requeridos."
            });
        }
    }

    public async submitAllBookings() {
        try {
            const responses = await Promise.all(
                this.temporaryBookings.map(bookingRequestData => firstValueFrom(this._bookingService.postBooking(bookingRequestData)))
            );

            const failedResponses = responses.filter(response => !response.ok);

            if (failedResponses.length > 0) {
                Swal.fire({
                    icon: "error",
                    title: "Error al realizar las reservas",
                    text: "Algunas reservas no se pudieron realizar correctamente."
                });
                failedResponses.forEach(response => {
                    if (response) {
                        for (const message of AppResponse.getErrors(response)) {
                            this._notificationService.show(message);
                        }
                    }
                });
            } else {
                Swal.fire({
                    icon: "success",
                    title: "Todas las reservas se han realizado con éxito."
                });
                this.temporaryBookings = []; // Limpiar la lista de reservas temporales
                this.dataSource.data = [...this.temporaryBookings]; // Actualiza la referencia de datos con una nueva copia
                this.cdr.detectChanges(); // Fuerza la detección de cambios
            }
        } catch (error) {
            Swal.fire({
                icon: "error",
                title: "Ha ocurrido un error",
                text: "No se pudieron realizar las reservas."
            });
        }
    }

    public deleteRoomBooking(rowIndex: number) {
        this.temporaryBookings.splice(rowIndex, 1);
        this.dataSource.data = [...this.temporaryBookings]; // Actualiza la referencia de datos con una nueva copia
        this.cdr.detectChanges(); // Fuerza la detección de cambios
    }

    public openBookingDrawer(lodging: Lodging) {
        if (this.isUserLogged) {
            this._lodgingService.getLodging(lodging.id).subscribe(data => {
                this.selectedLodging = data;
                this.sidebar.open();
            });
        } else {
            this.router.navigate(["login"]);
        }
    }

    public bookingDrawerClosed() {
        this.bookingForm.resetForm();
        this.temporaryBookings = [];
        this.dataSource.data = [...this.temporaryBookings];
        this.cdr.detectChanges();
    }

    public pageChanged(event: any) {
        this.pageSize = event.pageSize;
        this.updatePagedList(event.pageIndex);
    }

    public searchTermChanged(event: any) {
        this.searchTerm = event.target.value;
        if (this.searchTermCurrentTimeout != null) {
            clearTimeout(this.searchTermCurrentTimeout);
        }

        this.searchTermCurrentTimeout = setTimeout(this.filterLodgings, 500, this);
    }

    public filterLodgings(component: LodgingComponent) {
        this.searchTermCurrentTimeout = null;
        if (component.searchTerm != "" || component.selectedPerks.length > 0) {
            const searchTermUppercase = component.searchTerm.toLocaleUpperCase();
            component._filteredLodgings = component.lodgings.filter(lodging => {
                let foundPerks = !lodging.perks;
                if (lodging.perks) {
                    foundPerks = lodging.perks.length > 0 && lodging.perks.every(perk => component.selectedPerks.find(perk0 => perk0.id === perk.id));
                }

                if (component.searchTerm != "") {
                    return lodging.name.toLocaleUpperCase().includes(searchTermUppercase)
                            || lodging.description.toLocaleUpperCase().includes(searchTermUppercase)
                            || lodging.address.toLocaleUpperCase().includes(searchTermUppercase)
                            || foundPerks;
                }
                else {
                    return foundPerks;
                }
            });
        } else {
            component._filteredLodgings = null;
        }

        component.updatePagedList(0);
    }

    public async deleteLodging(lodgingId: number): Promise<void> {
        const deleteLodging = await Dialogs.showConfirmDialog(
            "¿Está seguro de que desea eliminar este alojamiento?",
            "Está acción no se puede revertir."
        );

        if (!deleteLodging) {
            return;
        }

        const lodgingBookings = await firstValueFrom(this._lodgingService.getLodgingBookings(lodgingId));

        if (lodgingBookings.length > 0) {
            const proceed = await Dialogs.showConfirmDialog(
                "Acción requerida",
                "Este alojamiento aún tiene reservas pendientes. Si continua, las reservas serán borradas ¿Desea continuar?");
            if (proceed) {
                const response = await firstValueFrom(this._lodgingService.deleteBookings(lodgingId, lodgingBookings.map(booking => booking.id)));
                if (!response.ok) {
                    Swal.fire({
                        icon: "error",
                        title: "Ha ocurrido un error al eliminar las reservas",
                        text: response.body.message
                    });
                    return;
                }
            }
            else {
                return;
            }
        }

        this._lodgingService.deleteLodging(lodgingId).subscribe(
            (response: AppResponse) => {
                if (response.ok) {
                    const index = this.lodgings.findIndex(lodging => lodging.id == lodgingId);

                    if (index > 0) {
                        this.lodgings.splice(index, 1);
                        this.updatePagedList(this.currentPage);

                        Swal.fire({
                            icon: "info",
                            title: "Alojamiento eliminado con éxito.",
                        });
                    }
                }
                else {
                    Swal.fire({
                        icon: "error",
                        "title": "Ha ocurrido un error",
                        "text": response.body.message
                    });
                }
            });
    }

    private updatePagedList(pageIndex: number) {
        let startIndex = pageIndex * this.pageSize;
        let endIndex = startIndex + this.pageSize;
        if(endIndex > this.lodgings.length) {
            endIndex = this.lodgings.length;
        }

        let lodgings = this.lodgings;
        if (this._filteredLodgings != null) {
            lodgings = this._filteredLodgings;
        }

        this.pagedLodgings = lodgings.slice(startIndex, endIndex);
    }

    private filterPerks(value: Perk | string): Perk[] {
        if (!(value instanceof String)) { // Don't know why this happens
        return [];
        }

        const filterValue = value.toLowerCase();

        const filteredPerks: Perk[] = this.perks.filter(perk => perk.name.toLowerCase().includes(filterValue));
        return filteredPerks;
    }

    ngOnInit(): void {
        this._lodgingService.getPerks().subscribe(perks => this.perks = perks);

        this.filteredPerks = this.perkFormControl.valueChanges.pipe(
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

        this.bookingFormGroup = new FormGroup({
            startDate: new FormControl<Date | null>(null, Validators.required),
            endDate: new FormControl<Date | null>(null, Validators.required),
            roomTypeId: new FormControl<number | null>(null),
            discount: new FormControl<number | null>(null)
        });
        
        this.bookingFormGroup.get('roomTypeId')?.valueChanges.subscribe(() => {
            this.updatePrices();
        });
    
        this.bookingFormGroup.get('startDate')?.valueChanges.subscribe(() => {
            this.updatePrices();
        });
        this.bookingFormGroup.get('endDate')?.valueChanges.subscribe(() => {
            this.updatePrices();
        });
    
        this.isUserLogged = this._appState.isUserLogged;
        if (this.isUserLogged) {
            this.isLessor = this._appState.role === UserRoleEnum.Lessor;
            this.canDelete = this._appState.role === UserRoleEnum.Administrator || this.isLessor;
        }

        this.canBook = !this.isUserLogged || this._appState.role === UserRoleEnum.Customer;
    
        let lodgingsObservable: Observable<Lodging[]>;
        if (this.isLessor) {
            this.title = "Mis alojamientos";
            lodgingsObservable = this._lodgingService.getLessorLodgings(this._appState.userName!);
        } else {
            lodgingsObservable = this._lodgingService.getLodgings(10000, 1);
        }

        lodgingsObservable.subscribe(lodgings => {
            this.lodgings = lodgings;

            if (this._appState.role == UserRoleEnum.Customer) {
                this.lodgings = this.lodgings
                    .filter(lodging => !Lodging.offersRooms(lodging) || (lodging.roomTypes != null && lodging.roomTypes.length > 0));
            }

            this.lodgings.forEach(lodging => {
                if (lodging.roomTypes) {
                    let min = Infinity, max = 0;

                    for (const roomType of lodging.roomTypes) {
                        if (min > roomType.perNightPrice) {
                            min = roomType.perNightPrice;
                        }

                        if (max < roomType.perNightPrice) {
                            max = roomType.perNightPrice;
                        }
                    }

                    lodging.roomTypeMaxPrice = max;
                    lodging.roomTypeMinPrice = min;
                }
            });
            this.updatePagedList(0);
        });
    
        this.updateValidators();
    }
    
    private updateValidators() {
        if (this.isCompleteLodging()) {
            this.bookingFormGroup.get('roomTypeId')?.clearValidators();
        } else {
            this.bookingFormGroup.get('roomTypeId')?.setValidators(Validators.required);
        }
        this.bookingFormGroup.get('roomTypeId')?.updateValueAndValidity();
    }
    

    private updatePrices(): void {
        const roomTypeId = this.bookingFormGroup.get('roomTypeId')?.value;
        if (this.selectedLodging?.roomTypes && roomTypeId) {
            const selectedRoomType = this.selectedLodging.roomTypes.find(roomType => roomType.id === roomTypeId);
            this.selectedRoomTypePrice = selectedRoomType ? selectedRoomType.perNightPrice : 0;

            const startDateValue = this.bookingFormGroup.get('startDate')?.value;
            const endDateValue = this.bookingFormGroup.get('endDate')?.value;
            if (startDateValue && endDateValue) {
                const startDate = moment(startDateValue);
                const endDate = moment(endDateValue);
                const days = endDate.diff(startDate, 'days');
                this.selectedTotalPrice = days > 0 ? days * this.selectedRoomTypePrice : 0;
            } else {
                this.selectedTotalPrice = 0;
            }
        } else if (this.selectedLodging?.perNightPrice) {
            this.selectedRoomTypePrice = this.selectedLodging.perNightPrice;
            const startDateValue = this.bookingFormGroup.get('startDate')?.value;
            const endDateValue = this.bookingFormGroup.get('endDate')?.value;
            if (startDateValue && endDateValue) {
                const startDate = moment(startDateValue);
                const endDate = moment(endDateValue);
                const days = endDate.diff(startDate, 'days');
                this.selectedTotalPrice = days > 0 ? days * this.selectedRoomTypePrice : 0;
            } else {
                this.selectedTotalPrice = 0;
            }
        } else {
            this.selectedRoomTypePrice = 0;
            this.selectedTotalPrice = 0;
        }
    }
}
