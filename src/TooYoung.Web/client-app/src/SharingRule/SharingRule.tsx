import copy from 'clipboard-copy';
import { format as formatDate } from 'date-fns';
import { action, observable } from 'mobx';
import { inject, observer } from 'mobx-react';
import { DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { Callout, DirectionalHint } from 'office-ui-fabric-react/lib/Callout';
import { DocumentCardTitle } from 'office-ui-fabric-react/lib/DocumentCard';
import React, { Component } from 'react';
import styled from 'styled-components';

import { dateFormat } from '../Common';
import { selectAppStore, WithAppStore } from '../Context';
import { IFileInfo } from '../models/file';
import { IRefererRule, ITokenRule } from '../models/sharing';
import { SharingStore } from '../stores/Sharing.store';

const ImgPreivewDiv = styled.div`
    cursor:zoom-in;
`;

const InfoDiv = styled.div`
    display: flex;
    flex-flow: column nowrap;
    margin:16px;
`;

const DetailDiv = styled.div`
    display: flex;
    flex-flow: column nowrap;
    justify-content: space-between;
    width: 100%;
`;

const RuleDiv = styled.div`
    display: flex;
    justify-content:stretch;
    justify-content:stretch;
    border: 1px gray solid;
    margin:16px 0;
`;

type ISharingRule = IRefererRule | ITokenRule;
function isTokenRule(rule: ISharingRule): rule is ITokenRule {
    return (rule as ITokenRule).token !== undefined;
}

interface ISharingRuleProps {
    rule: IRefererRule | ITokenRule;
    file: IFileInfo;
}

const baseUrl = window.location.origin + '/api/v1/files';

type SharingRuleProps = WithAppStore & ISharingRuleProps;

const generateUrl = (rule: ISharingRule, file: IFileInfo) => {
    if (isTokenRule(rule)) {
        // tslint:disable-next-line:max-line-length
        const url = `${location.origin}/file-share/${file.id}/token/${rule.token}/file/${btoa(encodeURIComponent(file.name))}`;
        return url;
    } else {
        const url = `${baseUrl}/${file.id}/${file.name}`;
        return url;
    }
};

const qrcodeSrc = (content: string) => {
    return `/api/v1/qrcode/${encodeURIComponent(content)}`;
};

@inject(selectAppStore)
@observer
export class SharingRule extends Component<SharingRuleProps> {
    @observable showCallout = false;
    sharingStore!: SharingStore;
    private previewImg = React.createRef<HTMLDivElement>();
    constructor(props: SharingRuleProps) {
        super(props);
        this.sharingStore = props.appStore!.sharing;
    }
    public render() {
        const { rule, file } = this.props;
        const ruleUrl = generateUrl(rule, file);
        return (
            <>
                <RuleDiv>
                    <ImgPreivewDiv
                        onClick={e => {
                            e.preventDefault();
                            this.setCalloutVisablity(true);
                        }}
                        ref={this.previewImg}>
                        <img src={qrcodeSrc(ruleUrl)} height={130} width={130} />
                    </ImgPreivewDiv>
                    <DetailDiv>
                        <InfoDiv>
                            {isTokenRule(this.props.rule) ?
                                <>
                                    <span><label>过期时间：</label>{formatDate(this.props.rule.expiredAt, dateFormat)}</span>
                                    <span><label>提取码：</label>{this.props.rule.password}</span>
                                </> :
                                <>
                                    <span><label>允许的域名模式：</label>{this.props.rule.allowedHost}</span>
                                </>
                            }
                        </InfoDiv>
                        <div className='actions ms-textAlignRight'>
                            <DefaultButton
                                onClick={() => {
                                    if (isTokenRule(this.props.rule)) {
                                        this.sharingStore.deleteRule('token', this.props.rule.id, this.props.file.id);
                                    } else {
                                        this.sharingStore.deleteRule('referer', this.props.rule.id, this.props.file.id);
                                    }
                                }}
                                iconProps={{
                                    iconName: 'Delete'
                                }}>删除</DefaultButton>
                            <PrimaryButton
                                text='复制链接'
                                onClick={() => {
                                    copy(ruleUrl);
                                }}
                                iconProps={{
                                    iconName: 'Copy'
                                }} />
                        </div>
                    </DetailDiv>
                </RuleDiv>
                <Callout
                    target={this.previewImg.current}
                    hidden={!this.showCallout}
                    onDismiss={() => {
                        this.setCalloutVisablity(false);
                    }}
                    directionalHint={DirectionalHint.leftCenter}
                >
                    <img src={qrcodeSrc(ruleUrl)} height='300' width='300' />
                </Callout>
            </>
        );

    }
    @action.bound
    public setCalloutVisablity = (visable: boolean) => {
        this.showCallout = visable;
    }
}
