import './File.less';

import formatDate from 'date-fns/format';
import filesize from 'filesize';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';
import { PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React, { Component } from 'react';

import { dateFormat } from '../../Common';
import { IDocument } from '../../CommonTypes';

// tslint:disable-next-line:no-empty-interface
interface IFileProps {
}

type FileProps = IFileProps;

@observer
export class File extends Component<FileProps> {
    @observable public file!: IDocument;
    @observable public owner = 'Foo Bar';
    @observable public unlocked = false;
    constructor(props: FileProps) {
        super(props);
        this.file = {
            id: 'data.zip',
            name: 'data.zip',
            dateModified: formatDate(new Date().setDate(2), dateFormat),
            dateModifiedValue: +new Date(),
            fileSize: filesize(231231),
            fileSizeRaw: 231231,
            iconName: 'Page',
            fileType: 'zip',
            value: 'uuid-for-zip',
            sharedLinks: 2
        };
    }
    @action.bound
    public unlock = () => {
        this.unlocked = true;
    }
    public render() {
        return (
            <div className='file'>
                <div className='begin'></div>
                {this.unlocked
                    ? this.renderUnlocked()
                    : this.renderLocked()}
                <div className='end'></div>
            </div>
        );
    }
    public renderUnlocked() {
        return (
            <>
                <div className='filename'>
                    <span className='ms-font-xxl ms-fontWeight-regular'>
                        {this.file.name}
                    </span>
                </div>
                <div className='info ms-fontWeight-semilight'>
                    <span>由{this.owner}共享</span>
                    <span>最后修改时间：{this.file.dateModified}</span>
                </div>
                <div className='buttons'>
                    <PrimaryButton text={`下载（${this.file.fileSize}）`} />
                </div>
            </>
        );
    }
    public renderLocked() {
        return (
            <>
                <div className='info'>
                    <TextField
                        label='提取码'
                        underlined={true} />
                </div>
                <div className='buttons'>
                    <PrimaryButton
                        onClick={this.unlock}
                        text='确认' />
                </div>
            </>
        );
    }
}
