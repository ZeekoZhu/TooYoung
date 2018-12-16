import React, { Component } from 'react';
import { observer, inject } from 'mobx-react';
import { DocumentCard, DocumentCardType, DocumentCardPreview, DocumentCardTitle, DocumentCardActivity } from 'office-ui-fabric-react/lib/DocumentCard';
import { isTokenRule } from '../Common';
import { ISharingRule } from '../CommonTypes';

interface ISharingRuleProps {
    rule: SharingRule;
}

const baseUrl = 'http://pan.gianthard.rocks/share/';

type SharingRuleProps = ISharingRuleProps;

const generateUrl = (rule: ISharingRule) => {
    if (isTokenRule(rule)) {
        const url = baseUrl + rule.token;
    }
}

@observer
export class SharingRule extends Component<SharingRuleProps> {
    public render() {

        return (
            <DocumentCard type={DocumentCardType.compact} onClickHref="http://bing.com">
                <DocumentCardPreview previewImages={[
                    {

                    }
                ]} />
                <div className="ms-DocumentCard-details">
                    <DocumentCardTitle title="Conversation about anual report from SharePoint conference" shouldTruncate={true} />
                    {/* <DocumentCardActivity
                        activity="Sent a few minutes ago"
                        people={[{ name: 'Kat Larrson', profileImageSrc: TestImages.personaFemale }]}
                    /> */}
                </div>
            </DocumentCard>
        );
    }
}
