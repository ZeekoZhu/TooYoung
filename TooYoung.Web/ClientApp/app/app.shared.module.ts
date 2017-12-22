import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { DxButtonModule, DxTreeViewModule, DxTemplateModule } from 'devextreme-angular';

import { AppComponent } from './components/app/app.component';
import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { GroupPanelComponent } from './components/group-panel/group-panel.component';

@NgModule({
    declarations: [
        AppComponent,
        HeaderComponent,
        SidebarComponent,
        GroupPanelComponent
    ],
    imports: [
        CommonModule,
        HttpModule,
        FormsModule,
        DxButtonModule, DxTreeViewModule, DxTemplateModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'group-panel', pathMatch: 'full' },
            { path: 'group-panel', component: GroupPanelComponent },
            { path: '**', redirectTo: 'group-panel' }
        ])
    ]
})
export class AppModuleShared {
}
