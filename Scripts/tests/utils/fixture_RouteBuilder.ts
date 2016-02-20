module dockyard.tests.utils.fixtures {

    export class RouteBuilder {
        public static newRoute = <interfaces.IRouteVM> {
            id: '89EBF277-0CC4-4D6D-856B-52457F10C686',
            name: "MockRoute",
            description: "MockRoute",
            routeState: 1,
            subscribedDocuSignTemplates: [],
            externalEventSubscription: [],
            startingSubrouteId: 1
        };

        public static routeBuilderState = new model.RouteBuilderState();

        public static updatedRoute = <interfaces.IRouteVM> {
            'name': 'Updated',
            'description': 'Description',
            'routeState': 1,
            'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626']
        }

        public static fullRoute = <interfaces.IRouteVM> {
            'name': 'Updated',
            'description': 'Description',
            'routeState': 1,
            'subscribedDocuSignTemplates': ['58521204-58af-4e65-8a77-4f4b51fef626'],
            subroutes: [
                <model.SubrouteDTO>{
                    id: '89EBF277-0CC4-4D6D-856B-52457F10C686',
                    isTempId: false,
                    name: 'Processnode Template 1',
                    activities: [
                        <model.ActivityDTO> {
                            id: '89EBF277-0CC4-4D6D-856B-52457F10C686',
                            activityTemplate: {
                                id: 1
                            },
                            parentRouteNodeId: '89EBF277-0CC4-4D6D-856B-52457F10C686'
                        },
                        <model.ActivityDTO>{
                            id: '82B62831-687F-4BC8-AB64-B421985D5CF3',
                            activityTemplate: {
                                id: 1
                            },
                            parentRouteNodeId: '89EBF277-0CC4-4D6D-856B-52457F10C686'
                        }
                    ]
                }]
        }
    }
}
