import * as React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';
import { DefaultButton } from 'office-ui-fabric-react/lib/Button';
import Header from './Header';
import { Route } from 'react-router-dom';
import ImagesPanel from './ImagesPanel';
import { SideNav } from './SideNav';
import { routeConfig } from '../routes';


export default class Home extends React.Component<{}, {}> {
    public render() {
        return <Fabric>
            <div className='home'>
                <Header />
                <div style={{ marginTop: '60px' }}>
                    <SideNav routes={routeConfig} />
                    <div className='page-content'>
                        {this.props.children}
                    </div>
                </div>
            </div>
        </Fabric>;
    }
}
