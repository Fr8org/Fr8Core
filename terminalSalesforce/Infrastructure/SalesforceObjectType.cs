namespace terminalSalesforce.Infrastructure
{
    public enum SalesforceObjectType
    {
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Account,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Case,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        CollaborationGroup,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Contact,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceProperties.HasChatter)]
        ContentDocument,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Contract,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceProperties.HasChatter)]
        Dashboard,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceProperties.HasChatter)]
        DashboardComponent,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Document,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Entitlement,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Event,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Group,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceProperties.HasChatter)]
        KnowledgeArticle,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Lead,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Opportunity,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.None)]
        Order,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Product2,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Quote,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceProperties.HasChatter)]
        Report,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        ServiceContract,
        [SalesforceObjectDescription(SalesforceObjectOperations.None, SalesforceProperties.HasChatter)]
        Site,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Solution,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Task,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        Topic,
        [SalesforceObjectDescription(SalesforceObjectOperations.Create, SalesforceProperties.HasChatter)]
        User
    }
}