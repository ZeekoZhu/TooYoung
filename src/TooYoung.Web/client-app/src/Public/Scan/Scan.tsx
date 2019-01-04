import './Scan.less';

import copy from 'clipboard-copy';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';
import { DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import React, { Component } from 'react';
import QrReader from 'react-qr-scanner';

// tslint:disable-next-line:no-empty-interface
interface IScanProps {
}

type ScanProps = IScanProps;

@observer
export class Scan extends Component<ScanProps> {
    @observable public data = '';
    @action.bound
    public dataHandler = (data: string) => {
        console.log(this.data);
        if (data !== null) {
            this.data = data;
        }
    }
    public openHandler = () => {
        window.open(this.data);
    }
    public copyHandler = () => {
        copy(this.data);
    }
    public render() {
        return (
            <div className='scan'>
                <div className='scanner'>
                    <QrReader
                        onError={() => false}
                        className='scanner'
                        onScan={this.dataHandler}
                    />
                </div>
                <div className='scan-result'>
                    <span>扫描结果</span>
                    <span>{this.data}</span>
                </div>
                <div className='buttons'>
                    <PrimaryButton
                        onClick={this.openHandler} text='打开链接' />
                    <DefaultButton
                        onClick={this.copyHandler} text='复制链接' />
                </div>
            </div>
        );
    }
}
