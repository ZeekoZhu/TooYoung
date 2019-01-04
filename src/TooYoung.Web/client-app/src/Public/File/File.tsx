import './File.less';

import { action, computed } from 'mobx';
import { observer } from 'mobx-react';
import { PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

import { FilesAPI } from '../../api/files.api';
import { fileInfoToDoc, IDocument, WrappedProp } from '../../CommonTypes';

// tslint:disable-next-line:no-empty-interface
interface IFileProps {
}

type FileProps = IFileProps & RouteComponentProps<{ file: string, token: string, name: string }>;

@observer
class FileComp extends Component<FileProps> {
    file = new WrappedProp<IDocument | null>(null);
    tokenParam = new WrappedProp<string>('');
    fileInfoIdParam = new WrappedProp<string>('');
    fileNameParam = new WrappedProp<string>('');
    pwd = new WrappedProp('');
    @computed get unlocked() {
        return this.file.value !== null;
    }
    constructor(props: FileProps) {
        super(props);
    }

    componentDidMount() {
        this.bindParam();
    }

    bindParam() {
        const { file, token, name } = this.props.match.params;
        this.tokenParam.set(token);
        this.fileInfoIdParam.set(file);
        this.fileNameParam.set(decodeURIComponent(atob(name)));
    }

    loadFile() {
        FilesAPI.testAccess(this.fileInfoIdParam.value, this.fileNameParam.value, this.tokenParam.value, this.pwd.value)
            .subscribe(resp => {
                if (resp !== false) {
                    this.file.set(fileInfoToDoc(resp));
                }
            });
    }
    @action.bound
    public unlock = () => {
        this.loadFile();
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
                        {this.file.value!.name}
                    </span>
                </div>
                <div className='info ms-fontWeight-semilight'>
                    {/* <span>由{this.owner}共享</span> */}
                    <span>最后修改时间：{this.file.value!.dateModified}</span>
                </div>
                <div className='buttons'>
                    <PrimaryButton
                        onClick={() => {
                            // tslint:disable-next-line:max-line-length
                            const url = `${location.origin}/api/v1/files/${this.file.value!.id}/${this.file.value!.name}?token=${this.tokenParam.value}&pwd=${encodeURIComponent(this.pwd.value)}`;
                            window.open(url, '_blank');
                        }}
                        text={`下载（${this.file.value!.fileSize}）`} />
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
                        value={this.pwd.value}
                        onChange={(_, value) => this.pwd.set(value || '')}
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

export const File = withRouter(FileComp);
