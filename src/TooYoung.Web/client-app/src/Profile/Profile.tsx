import './Profile.less';

import { IReactionDisposer, reaction } from 'mobx';
import { inject, observer } from 'mobx-react';
import { PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import React, { Component } from 'react';

import { selectAppStore, WithAppStore } from '../Context';
import { WaitSpinner } from '../Spinner/Spinner';
import { ProfileStore } from './Profile.store';

// tslint:disable-next-line:no-empty-interface
interface IProfileProps {
}

type ProfileProps = IProfileProps & WithAppStore;

@inject(selectAppStore)
@observer
export class Profile extends Component<ProfileProps> {
    store!: ProfileStore;
    profileDisposer!: IReactionDisposer;
    constructor(props: ProfileProps) {
        super(props);
        this.store = new ProfileStore(props.appStore!.auth);
    }
    componentDidMount() {
        this.store.setForm();
        this.profileDisposer = reaction(() => this.store.profile, () => {
            this.store.setForm();
        });
    }
    componentWillUnmount() {
        this.profileDisposer();
    }
    public render() {
        return (
            <>
                <div className='profile'>
                    <div className='title'>
                        <h2 className='ms-font-xxl ms-fontWeight-semilight'>个人信息</h2>
                    </div>
                    <WaitSpinner show={this.store.loading.value}>
                        <div className='form'>
                            <TextField
                                required={true}
                                label='用户名'
                                value={this.store.userName.value}
                                onChange={(_, value) => this.store.userName.set(value || '')}
                                underlined={true} />
                            <TextField
                                value={this.store.displayName.value}
                                onChange={(_, value) => this.store.displayName.set(value || '')}
                                required={true}
                                label='显示名称'
                                underlined={true} />
                            <TextField
                                value={this.store.email.value}
                                onChange={(_, value) => this.store.email.set(value || '')}
                                required={true}
                                label='邮箱地址'
                                underlined={true} />
                            <TextField
                                onChange={(_, value) => this.store.password.set(value || '')}
                                label='密码'
                                type='password'
                                underlined={true} />
                            <TextField
                                onChange={(_, value) => this.store.comfirmPwd.set(value || '')}
                                label='确认密码'
                                errorMessage={this.store.pwdErrorMsg}
                                underlined={true} />
                        </div>
                        <div className='buttons'>
                            <PrimaryButton text='更新' onClick={() => this.store.submitForm()} />
                        </div>
                    </WaitSpinner>
                </div>
            </>
        );
    }
}
