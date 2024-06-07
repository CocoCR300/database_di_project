import { Component, OnInit, ViewChild } from '@angular/core';
import { LodgingService } from '../../services/lodging.service';
import { Dialogs } from '../../util/dialogs';
import { firstValueFrom, of } from 'rxjs';
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
import { Booking, BookingStatus } from '../../models/booking';
import { Router } from '@angular/router';
import { NotificationService } from '../../services/notification.service';
import { MatButtonModule } from '@angular/material/button';
import { server } from '../../services/global';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';

@Component({
    selector: 'app-lodging',
    standalone: true,
    imports: [AsyncPipe, CurrencyPipe, FormsModule, NgFor, NgIf, MatButtonModule, MatDatepickerModule, MatFormFieldModule, MatInputModule, MatPaginatorModule, MatSidenavModule, ReactiveFormsModule],
    providers: [provideMomentDateAdapter()],
    templateUrl: './lodging.component.html',
    styleUrl: './lodging.component.scss'
})
export class LodgingComponent implements OnInit {
    private _lodgings!: Lodging[];

    @ViewChild('bookingForm')
    bookingForm!: NgForm;
    @ViewChild('paginator')
    paginator!: MatPaginator;
    @ViewChild(MatDrawer)
    sidebar!: MatDrawer;

    bookingFormGroup!: FormGroup;
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
    searchTermCurrentTimeout!: any;

    public constructor(
        private _appState: AppState,
        private _bookingService: BookingService,
        private _lodgingService: LodgingService,
        private _notificationService: NotificationService,
        private _userService: UserService,
        private router: Router
    ) {
    }

    prependImagesRoute(lodging: Lodging) {
        let imageSrc = "";
        if (lodging.photos != null) {
            imageSrc = `${server.lodgingImages}${lodging.photos[0]}`;
        }

        return imageSrc;
    }

    public async createLodging() {
        this.router.navigate(["lodging/create"]);
    }

    public async editLodging(lodgingId: number) {
        this.router.navigate(["lodging", lodgingId]);
    }

    public async submitBooking() {
    //    if (!this.bookingForm.valid) {
    //        return;
    //    }
    //
    //    const startDateMoment = this.bookingFormGroup.get("startDate")?.value as moment.Moment;
    //    const endDateMoment = this.bookingFormGroup.get("endDate")?.value as moment.Moment;
    //    let startDate = startDateMoment.format("yyyy-MM-DD");
    //    let endDate = endDateMoment.format("yyyy-MM-DD");
    //    
    //    const user = await firstValueFrom(this._userService.getUser(this._appState.userName!));
    //
    //    const booking = new Booking(
    //        0,
    //        this.selectedLodging!.lodging_id,
    //        user.person_id!,
    //        BookingStatus.Created,
    //        startDate,
    //        endDate);
    //    const response = await firstValueFrom(this._bookingService.postBooking(booking));
    //    
    //    if (response.ok) {
    //        Swal.fire({
    //            icon: "success",
    //            title: "La reserva ha sido creada con éxito."
    //        });
    //        this.sidebar.close();
    //    }
    //    else {
    //        for (const message of AppResponse.getErrors(response)) {
    //            this._notificationService.show(message);
    //        }
    //    }
    }

    public openBookingDrawer(lodging: Lodging) {
        if (this.isUserLogged) {
            this.selectedLodging = lodging;
            this.sidebar.open();
        }
        else {
            this.router.navigate(["login"]);
        }
    }

    public bookingDrawerClosed() {
        this.bookingForm.resetForm();
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
        if (component.searchTerm != "") {
            const searchTermUppercase = component.searchTerm.toLocaleUpperCase();
            component._filteredLodgings = component._lodgings.filter(lodging => {
                // yeah, this again
                return lodging.name.toLocaleUpperCase().includes(searchTermUppercase)
                        || lodging.description.toLocaleUpperCase().includes(searchTermUppercase)
                        || lodging.address.toLocaleUpperCase().includes(searchTermUppercase);
            });
        }
        else {
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
                try {
                    await firstValueFrom(this._lodgingService.deleteBookings(lodgingBookings.map(booking => booking.id)));
                }
                catch (error: any) {
                    console.log(error);
                    Swal.fire({
                        icon: "error",
                        title: "Ha ocurrido un error al eliminar las reservas",
                        text: error.message
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
                    // lodgings, lodgings, lodgings, lodgings, lodgings
                    this._lodgings = this._lodgings.filter(lodging => lodging.id == lodgingId);
                    this.updatePagedList(this.currentPage);

                    Swal.fire({
                        icon: "info",
                        title: "Alojamiento eliminado con éxito.",
                    });
                    console.log("Eliminado con éxito.");
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
        if(endIndex > this._lodgings.length) {
          // TODO: Get more lodgings, if available
          endIndex = this._lodgings.length;
        }

        let lodgings = this._lodgings;
        if (this._filteredLodgings != null) {
            // TODO: Filtering should be done in the API now, it's missing filtering by address though
            lodgings = this._filteredLodgings;
        }

        this.pagedLodgings = lodgings.slice(startIndex, endIndex);
    }

    ngOnInit(): void {
        this.bookingFormGroup = new FormGroup({
            startDate: new FormControl<Date | null>(null, Validators.required),
            endDate: new FormControl<Date | null>(null, Validators.required)
        });

        this.isUserLogged = this._appState.isUserLogged;
        if(this.isUserLogged) {
            this.isLessor = this._appState.role == UserRoleEnum.Lessor;
            this.canDelete = this._appState.role == UserRoleEnum.Administrator || this.isLessor;
        }
        this.canBook = !this.isUserLogged || this._appState.role == UserRoleEnum.Customer;

        if (this.isLessor) {
            this.title = "Mis alojamientos";
            this._userService.getUser(this._appState.userName!).subscribe(user => {
                this._lodgingService.getLessorLodgings(user.person!.id!).subscribe(lodgings => {
                    this._lodgings = lodgings;
                    this.updatePagedList(0);
                });
            });
        }
        else {
            this._lodgingService.getLodgings(100, 1).subscribe(lodgings => {
                this._lodgings = lodgings;
                this.updatePagedList(0);
            });
        }
    }
}
