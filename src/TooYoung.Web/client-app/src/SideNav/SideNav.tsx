import './SideNav.less';

import { computed } from 'mobx';
import { inject, observer } from 'mobx-react';
import { INavLink, Nav } from 'office-ui-fabric-react/lib/Nav';
import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';

import { selectAppStore, WithAppStore } from '../Context';
import { AuthStore } from '../stores/Auth.store';

// tslint:disable-next-line:no-empty-interface
interface ISideNavProps {
}

type SideNavProps = ISideNavProps & RouteComponentProps & WithAppStore;

@inject(selectAppStore)
@observer
class SideNavComp extends Component<SideNavProps> {
    authStore!: AuthStore;
    @computed
    get links() {
        const links = [
            {
                name: '文件管理',
                url: '/files',
                forceAnchor: true
            },
            {
                name: '分享管理',
                url: '/shared',
                forceAnchor: true
            },
        ];
        if (this.authStore.isAdmin) {
            links.push(
                {
                    name: '用户管理',
                    url: '/admin',
                    forceAnchor: true
                }
            );
        }
        return links;
    }
    constructor(props: SideNavProps) {
        super(props);
        this.authStore = props.appStore!.auth;
    }
    public render() {
        return (
            <div className='side-nav'>
                <div className='placeholder'>
                </div>
                <Nav
                    groups={[
                        {
                            links: this.links
                        }
                    ]}
                    onLinkClick={(event, link) => {
                        event!.preventDefault();
                        this.props.history.push(link!.url);
                    }}
                />
            </div>
        );
    }
}

export const SideNav = withRouter(SideNavComp);
