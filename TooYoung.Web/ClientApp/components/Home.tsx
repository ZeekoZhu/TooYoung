import * as React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import { Fabric } from 'office-ui-fabric-react/lib/Fabric';
import { DefaultButton } from 'office-ui-fabric-react/lib/Button';
import Header from './Header';

export default class Home extends React.Component<RouteComponentProps<{}>, {}> {
    public render() {
        return <Fabric>
            <div className='home'>
                <Header />
                <div className='page-content'>
                    <h1>Hello, React</h1>
                    <DefaultButton>
                        I am a button.
                    </DefaultButton>
                </div>
            </div>
        </Fabric>;
    }
}
