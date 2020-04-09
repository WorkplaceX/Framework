import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BootstrapNavbarComponent } from './bootstrap-navbar.component';

describe('BootstrapNavbarComponent', () => {
  let component: BootstrapNavbarComponent;
  let fixture: ComponentFixture<BootstrapNavbarComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BootstrapNavbarComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BootstrapNavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
