import React, { Component } from 'react';
import { observer } from 'mobx-react';
import { Nav, INavLink } from 'office-ui-fabric-react/lib/Nav';
import { withRouter, RouteComponentProps } from 'react-router-dom';

import './SideNav.less';

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
        }
    ];
    public render() {
        return (
            <div className="side-nav">
                <div className="placeholder">
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
