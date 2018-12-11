import React, { Component } from 'react';
import { observer, inject } from 'mobx-react';

interface IFilesProps {
}

type FilesProps = IFilesProps;


@observer
export class Files extends Component<FilesProps> {
    public render() {
        return (
            <p>Files component</p>
        );
    }
}
