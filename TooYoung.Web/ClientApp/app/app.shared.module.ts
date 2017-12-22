import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { DxButtonModule } from 'devextreme-angular';

import { AppComponent } from './components/app/app.component';
import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';

@NgModule({
    declarations: [
        AppComponent,
        HeaderComponent,
        SidebarComponent
    ],
    imports: [
        CommonModule,
        HttpModule,
        FormsModule,
        DxButtonModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {
}
