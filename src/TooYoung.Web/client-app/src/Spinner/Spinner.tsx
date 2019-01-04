import { Spinner } from 'office-ui-fabric-react/lib/Spinner';
import React, { Component } from 'react';
import styled from 'styled-components';

const WrapperDiv = styled.div`
    position:relative;
`;

const OverlayDiv = styled.div`
    position: absolute;
    height: 100%;
    width: 100%;
    display:flex;
    justify-content: center;
    align-items: center;
    background:rgba(255,255,255,0.75);
    z-index: 10;
`;

interface IWaitSpinnerProps {
    show: boolean;
}

type WaitSpinnerProps = IWaitSpinnerProps;

export class WaitSpinner extends Component<WaitSpinnerProps> {
    public render() {
        const children = this.props.children;
        return (
            <WrapperDiv>
                {this.props.show ?
                    <OverlayDiv>
                        <Spinner label='请稍候...' />
                    </OverlayDiv> :
                    <></>
                }
                {children}
            </WrapperDiv>
        );
    }
}
