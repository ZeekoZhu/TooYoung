import './SideNav.less';

import { observer } from 'mobx-react';
import { INavLink, Nav } from 'office-ui-fabric-react/lib/Nav';
import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router-dom';

// tslint:disable-next-line:no-empty-interface
interface ISideNavProps {
}

type SideNavProps = ISideNavProps & RouteComponentProps;

@observer
class SideNavComp extends Component<SideNavProps> {
    private links: INavLink[] = [
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
        {
            name: '用户管理',
            url: '/admin',
            forceAnchor: true
        }
    ];
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
