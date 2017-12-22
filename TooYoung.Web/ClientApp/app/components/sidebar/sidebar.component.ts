import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'sidebar',
    templateUrl: 'sidebar.component.html',
    styleUrls: ['./sidebar.component.css']
})

export class SidebarComponent implements OnInit {

    navItems: SideBarNavItem[] = [];
    ngOnInit() {
        this.navItems = [
            {
                text: '图床管理', routerLink: ['/group-panel'],
                items: [
                    { text: '分组管理', routerLink: ['/group-panel'] }
                ]
            }
        ];
    }
}

class SideBarNavItem {
    text: string;
    routerLink: any[];
    items?: SideBarNavItem[];
}
