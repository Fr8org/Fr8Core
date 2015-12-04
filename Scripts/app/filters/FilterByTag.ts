 /// <reference path="../_all.ts" />

/*
    This filter sorts out the list items with right tags
*/

module dockyard.filters.filterByTag {
    'use strict';
    app.filter('FilterByTag', () => 
        function (list: model.DropDownListItem[], filterByTag: string): model.DropDownListItem[]{
            var result = [];
            if (filterByTag) {
                list.forEach((item) => {
                    if (item.tags && item.tags.indexOf(filterByTag) !== -1) result.push(item);
                });
            } else {
                result = [].concat(list);
            }

            return result;
    });
}