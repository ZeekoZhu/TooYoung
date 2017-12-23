import * as React from 'react';
import {
    Nav,
    INavLinkGroup,
    INavLink
}

from 'office-ui-fabric-react/lib/Nav';
import {
    withRouter
}

from 'react-router-dom';
export interface ISideNavProps {
    routes: any;
}

const ParseGroup=(route: any)=> {
    return {
        name: route.name, key: route.path
    }
    ;
}

;
export class SideNav extends React.Component<ISideNavProps,
any> {
    navWithRouter=withRouter(( {
        history
    }
    )=> {
        return ( <Nav groups= {
            [ {
                links: this.props.routes.filter((r: any)=> !r.hide).map(ParseGroup)
            }
            ]
        }
        onLinkClick= {
            (ev?: React.MouseEvent<HTMLElement>, item?: INavLink)=> {
                if (item && item.key) {
                    history.push(item.key);
                }
            }
        }
        selectedKey= {
            history.location.pathname
        }
        ></Nav>);
    }
    );
    render() {
        return ( <div className='sidebar'> <this.navWithRouter /> </div>);
    }
}
