import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Booking } from '../../models/booking';
import { BookingService } from '../../services/booking.service';
import { AppState } from '../../models/app_state';
import { AsyncPipe, NgFor } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [RouterOutlet, NgFor, AsyncPipe],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.css'
})
export class BookingComponent implements OnInit{
  
  async ngOnInit() {
    
  }
}