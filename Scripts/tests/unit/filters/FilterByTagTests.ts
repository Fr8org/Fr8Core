/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.unit.filters {

    var filter: (list: model.DropDownListItem[], filterByTag: string) => model.DropDownListItem[];
    //app.service('filterByTagTest', ['$filter', ($filter) => {
    //    filter = $filter('FilterByTag')();
    //}]);

    var fieldList = [
        <model.DropDownListItem> { key: 'item1', value: '1', tags: 'tag' },
        <model.DropDownListItem> { key: 'item2', value: '2', tags: 'tag' },
        <model.DropDownListItem> { key: 'item3', value: '3', tags: 'tag2' },
        <model.DropDownListItem> { key: 'item4', value: '4', tags: 'tag2' },
        <model.DropDownListItem> { key: 'item5', value: '5', tags: 'tag3, tag2' },
        <model.DropDownListItem> { key: 'item6', value: '6', tags: 'tag3' },
    ];

    describe('FilterByTag', () => {
        beforeEach(module('app'));
        beforeEach(inject((_$filter_) => {
            filter = _$filter_('FilterByTag');
        }));

        it('should filter out items with specified tag', () => {
            var filtered = filter(fieldList, 'tag');
            expect(filtered.length).toBe(2);
            filtered.forEach((field) => {
                expect(field.tags).toBe('tag');
            });
        });

        it('should not change the original list', () => {
            var filtered = filter(fieldList, 'tag');
            expect(fieldList.length).toBe(6);
            expect(filtered).not.toBe(fieldList);
        });

        it('should return empty list if no entries contain right tag', () => {
            var filtered = filter(fieldList, 'noTag');
            expect(filtered.length).toBe(0);
        });

        it('should understand coma-separated tags', () => {
            var filtered = filter(fieldList, 'tag2');
            expect(filtered.length).toBe(3);
            expect(filter(fieldList, 'tag3').length).toBe(2);
        });

    });

} 