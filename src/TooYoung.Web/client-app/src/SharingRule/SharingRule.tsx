import copy from 'clipboard-copy';
import { action, observable } from 'mobx';
import { observer } from 'mobx-react';
import { DefaultButton, PrimaryButton } from 'office-ui-fabric-react/lib/Button';
import { Callout, DirectionalHint } from 'office-ui-fabric-react/lib/Callout';
import {
    DocumentCard,
    DocumentCardPreview,
    DocumentCardTitle,
    DocumentCardType,
} from 'office-ui-fabric-react/lib/DocumentCard';
import React, { Component } from 'react';
import styled from 'styled-components';

import { isTokenRule } from '../Common';
import { ISharingRule } from '../CommonTypes';

const ImgPreivewDiv = styled.div`
    cursor:zoom-in;
`;

interface ISharingRuleProps {
    rule: ISharingRule;
}

const baseUrl = 'http://pan.gianthard.rocks/share';

type SharingRuleProps = ISharingRuleProps;

const generateUrl = (rule: ISharingRule) => {
    if (isTokenRule(rule)) {
        const url = `${baseUrl}/${rule.resourceId}/${rule.token}`;
        return url;
    } else {
        const url = baseUrl + rule.resourceId;
        return url;
    }
};

const qrcodeSrc = (content: string) => {
    return `/api/v1/qrcode/${encodeURIComponent(content)}`;
};

@observer
export class SharingRule extends Component<SharingRuleProps> {
    @observable showCallout = false;
    private previewImg = React.createRef<HTMLDivElement>();
    public render() {
        const { rule } = this.props;
        const ruleUrl = generateUrl(rule);
        return (
            <>
                <DocumentCard type={DocumentCardType.compact}>
                    <ImgPreivewDiv
                        onClick={e => {
                            e.preventDefault();
                            this.setCalloutVisablity(true);
                        }}
                        ref={this.previewImg}>
                        <DocumentCardPreview
                            previewImages={[
                                {
                                    previewImageSrc: qrcodeSrc(ruleUrl),
                                    height: 107,
                                    width: 107
                                }
                            ]}
                        />

                    </ImgPreivewDiv>
                    <div className='ms-DocumentCard-details'>
                        <DocumentCardTitle
                            title={ruleUrl}
                            shouldTruncate={true} />
                        {/* <DocumentCardActivity
                        activity="Sent a few minutes ago"
                        people={[{ name: 'Kat Larrson', profileImageSrc: TestImages.personaFemale }]}
                    /> */}
                        <div className='actions ms-textAlignRight'>
                            <DefaultButton
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
                    </div>
                </DocumentCard>
                <Callout
                    target={this.previewImg.current}
                    hidden={!this.showCallout}
                    onDismiss={() => {
                        this.setCalloutVisablity(false);
                    }}
                    directionalHint={DirectionalHint.leftCenter}
                >
                    <img src={qrcodeSrc(ruleUrl)} height='224' width='224' />
                </Callout>
            </>
        );

    }
    @action.bound
    public setCalloutVisablity = (visable: boolean) => {
        this.showCallout = visable;
    }
}
