import { Component, OnInit } from '@angular/core';
import { CarouselModule, ThemeDirective } from '@coreui/angular';
import { Lodging } from '../../models/lodging';
import { LodgingService } from '../../services/lodging.service';
import { firstValueFrom } from 'rxjs';
import { NgFor, NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { server } from '../../services/global';
import { MatButtonModule } from '@angular/material/button';
import { AppState } from '../../models/app_state';
import { UserRoleEnum } from '../../models/user';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CarouselModule, MatButtonModule, NgFor, NgIf, RouterLink, ThemeDirective],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit
{
  lodgings: Lodging[] | null = null;

  public constructor(
    private _appState: AppState,
    private _lodgingService: LodgingService,
    private _router: Router
  ) { }

  public get isCustomer() {
    return this._appState.role == UserRoleEnum.Customer;
  }

  prependImagesRoute(lodging: Lodging) {
    let imageSrc = "";
    if (lodging.photos!.length > 0) {
      imageSrc = `${server.lodgingImages}${lodging.photos![0]}`;
    }

    return imageSrc;
  }

  lodgingsButtonClicked() {
    this._router.navigate(["lodging"]);
  }

  async ngOnInit() {
    this._lodgingService.getLodgings(50, 1).subscribe(lodgings =>
      this.lodgings = lodgings.filter(lodging => lodging.photos!.length > 0)
    );
  }
}
