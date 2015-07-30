(function (ns) {

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
        startNodeHeight: 30,
        startNodeStroke: '#34495E',
        startNodeFill: 'white',
        startNodeTextSize: 15,
        startNodeTextFill: '#34495E',
        startNodeTextFont: 'Tahoma',
        startNodeTextOffsetY: 2,
        startNodeCornerRadius: 5,

        // AddCriteriaNode (predefined diamond, which is used to create new criteria) parameters.
        addCriteriaNodeStroke: '#CACACA',
        addCriteriaNodeFill: 'white',
        addCriteriaNodeTextSize: 15,
        addCriteriaNodeTextFill: '#ACACAC',
        addCriteriaNodeTextFont: 'Tahoma',
        addCriteriaNodeTextOffsetY: 3,

        // CriteriaNode (criteria diamon that was added by user) parameters.
        criteriaNodeStroke: '#1A83C5',
        criteriaNodeFill: '#2B94D6',
        criteriaNodeTextSize: 15,
        criteriaNodeTextFill: 'white',
        criteriaNodeTextFont: 'Tahoma',

        // ActionsNode (actions panel on the right from criteria diamond) parameters.
        actionsNodeFill: 'white',
        actionsNodeStroke: '#34495E',
        actionsNodeCornerRadius: 5,

        // AddActionNode (add new action button) parameters.
        addActionNodePadding: 8,
        addActionNodeHeight: 30,
        addActionNodeTextSize: 15,
        addActionNodeTextFill: '#566B7F',
        addActionNodeTextFont: 'Tahoma',

        // ActionNode (single action that was added by user) parameters.
        actionNodePadding: 8,
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

})(Core.ns('ProcessBuilder'));