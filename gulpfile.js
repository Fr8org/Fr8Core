    var gulp = require('gulp');
var bower = require('gulp-bower');
var concat = require('gulp-concat');
var sourcemaps = require('gulp-sourcemaps');
var templateCache = require('gulp-angular-templatecache');

gulp.task('bower', function () {
    return bower({ layout: "byComponent" });
});

gulp.task('concattemplates', function () {
    return gulp.src('Views/AngularTemplate/**/*.cshtml')
      .pipe(templateCache('templateCache.js', {
          module: 'templates', standalone: true, transformUrl: function (url) {
              //remove .cshtml extension and /AngularTemplate/ prefix
              return '/AngularTemplate/' + url.slice(0, -7);
          }
      }))
      .pipe(gulp.dest('Scripts/tests/templates'));
});

gulp.task('compile_js', function () {
    return gulp.src([
        'Scripts/app/model/ActionDTO.js',
        'Scripts/app/model/ActivityTemplate.js',
        'Scripts/app/model/Condition.js',
        'Scripts/app/model/Criteria.js',
        'Scripts/app/model/Field.js',
        'Scripts/app/model/Subroute.js',
        'Scripts/app/model/ControlsList.js',
        'Scripts/app/model/CrateStorage.js',
        'Scripts/app/model/FieldMappingSettings.js',
        'Scripts/app/model/Route.js',
        'Scripts/app/model/RouteBuilderState.js',
        'Scripts/app/model/User.js',
        'Scripts/app/model/ContainerDTO.js',
        'Scripts/app/model/ActionGroup.js',
        'Scripts/app/model/WebServiceDTO.js',
        'Scripts/app/model/WebServiceActionSetDTO.js',
        'Scripts/app/model/TerminalDTO.js',
        'Scripts/app/model/TerminalActionSetDTO.js',
        'Scripts/app/services/CrateHelper.js',
        'Scripts/app/services/RouteBuilderService.js',
        'Scripts/app/services/StringService.js',
        'Scripts/app/services/LocalIdentityGenerator.js',
        'Scripts/app/services/ReportService.js',
        'Scripts/app/services/ManageFileService.js',
        'Scripts/app/services/ContainerService.js',
        'Scripts/app/services/UIHelperService.js',
        'Scripts/app/services/LayoutService.js',
        'Scripts/app/services/PusherNotifierService.js',
        'Scripts/app/services/UserService.js',
        'Scripts/app/services/WebServiceService.js',
        'Scripts/app/services/TerminalService.js',
        'Scripts/app/filters/RouteState.js',
        'Scripts/app/filters/ContainerState.js',
        'Scripts/app/filters/FilterByTag.js',
        'Scripts/app/directives/EventArgsBase.js',
        'Scripts/app/directives/directives.js',
        'Scripts/app/directives/indiClick.js',
        'Scripts/app/directives/layout.js',
        'Scripts/app/directives/PaneWorkflowDesigner/Messages.js',
        'Scripts/app/directives/PaneWorkflowDesigner/PaneWorkflowDesigner.js',
        'Scripts/app/directives/PaneConfigureAction/PaneConfigureAction.js',
        'Scripts/app/directives/PaneConfigureAction/ConfigurationControl.js',
        'Scripts/app/directives/PaneSelectAction/PaneSelectAction.js',
        'Scripts/app/directives/DesignerHeader/DesignerHeader.js',
        'Scripts/app/directives/Controls/FilePicker.js',
        'Scripts/app/directives/Controls/RadioButtonGroup.js',
        'Scripts/app/directives/Controls/DropDownListBox.js',
        'Scripts/app/directives/Controls/TextBlock.js',
        'Scripts/app/directives/Controls/TextArea.js',
        'Scripts/app/directives/Controls/FilterPane.js',
        'Scripts/app/directives/QueryBuilderWidget.js',
        'Scripts/app/directives/Controls/MappingPane.js',
        'Scripts/app/directives/Controls/ManageRoute.js',
        'Scripts/app/directives/Controls/RunRouteButton.js',
        'Scripts/app/directives/Controls/FieldList.js',
        'Scripts/app/directives/Controls/QueryBuilder.js',
        'Scripts/app/directives/Controls/TextSource.js',
        'Scripts/app/directives/Controls/InputFocus.js',
        'Scripts/app/directives/Controls/Counter.js',
        'Scripts/app/directives/Controls/Duration.js',
        'Scripts/app/directives/Controls/UpstreamDataChooser.js',
        'Scripts/app/directives/LongAjaxCursor.js',
        'Scripts/app/filters/ActionNameFormatter.js',
        'Scripts/app/controllers/RouteBuilderController.js',
        'Scripts/app/controllers/SandboxController.js',
        'Scripts/app/controllers/RouteFormController.js',
        'Scripts/app/controllers/RouteListController.js',
        'Scripts/app/controllers/ReportFactController.js',
        'Scripts/app/controllers/ReportIncidentController.js',
        'Scripts/app/controllers/RouteDetailsController.js',
        'Scripts/app/controllers/ManageFileListController.js',
        'Scripts/app/controllers/AccountListController.js',
        'Scripts/app/controllers/AccountDetailsController.js',
        'Scripts/app/controllers/InternalAuthenticationController.js',
        'Scripts/app/controllers/SelectActionController.js',
        'Scripts/app/controllers/ContainerListController.js',
        'Scripts/app/controllers/ContainerDetailsController.js',
        'Scripts/app/controllers/WebServiceListController.js',
        'Scripts/app/controllers/WebServiceFormController.js',
        'Scripts/app/controllers/TerminalListController.js',
        'Scripts/app/controllers/TerminalFormController.js',
        'Scripts/app/controllers/SolutionListController.js',
        'Scripts/app/controllers/NotifierController.js',
        'Scripts/app/controllers/RouteActionsDialogController.js',
        'Scripts/app/controllers/FindObjectsController.js',
        'Scripts/app/controllers/FindObjectsResultsController.js',
        'Scripts/app/controllers/PayloadFormController.js',
        'Scripts/app/controllers/TerminalListController.js',
        'Scripts/app/controllers/TerminalFormController.js'

    ])
        .pipe(sourcemaps.init())
        .pipe(concat('_compiled.js'))
        .pipe(sourcemaps.write())
        .pipe(gulp.dest('Scripts/app'));
});

gulp.task('watch_js', ['compile_js'], function () {
    gulp.watch('Scripts/app/**/*.js', ['compile_js']);
});

gulp.task('default', ['bower', 'concattemplates']);
