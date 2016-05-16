namespace dockyard.tests.unit.directives {
    import dd = dockyard.directives;
    import Fr8InternalEvent = dockyard.controllers.NotifierController.Fr8InternalEvent
    
    describe('Collapse Directive', () => {
        let $compile: ng.ICompileService,
            $rootScope: ng.IRootScopeService,
            scope: dd.IFr8EventScope,
            elScope: dd.IFr8CollapseScope,
            element: ng.IAugmentedJQuery,
            elCtrl: dd.Fr8CollapseCtrl,
            evtData: any = {
                ActivityName: 'Test Activity Name',
                PlanName: 'Test Plan',
                ContainerId: '1234-1234-1234-1234-1234-1234-1234-1234-1234-1234-1234-1234-1234-1234-1234-1234'
            };

        beforeEach(module('fr8.collapse'));

        beforeEach(inject(($injector) => {
            $compile = $injector.get('$compile');
            $rootScope = $injector.get('$rootScope');
            scope = <dd.IFr8EventScope> $rootScope.$new();
            scope.event = angular.copy(evtData);
            element = $compile(`
                <md-list>
                    <md-list-item class="md-2-line fr8-collapse-heading" fr8-collapse-heading>
                        <div class="md-list-item-text" layout="column">
                            <h3>Executing Activity</h3>
                            <h4>{{event.ActivityName}}</h4>
                            <div fr8-collapse-content>
                                <p>For Plan: {{event.PlanName}}</p>
                                <p>Container: {{event.ContainerId}}</p>
                            </div>
                        </div>
                    </md-list-item>
                </md-list>
            `)(scope);
            scope.$digest();
            elCtrl = <dd.Fr8CollapseCtrl> element.find('md-list-item[fr8-collapse-heading]').controller('fr8CollapseHeading');
            elScope = <dd.IFr8CollapseScope> element.find('div[fr8-collapse-content]').isolateScope();
        }));

        it('should indicate collapse state', () => {
            expect(elCtrl.isCollapsed).toBeDefined();
        });
        
        it('should initialize collapsed', () => {
            expect(elCtrl.isCollapsed).toBe(true);
        });
        
        it('should have a collapse toggle method', () => {
            expect(elCtrl.toggle).toBeDefined();
        });

        it('should expand when toggled', () => {
            elCtrl.toggle();
            expect(elCtrl.isCollapsed).toBe(false);
        });

        it('should reference the same controller in both directives', () => {
            expect(elCtrl).toEqual(elScope.ctrl);
        });

        it('should remove the content from the content element', () => {
            expect(element.children('.md-list-item-text').children('div[fr8-collapse-content]').children().length).toBe(0);
        });

        it('should add the content as an item list element', () => {
            expect(element.find('md-list-item.fr8-collapse-content').length).toBe(1);
        });

        it('should add a collapse marker', () => {
            expect(element.find('md-icon.fr8-collapse-marker').length).toBe(1);
        });
        
    });
}
