namespace terminalSalesforce.Infrastructure
{
    public enum SalesforceObjectType
    {
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Account,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Case,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        CollaborationGroup,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Contact,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceObjectProperties.HasChatter)]
        ContentDocument,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Contract,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceObjectProperties.HasChatter)]
        Dashboard,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceObjectProperties.HasChatter)]
        DashboardComponent,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Document,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Entitlement,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Event,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.None)]
        FeedItem,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Group,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceObjectProperties.HasChatter)]
        KnowledgeArticle,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Lead,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Opportunity,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.None)]
        Order,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Product2,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Quote,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceObjectProperties.HasChatter)]
        Report,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        ServiceContract,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceObjectProperties.HasChatter)]
        Site,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Solution,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Task,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        Topic,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceObjectProperties.HasChatter)]
        User
    }
}