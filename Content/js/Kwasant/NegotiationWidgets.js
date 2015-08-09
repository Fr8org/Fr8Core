(function ($) {
   
    var that;
    var settings;
    var initValues;

    var nodes = {
        Name: null,
        QuestionHolder: null,

        Questions: []
    };

    $.fn.NegotiationWidget = function (options, initialValues) {
        
        that = this; 
        settings = $.extend({
            DisplayMode: 'edit',
            PrefixQuestionText: 'Question:',
            //Based on AnswerState.cs - this is overridable via the options
            AnswerProposedStatus: 2,
            AnswerSelectedStatus: 3,

            AllowModifyNegotiationRequest: true,

            AllowAddQuestion: true,
            AllowModifyQuestion: true,
            AllowDeleteQuestion: true,

            AllowAddAnswer: true,
            AllowModifyAnswer: true,
            AllowDeleteAnswer: true,
        }, options);

        initValues = $.extend({
            Id: null,
            Name: 'Negotiation 1'
        }, initialValues);

        buildBaseWidget();

        this.getValues = function () {
            var returnNeg = {};
            returnNeg.Id = initValues.Id,
            returnNeg.BookingRequestID = initValues.BookingRequestID,
            returnNeg.Name = nodes.Name.val();
            returnNeg.Questions = [];

            for (var q = 0; q < nodes.Questions.length; q++) {
                var question = nodes.Questions[q];

                returnNeg.Questions.push(question.getValues());
            }

            return returnNeg;
        };

        this.click();

        if (nodes.Questions.length > 0)
            setTimeout(function() { nodes.Questions[0].FocusMe(); }, 1000);
        
        return this;
    };

    function buildBaseWidget() {
        that.empty();

        that.addClass('negotiationsidebar');
        that.css('height', '100%');

        var baseInfoDiv = $('<div></div>')
            .addClass('form-group')
            .addClass('negotiation-mrbottom');

        var baseInfoTable = $('<table></table>')
            .css('width', '100%');

        /* Build the name input object */
        var nameInput = $('<input type="text" />')
            .addClass('form-control')
            .addClass('col-md-1')
            .val(initValues.Name);

        var nameRow = $('<tr />')
                .append($('<td />')
                    .append('&nbsp;<label>Name:</label>'))
                .append($('<td />')
                    .append(nameInput));

        baseInfoTable.append(nameRow);

        if (!settings.AllowModifyNegotiationRequest) {
            nameInput.attr('disabled', 'disabled');
        }

        baseInfoDiv.append(baseInfoTable);

        var questionHolder = $('<div></div>');

        nodes.QuestionHolder = questionHolder;

        var addQuestionSpan = $(' \
        <span class="form-group handIcon"> \
                &nbsp; &nbsp; \
            <img src="/Content/img/plus.png" /> \
            <label class="handIcon">Add Question</label> \
        </span>')
            .addClass('form-group')
            .addClass('handIcon')
            .click(function () {
                addQuestion();
            });

        if (!settings.AllowAddQuestion)
            addQuestionSpan.hide();

        that.append(baseInfoDiv);
        that.append(questionHolder);
        that.append(addQuestionSpan);

        nodes.Name = nameInput;
        if (initValues.Questions !== null && initValues.Questions !== undefined) {
            for (var i = 0; i < initValues.Questions.length; i++) {
                var questionValues = initValues.Questions[i];
                addQuestion(questionValues, true);
            }
        }

        if (settings.DisplayMode == 'reply') {
            nameRow.hide();
        }

    }

    function addQuestion(initialValues, immediate) {
        if (immediate === undefined)
            immediate = false;

        if (initialValues === undefined || initialValues === null)
            initialValues = {};

        var questionInitValues = $.extend({
            Id: 0,
            CalendarID: initialValues.CalendarID,
            QuestionGUID: guid(),
            Type: 'Text',
            Text: ''
        }, initialValues);

        var questionObject = createQuestionObject(questionInitValues);
        var questionNode = questionObject.Node;
        questionNode.hide();

        nodes.QuestionHolder.append(questionNode);

        if (immediate)
            questionNode.show();
        else {
            questionNode.slideDown(400, function() {
                questionObject.FocusMe();
            });
        }

        if (questionInitValues.Answers !== null && questionInitValues.Answers !== undefined) {
            for (var i = 0; i < questionInitValues.Answers.length; i++) {
                var answerValues = questionInitValues.Answers[i];
                
                if (!answerValues.EventID)
                    questionObject.addTextAnswer(answerValues, true);
                else
                    questionObject.addAnswer(answerValues, true);
            }
            //Now we create a new 'answer' that customers can use to specify their own answer..

            if (settings.DisplayMode == 'reply') {
                questionObject.addTextAnswer({
                    CanDelete: false
                }, true);
            }
        }

        return questionObject;
    }

    function createQuestionObject(questionInitValues) {
        var questionObject = {};

        questionObject.CalendarID = questionInitValues.CalendarID;

        var groupID = guid();

        var answerHolder = $('<div></div>');

        var questionTypeText = $('<input type="radio"/>')
            .attr('name', groupID)
            .attr('QuestionType', 'Text');

        var questionTypeCalendar = $('<input type="radio"/>')
            .attr('name', groupID)
            .attr('QuestionType', 'Timeslot');

        if (questionInitValues.AnswerType == 'Timeslot')
            questionTypeCalendar.get(0).checked = true;
        else
            questionTypeText.get(0).checked = true;        

        questionObject.OpenEventWindowSelection = function () {
            var _that = this;

            var launchCalendar = function (calID) {
                _that.CalendarID = calID;
                Kwasant.IFrame.Display('/Calendar/GetNegotiationCalendars?calendarID=' + calID + '&defaultEventDescription=' + 'Proposed for: ' + questionName.val(),
                    {
                        horizontalAlign: 'left',
                        callback: function (result) {
                            var filteredEvents = $.grep(result.events, function (elem) {
                                if (elem.tag[0] == calID)
                                    return true;
                                return false;
                            });

                            //Here we need to find out which answers to leave, which to delete, and which to add
                            var answersToAdd = [];
                            var touchedAnswers = [];
                            $.each(filteredEvents, function (i, event) {
                                var foundMatchingAnswer = false;
                                $.each(_that.Answers, function (j, answer) {
                                    if (foundMatchingAnswer)
                                        return;

                                    if (answer.EventID == event.id) {
                                        touchedAnswers.push(answer);
                                        foundMatchingAnswer = true;
                                    }
                                });

                                if (!foundMatchingAnswer) {
                                    answersToAdd.push({
                                        EventStart: event.start,
                                        EventEnd: event.end,
                                        EventID: event.id,
                                        Selected: true,
                                        Type: _that.Type
                                    });
                                }
                            });

                            //We need to copy the array, because it's being modified
                            var tmpAnswers = _that.Answers.slice(0);
                            $.each(tmpAnswers, function (j, answer) {
                                if ($.inArray(answer, touchedAnswers) == -1) {
                                    //Remove it!
                                    if (answer.EventID)
                                        answer.RemoveMe();
                                }
                            });
                            $.each(answersToAdd, function (k, newAnswer) {
                                _that.addAnswer(newAnswer);
                            });
                        }
                    });
            };

            if (this.CalendarID == null) {
                Kwasant.IFrame.DispatchUrlRequest('/Question/EditTimeslots?calendarID=null&negotiationID=' + initValues.Id, launchCalendar, 'POST');
            } else {
                launchCalendar(_that.CalendarID);
            }
        };


        var selectEventWindowsButton;
        if (settings.DisplayMode == 'reply') {
            selectEventWindowsButton = $('<span>')
                .append('Write in an alternative, or ')
                .append(
                    $('<a>')
                        .addClass('handIcon')
                        .append('choose from a calendar')
                        .click(function() { questionObject.OpenEventWindowSelection(); })
                );
        } else {
            selectEventWindowsButton = $('<a>')
                .addClass('handIcon')
                .append('Select Times')
                .click(function() { questionObject.OpenEventWindowSelection(); });
        }

        var radioButtons = [questionTypeText, questionTypeCalendar];


        var topWidget = $('<div>');
        var edittableType =
            $('<td />')
                .append(
                    $('<label>Type:</label>')
                )
                .append(
                    $('<label></label>')
                        .append(
                            questionTypeText
                        ).append("Text")
                ).append(
                    $('<label></label>')
                        .append(
                            questionTypeCalendar
                        ).append("Timeslot")
                );

        topWidget.append(edittableType);
        topWidget.append(selectEventWindowsButton);

        if (!settings.AllowModifyQuestion) {
            edittableType.hide();
        }

        var questionName = $('<input type="text" />')
            .addClass('form-control')
            .addClass('col-md-1')
            .addClass('QuesText')
            .attr('placeholder', 'Enter your question...')
            .val(questionInitValues.Text);

        var removeMeIcon = $('<img src="/Content/img/Cross.png"></img>')
            .addClass('handIcon')
            .click(function () {
                questionObject.RemoveMe();
            });

        var configureAnswerButton = function (isCalendar) {
            if (isCalendar) {
                if (settings.DisplayMode === 'review')
                    selectEventWindowsButton.hide();
                else {
                    selectEventWindowsButton.show();
                }

            } else {
                selectEventWindowsButton.hide();

                if (settings.AllowAddAnswer) {
                    addAnswerSpan.show();
                } else  {
                    addAnswerSpan.hide();
                }
            }
        };

        $.each(radioButtons, function (index, elem) {
            var closedFunc = function () {
                reconfigureAnswerButton();
            };
            elem.change(closedFunc);
        });

        var addAnswerSpan = $('<span>')
            .addClass('form-group')
            .addClass('handIcon')
            .click(function () {
                questionObject.addTextAnswer();
            })
            .append(
                $('<img src="/Content/img/plus.png" />')
            ).append(
                $('<label>Add answer</label>')
                .addClass('handIcon')
            );

        var reconfigureAnswerButton = function () {
            if (questionObject.getQuestionType() == 'Timeslot') {
                questionTypeCalendar.get(0).checked = true;
                configureAnswerButton(true);
            } else {
                questionTypeText.get(0).checked = true;
                configureAnswerButton(false);
            }
        };

        if (!settings.AllowDeleteQuestion && questionInitValues.Id > 0)
            removeMeIcon.hide();

        if (!settings.AllowAddAnswer && questionInitValues.Id > 0)
            addAnswerSpan.hide();

        if (!settings.AllowModifyQuestion)
            questionName.attr('disabled', 'disabled');

        var questionDiv = $('<div></div>')
            .addClass('questionBox')
            .append(
                $('<table />')
                    .css('width', '100%')
                    .append(
                        $('<tr />')
                            .append(
                                $('<td />')
                                    .append(
                                         $('<label>' + settings.PrefixQuestionText + ' </label>')
                                    )
                            ).append(
                                $('<td />')
                                    .css('width', '100%')
                                    .append(
                                        questionName
                                    )
                            ).append(
                                $('<td />')
                                    .append(
                                        removeMeIcon
                                    )
                            )
                    )
                    .append(
                        $('<tr />')
                            .append(
                                $('<td />')
                            ).append(
                                topWidget
                            )
                    )
            )
            .append(
                answerHolder
            )
            .append(
                addAnswerSpan
            );

        questionObject.Node = questionDiv;
        questionObject.Answers = [];

        questionObject.getQuestionType = function () {
            for (var i = 0; i < radioButtons.length; i++) {
                var button = radioButtons[i];
                if (button.get(0).checked) {
                    return button.attr('QuestionType');
                }
            }
            return 'Text';
        };

        questionObject.getValues = function () {
            var answers = [];
            for (var i = 0; i < this.Answers.length; i++) {
                var answer = this.Answers[i];
                answers.push(answer.getValues());
            }
            return {
                Id: questionInitValues.Id,
                Text: questionName.val(),
                CalendarID: this.CalendarID,
                Answers: answers,
                AnswerType: this.getQuestionType(),
            };
        };

        questionObject.AnswerHolder = answerHolder;
        questionObject.Id = questionInitValues.Id;

        var adjustRadioButtonEnabled = function () {
            if (questionObject.Answers.length == 0) {
                questionTypeText.removeAttr('disabled');
                questionTypeCalendar.removeAttr('disabled');
            } else {
                questionTypeText.attr('disabled', 'disabled');
                questionTypeCalendar.attr('disabled', 'disabled');
            }
        };

        questionObject.addTextAnswer = function (initialValues, immediate) {
            if (!initialValues)
                initialValues = {};
            initialValues.ForceTextAnswer = true;
            this.addAnswer(initialValues, immediate);
        };

        questionObject.FocusMe = function() {
            questionName.focus();
        };

        questionObject.addAnswer = function (initialValues, immediate) {
            if (immediate === undefined)
                immediate = false;

            if (initialValues === null || initialValues === undefined)
                initialValues = {};

            var answerInitValues = $.extend({
                Id: 0,
                VotedBy: [],
                CanDelete: !(initialValues.DisableManualEdit || !settings.AllowDeleteAnswer && initialValues.Id > 0),
                StartDate: initialValues.StartDate,
                EndDate: initialValues.EndDate,
                AnswerState: settings.AnswerProposedStatus,
                Selected: this.Answers.length == 0 ? true : false,
                QuestionGUID: questionInitValues.QuestionGUID,
                //PromptText: 'Enter an alternative suggestion here...',
                PromptText: settings.DisplayMode == 'reply' ? "Enter an alternative suggestion here..." : "",
                Text: ''
            }, initialValues);

            var answerObject;
            if (!initialValues.ForceTextAnswer && this.getQuestionType() == 'Timeslot') {
                answerObject = createCalendarAnswerObject(this, answerInitValues);
            } else {
                answerObject = createTextAnswerObject(this, answerInitValues);
            }

            this.Answers.push(answerObject);

            var answerNode = answerObject.Node;
            answerNode.hide();

            this.AnswerHolder.append(answerNode);
            if (immediate)
                answerNode.show();
            else {
                answerNode.slideDown(400, function() {
                    answerObject.FocusMe();
                });
            }

            adjustRadioButtonEnabled();

            reconfigureAnswerButton();

            return answerObject;
        };

        questionObject.UnselectOtherAnswers = function (exceptObject) {
            for (var i = 0; i < this.Answers.length; i++) {
                var currAnswer = this.Answers[i];
                if (currAnswer !== exceptObject) {
                    currAnswer.unmarkMeSelected();
                }
            }
        };

        questionObject.removeAnswer = function (answerObject) {
            this.Answers.splice(this.Answers.indexOf(answerObject), 1);
            answerObject.Node.slideUp();

            reconfigureAnswerButton();

            adjustRadioButtonEnabled();
        };

        questionObject.RemoveMe = function () {
            nodes.Questions.splice(nodes.Questions.indexOf(questionObject), 1);
            questionDiv.slideUp();
        };


        //Check radio buttons based on original settings
        for (var i = 0; i < radioButtons.length; i++) {
            var button = radioButtons[i];
            if (button.attr('QuestionType') == questionInitValues.Type) {
                button.get(0).checked = true;
            }
        }

        if (questionObject.CalendarID == null)
            configureAnswerButton(false);
        else
            configureAnswerButton(true);

        nodes.Questions.push(questionObject);

        return questionObject;
    }

    function createTextAnswerObject(question, answerInitValues) {
        var answerObject = {};
        answerObject.AnswerState = answerInitValues.AnswerState;

        var radioSelect = $('<input type="radio"/>')
            .attr('name', answerInitValues.QuestionGUID);

        if (answerInitValues.Selected)
            radioSelect.click();

        if (settings.DisplayMode != 'reply')
            radioSelect.hide();

        var suggestedByText = 'Suggested by ';
        if (answerInitValues.SuggestedBy === '') {
            suggestedByText += ' the booker';
        } else {
            suggestedByText += answerInitValues.SuggestedBy;
        }

        var answerText = $('<input />')
            .addClass('form-control')
            .addClass('col-md-1')
            .attr('title', suggestedByText)
            .val(answerInitValues.Text);

        if (settings.DisplayMode != 'review')
            answerText.attr('placeholder', answerInitValues.PromptText);

        answerText.click(function() {
            radioSelect.click();
        });

        var suggestedBy = $('<span />')
            .css('float', 'right')
            .css('padding-top', '6px')
            .css('padding-right', '16px')
            .css('color', 'grey')
            .text('[' + suggestedByText + ']');
        suggestedBy.hide();

        var canEditAnswer =
            !answerInitValues.DisableManualEdit && (
                settings.AllowModifyAnswer ||
                answerInitValues.Id == 0
            );

        if (!canEditAnswer) {
            answerText.attr('disabled', 'disabled');
        }

        var deleteButton = $('<img src="/Content/img/Cross.png" />')
            .addClass('handIcon')
            .click(function () {
                answerObject.RemoveMe();
            });

        var peopleWhoVoted = 'No one voted for this answer.';
        if (answerInitValues.VotedByList === undefined)
            answerInitValues.VotedByList = [];
        
        for (var i = 0; i < answerInitValues.VotedByList.length; i++) {
            if (i > 0)
                peopleWhoVoted += ', ';
            else
                peopleWhoVoted = 'The following people voted for this answer: ';

            peopleWhoVoted += answerInitValues.VotedByList[i];
        }

        var votesIcon = $('<label>')
            .append(answerInitValues.VotedByList.length);

        answerObject.markMeSelected = function () {
            answerDiv
                .css('background-color', 'rgb(183, 228, 195)');
            question.UnselectOtherAnswers(answerObject);

            answerObject.AnswerState = settings.AnswerSelectedStatus;

            btnMarkProposed
                .val('Unmark as selected')
                .css('background-color', '#E01E26')
                .unbind('click')
                .click(function () {
                    answerObject.unmarkMeSelected();
                });
        };

        answerObject.unmarkMeSelected = function () {
            answerDiv
                .css('background-color', '');

            answerObject.AnswerState = settings.AnswerProposedStatus;

            btnMarkProposed
                .val('Mark as selected')
                .css('background-color', '#3cc05e')
                .unbind('click')
                .click(function () {
                    answerObject.markMeSelected();
                });
        };

        var btnMarkProposed = $('<input type="button"/>')
            .val('Mark as selected')
            .addClass('btn')
            .addClass('handIcon')
            .css('background-color', '#3cc05e')
            .css('border-width', '0')
            .css('margin', '5px')
            .click(function () {
                answerObject.markMeSelected();
            });

        if (settings.DisplayMode != 'review') {
            votesIcon.hide();
            btnMarkProposed.hide();
        }

        if (!answerInitValues.CanDelete)
            deleteButton.hide();
        
        if (answerInitValues.CanDelete || !canEditAnswer) {
            if (suggestedByText == 'Suggested by undefined')
                suggestedBy.hide();
            else
                suggestedBy.show();
        }


        var answerDiv =
            $('<div />')
                .addClass('answerBox')
                .append(
                    $('<table />')
                        .css('width', '100%')
                        .append(
                            $('<tr />')
                                .append(
                                    $('<td />')
                                        .css('width', settings.DisplayMode == 'reply' ? '20px' : '0px')
                                        .css('padding-left', settings.DisplayMode == 'reply' ? '1px' : '0px')
                                        .css('padding-right', settings.DisplayMode == 'reply' ? '1px' : '0px')
                                        .append(
                                            radioSelect
                                        )
                                ).append(
                                    $('<td />')
                                        .append(
                                            answerText
                                        )
                                ).append(
                                    $('<td />')
                                        .css('padding-left', answerInitValues.CanDelete || !canEditAnswer ? '1px' : '0px')
                                        .css('padding-right', answerInitValues.CanDelete || !canEditAnswer ? '1px' : '0px')
                                        .append(
                                            suggestedBy
                                        )
                                ).append(
                                    $('<td />')
                                        .css('width', answerInitValues.CanDelete ? '34px' : '0px')
                                        .css('padding-left', answerInitValues.CanDelete ? '1px' : '0px')
                                        .css('padding-right', answerInitValues.CanDelete ? '1px' : '0px')
                                        .append(
                                            deleteButton
                                        )
                                ).append(
                                    $('<td />')
                                        .css('width', settings.DisplayMode == 'review' ? '25px' : '0px')
                                        .css('padding-left', settings.DisplayMode == 'review' ? '1px' : '0px')
                                        .css('padding-right', settings.DisplayMode == 'review' ? '1px' : '0px')
                                        .append(
                                            votesIcon
                                        )
                                )
                        )
                ).append(
                    $('<div />')
                        .append(btnMarkProposed)
                );


        if (settings.DisplayMode == 'review')
            answerDiv.attr('title', peopleWhoVoted);

        answerObject.Id = answerInitValues.Id;
        answerObject.Question = question;
        answerObject.getValues = function () {
            return {
                Id: answerObject.Id,
                AnswerState: answerObject.AnswerState,
                Text: answerText.val(),
                Selected: radioSelect.get(0).checked
            };
        };
        answerObject.FocusMe = function() {
            answerText.focus();
        };

        answerObject.RemoveMe = function () {
            this.Question.removeAnswer(answerObject);
        };

        if (answerObject.AnswerState == settings.AnswerSelectedStatus)
            answerObject.markMeSelected();

        answerObject.Node = answerDiv;
        return answerObject;
    }

    function createCalendarAnswerObject(question, answerInitValues) {

        var padMins = function (mins) {
            if (mins < 10)
                return mins + '0';
            return mins;
        };
        var padHours = function (hours) {
            if (hours < 10)
                return '0' + hours;
            return hours;
        };

        var startDateString;
        if (answerInitValues.EventStart.toDateString === undefined) {
            startDateString = answerInitValues.EventStart.d.toDateString();
        } else {
            startDateString = answerInitValues.EventStart.toDateString();
        }

        var startDateTime = padHours(answerInitValues.EventStart.getHours()) + ':' + padMins(answerInitValues.EventStart.getMinutes());
        var endDateTime = padHours(answerInitValues.EventEnd.getHours()) + ':' + padMins(answerInitValues.EventEnd.getMinutes());
        var eventStr = startDateString + ' ' + startDateTime + ' - ' + endDateTime;

        answerInitValues.Text = eventStr;
        answerInitValues.DisableManualEdit = true;

        var answerObject = createTextAnswerObject(question, answerInitValues);
        answerObject.EventID = answerInitValues.EventID;
        answerObject.EventStart = answerInitValues.EventStart;
        answerObject.EventEnd = answerInitValues.EventEnd;

        answerObject.baseGetValues = answerObject.getValues;
        answerObject.getValues = function () {
            var baseReturn = this.baseGetValues();
            baseReturn.EventID = this.EventID;

            return baseReturn;
        };

        answerObject.baseRemoveMe = answerObject.RemoveMe;
        answerObject.RemoveMe = function () {
            var outterThis = this;
            var eventID = answerObject.EventID;
            var spinner = Kwasant.IFrame.DisplaySpinner();
            $.post(
                '/Event/ConfirmDelete',
                { eventID: eventID }
            ).success(function() {
                outterThis.baseRemoveMe();
            }).fail(function() {
                alert('Server failed to delete event.');
            }).always(function() {
                if (spinner !== null)
                    spinner.hide();
            });
        };

        return answerObject;
    }

}(jQuery));