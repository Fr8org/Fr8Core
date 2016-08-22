(function (ns) {

    ns.ActionType = {
        immediate: 1,
        scheduled: 2
    };

    // Style constants.
    ns.WidgetConsts = {
        // Padding from canvas edge.
        canvasPadding: 10,
        // Minimum space between nodes (i.e. defalt length of arrows).
        minSpaceBetweenObjects: 40,
        // Default node width and height.
        defaultSize: 130,
        // Default stroke thickness.
        strokeWidth: 1,

        // Canvas background color.
        canvasBgFill: '#E5ECF6',

        // StartNode parameters.
        startNodeWidth: 170,
        startNodeHeight: 72,
        startNodeStroke: '#34495E',
        startNodeFill: 'white',
        startNodeTextSize: 15,
        startNodeTextFill: 'white',
        startNodeTextFont: 'Tahoma',
        startNodeTextWeight: 'normal',
        startNodeTextOffsetY: 2,
        startNodeCornerRadius: 5,
        startNodeBgImage: '/Content/img/PlanBuilder/start-node-bg.png',

        // AddCriteriaNode (predefined diamond, which is used to create new criteria) parameters.
        addCriteriaNodeWidth: 150,
        addCriteriaNodeHeight: 128,
        addCriteriaNodeStroke: '#CACACA',
        addCriteriaNodeFill: 'white',
        addCriteriaNodeTextSize: 15,
        addCriteriaNodeTextFill: '#ACACAC',
        addCriteriaNodeTextFont: 'Tahoma',
        addCriteriaNodeTextOffsetX: 6,
        addCriteriaNodeTextOffsetY: 1,
        addCriteriaNodeBgImage: '/Content/img/PlanBuilder/add-criteria-node-bg.png',

        // CriteriaNode (criteria diamon that was added by user) parameters.
        criteriaNodeWidth: 146,
        criteriaNodeHeight: 124,
        criteriaNodeStroke: '#1A83C5',
        criteriaNodeFill: '#2B94D6',
        criteriaNodeTextSize: 15,
        criteriaNodeTextFill: 'white',
        criteriaNodeTextFont: 'Tahoma',
        criteriaNodeTextOffsetX: 0,
        criteriaNodeTextOffsetY: 2,
        criteriaNodeBgImage: '/Content/img/PlanBuilder/criteria-node-bg.png',

        // ActionsNode (actions panel on the right from criteria diamond) parameters.
        actionsNodeFill: 'white',
        actionsNodeStroke: '#34495E',
        actionsNodeCornerRadius: 5,
        actionsNodeWidth: 227,
        actionsNodeTopHeight: 35,
        actionsNodeBottomHeight: 8,
        actionsNodeTopImage: '/Content/img/PlanBuilder/actions-node-top.png',
        actionsNodeBottomImage: '/Content/img/PlanBuilder/actions-node-bottom.png',
        actionsNodeBgImage: '/Content/img/PlanBuilder/actions-node-bg.png',

        // AddActionNode (add new action button) parameters.
        addActionNodePadding: 8,
        addActionNodeHeight: 30,
        addActionNodeTextSize: 15,
        addActionNodeTextFill: '#566B7F',
        addActionNodeTextFont: 'Tahoma',
        addActionNodeAddImage: '/Content/img/PlanBuilder/action-add-small.png',

        // ActionNode (single action that was added by user) parameters.
        actionNodePadding: 5,
        actionNodeHeight: 30,
        actionNodeTextSize: 15,
        actionNodeTextFill: 'black',
        actionNodeTextFont: 'Tahoma',

        // Arrow parameters.
        arrowStroke: '#B3BCC3',
        arrowStrokeWidth: 1,
        arrowSize: 4,
        arrowPadding: 4
    };

})(Core.ns('PlanBuilder'));