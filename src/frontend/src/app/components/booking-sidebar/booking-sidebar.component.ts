import moment from 'moment';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { LodgingService } from '../../services/lodging.service';
import { firstValueFrom } from 'rxjs';
import { Lodging } from '../../models/lodging';
import { AsyncPipe, CurrencyPipe, NgFor, NgIf } from '@angular/common';
import { AppResponse } from '../../models/app_response';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
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
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { BookingRequestData } from '../../models/booking-request-data';
import { RoomRequest } from '../../models/room-request';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-booking-sidebar',
    standalone: true,
    imports: [MatIconModule, MatSelectModule, MatTableModule, AsyncPipe, CurrencyPipe, FormsModule, NgFor, NgIf, MatButtonModule, MatDatepickerModule, MatFormFieldModule, MatInputModule, MatSidenavModule, ReactiveFormsModule, RouterLink],
    providers: [provideMomentDateAdapter()],
    templateUrl: './booking-sidebar.component.html',
    styleUrls: ['./booking-sidebar.component.scss']
})
export class BookingSidebarComponent implements OnInit
{
    displayedColumns: string[] = ['roomTypeId', 'startDate', 'endDate', 'discount', 'actions'];

    lodgings: Lodging[] = [];

    @Input() selectedLodging!: Lodging;
    @Output() closeBookingSidebar = new EventEmitter();

    @ViewChild('bookingForm')
    bookingForm!: NgForm;

    bookingFormGroup!: FormGroup;
    isLessor = false;
    isUserLogged!: boolean;
    selectedRoomTypePrice: number = 0;
    selectedTotalPrice: number = 0;
    public temporaryBookings: BookingRequestData[] = [];
    public dataSource: MatTableDataSource<BookingRequestData>;

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

    hasRoomTypes(lodging: Lodging) {
        return Lodging.offersRooms(lodging) && lodging.roomTypes!.length > 0;
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
                userName: user.userName!,
                lodgingId: this.selectedLodging!.id,
                rooms: [roomRequest]
            };

            this.temporaryBookings.push(bookingRequestData);
            this.dataSource.data = [...this.temporaryBookings]; // Actualiza la referencia de datos con una nueva copia
            this.cdr.detectChanges(); // Fuerza la detección de cambios
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
                userName: user.userName!,
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
                this.closeBookingSidebar.emit();
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
        this.dataSource.data = [...this.temporaryBookings];
        this.cdr.detectChanges();
    }

    public openBookingDrawer(lodging: Lodging) {
        if (this.isUserLogged) {
            this._lodgingService.getLodging(lodging.id).subscribe(data => {
                this.selectedLodging = data;
                this.closeBookingSidebar.emit();
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

    ngOnInit(): void {
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
        }

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
