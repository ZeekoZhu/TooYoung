import './Profile.less';

import { observer } from 'mobx-react';
import { PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React, { Component } from 'react';

// tslint:disable-next-line:no-empty-interface
interface IProfileProps {
}

type ProfileProps = IProfileProps;

@observer
export class Profile extends Component<ProfileProps> {
    public render() {
        return (
            <>
                <div className='profile'>
                    <div className='title'>
                        <h2 className='ms-font-xxl ms-fontWeight-semilight'>个人信息</h2>
                    </div>
                    <div className='form'>
                        <TextField
                            required={true}
                            label='用户名'
                            underlined={true} />
                        <TextField
                            required={true}
                            label='显示名称'
                            underlined={true} />
                        <TextField
                            required={true}
                            label='邮箱地址'
                            underlined={true} />
                        <TextField
                            required={true}
                            label='密码'
                            type='password'
                            underlined={true} />
                        <TextField
                            required={true}
                            label='确认密码'
                            underlined={true} />
                    </div>
                    <div className='buttons'>
                        <PrimaryButton text='更新' />
                    </div>
                </div>
            </>
        );
    }
}
