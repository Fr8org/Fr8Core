module dockyard.tests.utils.fixtures {

    export class PlanBuilder {
        public static newPlan = <interfaces.IPlanVM>{
            id: '89EBF277-0CC4-4D6D-856B-52457F10C686',
            name: "MockPlan",
            description: "MockPlan",
            planState: model.PlanState.Inactive,
            subscribedDocuSignTemplates: [],
            externalEventSubscription: [],
            startingSubPlanId: 1
        };

        public static planBuilderState = new model.PlanBuilderState();

        public static updatedPlan = <interfaces.IPlanVM>{
            'name': 'Updated',
            'description': 'Description',
            'planState': model.PlanState.Inactive,
            'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
        }

        public static fullPlan = <interfaces.IPlanVM>{
            'name': 'Updated',
            'description': 'Description',
            'planState': model.PlanState.Inactive,
            'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626'],
            subPlans: [
                <model.SubPlanDTO>{
                    id: '89EBF277-0CC4-4D6D-856B-52457F10C686',
                    isTempId: false,
                    name: 'Processnode Template 1',
                    activities: [
                        <model.ActivityDTO>{
                            id: '89EBF277-0CC4-4D6D-856B-52457F10C686',
                            activityTemplate: {
                                name: 'test'
                            },
                            parentPlanNodeId: '89EBF277-0CC4-4D6D-856B-52457F10C686'
                        },
                        <model.ActivityDTO>{
                            id: '82B62831-687F-4BC8-AB64-B421985D5CF3',
                            activityTemplate: {
                                name: 'test'
                            },
                            parentPlanNodeId: '89EBF277-0CC4-4D6D-856B-52457F10C686'
                        }
                    ]
                }]
        }
    }
}
