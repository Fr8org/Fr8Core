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

        // StartNode parameters.
        startNodeHeight: 30,
        startNodeStroke: 'coral',
        startNodeFill: 'lightsalmon',
        startNodeTextSize: 15,
        startNodeTextFill: 'white',
        startNodeTextFont: 'Tahoma',
        startNodeTextOffsetY: 3,

        // AddCriteriaNode (predefined diamond, which is used to create new criteria) parameters.
        addCriteriaNodeStroke: '#FFCC00',
        addCriteriaNodeFill: '#FFFF99',
        addCriteriaNodeTextSize: 15,
        addCriteriaNodeTextFill: '#303030',
        addCriteriaNodeTextFont: 'Tahoma',
        addCriteriaNodeTextOffsetY: 3,

        // CriteriaNode (criteria diamon that was added by user) parameters.
        criteriaNodeStroke: '#99FF00',
        criteriaNodeFill: '#CCFF99',
        criteriaNodeTextSize: 15,
        criteriaNodeTextFill: 'black',
        criteriaNodeTextFont: 'Tahoma',

        // ActionsNode (actions panel on the right from criteria diamond) parameters.
        actionsNodeFill: 'white',
        actionsNodeStroke: 'red',

        // AddActionNode (add new action button) parameters.
        addActionNodePadding: 5,
        addActionNodeHeight: 30,
        addActionNodeTextSize: 15,
        addActionNodeTextFill: 'black',
        addActionNodeTextFont: 'Tahoma',

        // ActionNode (single action that was added by user) parameters.
        actionNodePadding: 5,
        actionNodeHeight: 30,
        actionNodeTextSize: 15,
        actionNodeTextFill: 'black',
        actionNodeTextFont: 'Tahoma',

        // Arrow parameters.
        arrowStroke: '#303030',
        arrowStrokeWidth: 1,
        arrowSize: 4,
        arrowPadding: 4
    };

})(Core.ns('ProcessBuilder'));