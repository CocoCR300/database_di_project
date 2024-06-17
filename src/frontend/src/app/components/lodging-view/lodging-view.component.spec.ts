import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LodgingViewComponent } from './lodging-view.component';

describe('LodgingInfoComponent', () => {
  let component: LodgingViewComponent;
  let fixture: ComponentFixture<LodgingViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LodgingViewComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LodgingViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
