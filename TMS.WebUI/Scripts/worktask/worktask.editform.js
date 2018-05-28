(function ($) {
    $.fn.serializeObject = function () {
        var o = {};
        var a = this.serializeArray();
        $.each(a, function () {
            if (o[this.name] !== undefined) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    }
})(jQuery);

var TaskList = [];
var e, EditWorkTasks = {
    setting: {
        form: $("#formEditTask"),
        messageDisplay: $("#edit-message"),
        assignee: $("#EditAssignee"),
        assigneeName: $("#AssigneeName"),
        //reporter: $("#EditReporter"),
        //reporterName: $("#ReporterName"),
        director: $("#EditDirector"),
        directorName: $("#DirectorName"),
        councilmember: $("#EditCouncilMember"),
        councilmemberName: $("#CouncilMemberName"),
        editStartDate: $("#editStartDate"),
        editEndDate: $("#editEndDate"),
        edittovrgdate: $('#tovrgdate'),
        edittodepartmentdate: $('#todepartmentdate'),
        _widget_assignee: '',
        //_widget_reporter: '',
        _widget_director: '',
        _widget_councilmember: ''
    },
    init: function () {
        //kick thing off
        e = this.setting;
        this.initDatepicker();
        this.initEvent();
        this.initAutoComplete();
    },
    initDatepicker: function () {
        e.editStartDate.datepicker({
            dateFormat: 'dd/mm/yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            keyboardNavigation: false,
            forceParse: false,
            calendarWeeks: true,
            language: 'vi',
        })
        .on('change', function () {
            $(this).valid();
            e.editEndDate.datepicker("setStartDate", $(this).val());
            //$('.datepicker').hide();
            var isValid = EditWorkTasks.CompareDate(e.editStartDate.val(), e.editEndDate.val());
            if(isValid){
                e.editEndDate.valid();
            }
        }).removeAttr("data-val-date");

        e.editEndDate.datepicker({
            dateFormat: 'dd/mm/yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            keyboardNavigation: false,
            forceParse: false,
            calendarWeeks: true,
            language: 'vi',
        })
        .on('change', function () {
            $(this).valid();
            //$('.datepicker').hide();
            e.editStartDate.datepicker("setEndDate", $(this).val());
        }).removeAttr("data-val-date");
        e.edittovrgdate.datepicker({
            dateFormat: 'dd/mm/yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            keyboardNavigation: false,
            forceParse: false,
            calendarWeeks: true,
            language: 'vi',
        }).removeAttr("data-val-date");
        e.edittodepartmentdate.datepicker({
            dateFormat: 'dd/mm/yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            keyboardNavigation: false,
            forceParse: false,
            calendarWeeks: true,
            language: 'vi',
        }).removeAttr("data-val-date");
    },
    initAutoComplete: function () {
        e.messageDisplay.show();
        // for assignee
        $.ajax({
            type: 'GET',
            url: '/Worktask/GetAccount?role=0',
            success: function (data) {
                var initAssignee;

                var onChange = function (newValue, oldValue) {
                    e.messageDisplay.hide();
                    if (newValue.length > 1) {
                        //remove first element of array
                        newValue.shift();
                        return newValue;
                    }
                };
                var accounts = [];
                for (var i = 0; i < data.source.length; i++)
                {
                    accounts.push({ value: data.source[i] });
                    if (data.source[i].id == parseInt(e.assignee.val())) {
                        initAssignee = i;
                    }
                }
                var config = {
                    onChange: onChange,
                    //initialValue: InitAssigneeArr,
                    lists: {
                        accounts: {
                            optionHTML: '{name}',
                            options: accounts,
                            noResultsHTML: "Không có Người thực hiện phù hợp."
                        }
                    }
                };
                e._widget_assignee = new AutoComplete('search_bar_assignee_edit', config);
                e._widget_assignee.setValue([accounts[initAssignee]]);
            },
            error: function (msg) {
                alert("error");
            }
        });
        // for director
        $.ajax({
            type: 'GET',
            url: '/Worktask/GetAccount?role=4',
            success: function (data) {
                var initReporter;
                var onChange = function (newValue, oldValue) {
                    e.messageDisplay.hide();
                    if (newValue.length > 1) {
                        //remove first element of array
                        newValue.shift();
                        return newValue;
                    }
                };
                var accounts = [];
                for (var i = 0; i < data.source.length; i++) {
                    accounts.push({ value: data.source[i] });
                    if (data.source[i].id == parseInt(e.director.val())) {
                        initReporter = i;
                    }
                }
                var config = {
                    onChange: onChange,
                    //initialValue: InitAssigneeArr,
                    lists: {
                        accounts: {
                            optionHTML: '{name}',
                            options: accounts,
                            noResultsHTML: "Không có PTGĐ/TGĐ phù hợp."
                        }
                    }
                };
                e._widget_director = new AutoComplete('search_bar_director_edit', config);
                e._widget_director.setValue([accounts[initReporter]]);
            },
            error: function (msg) {
                alert("error");
            }
        });
        // for councilmember
        $.ajax({
            type: 'GET',
            url: '/Worktask/GetAccount?role=5',
            success: function (data) {
                var initReporter;
                var onChange = function (newValue, oldValue) {
                    e.messageDisplay.hide();
                    if (newValue.length > 1) {
                        //remove first element of array
                        newValue.shift();
                        return newValue;
                    }
                };
                var accounts = [];
                for (var i = 0; i < data.source.length; i++) {
                    accounts.push({ value: data.source[i] });
                    if (data.source[i].id == parseInt(e.councilmember.val())) {
                        initReporter = i;
                    }
                }
                var config = {
                    onChange: onChange,
                    //initialValue: InitAssigneeArr,
                    lists: {
                        accounts: {
                            optionHTML: '{name}',
                            options: accounts,
                            noResultsHTML: "Không có HĐTV phù hợp."
                        }
                    }
                };
                e._widget_councilmember = new AutoComplete('search_bar_councilmember_edit', config);
                e._widget_councilmember.setValue([accounts[initReporter]]);
            },
            error: function (msg) {
                alert("error");
            }
        });

        $.ajax({
            type: 'GET',
            url: '/Worktask/GetTaskList?boardcode=' + taskmgr_current_board_code + '&taskid=' + $('#WorktaskID').val() + '&prompt=',
            success: function (data) {
                for (var i = 0; i < data.source.length; i++) {
                    TaskList.push({ value: data.source[i] });
                }
                var selectedtaskcodes = $('#relatedtasklistedit').attr('data-tag').split(",");
                var SelectedTasks = [];
                for (var i = 0; i < selectedtaskcodes.length; i++) {
                    if (selectedtaskcodes[i].length > 0) {
                        for (var j = 0; j < TaskList.length; j++) {
                            if (TaskList[j].value.code == selectedtaskcodes[i]) {
                                SelectedTasks.push([{ value: TaskList[j].value, tokenHTML: TaskList[j].value.name }]);
                            }
                        }
                    }
                }
                var createTaskListOptions = function (data) {

                    var options = [];
                    for (var i = 0; i < data.source.length; i++) {
                        options.push({ value: data.source[i] });
                    }

                    return options;
                };

                var config = {
                    initialValue: SelectedTasks,
                    lists: {
                        tasks: {
                            ajaxOpts: {
                                preProcess: createTaskListOptions,
                                url: '/Worktask/GetTaskList?boardcode=' + taskmgr_current_board_code + '&taskid=' + $('#WorktaskID').val() + '&prompt={input}'
                            },
                            optionHTML: '{name}',
                            ajaxLoadingHTML: "Đang tải...",
                            noResultsHTML: "Không có Công việc liên quan phù hợp."
                        }
                    }
                };
                e.relatedtaskwidget = new AutoComplete('search_bar_related_tasks_edit', config);
            },
            error: function (msg) {
                alert("error");
            }
        });
    },
    initEvent: function () {
        e.form.submit(function (event) {
            //stop submit normally
            event.preventDefault();
            //get attribute of form
            var $form = $(this);
            if (e._widget_assignee.getValue() == "") {
                e.assignee.val("");
            }
            else {
                e.assignee.val(0);
            }
            if (e._widget_director.getValue() == "") {
                e.director.val("");
            }
            else {
                e.director.val(0);
            }
            if (e._widget_councilmember.getValue() == "") {
                e.councilmember.val("");
            }
            else {
                e.councilmember.val(0);
            }
            if (!$form.valid()) {
                e.messageDisplay.show();
                return false;
            }
            var url = $form.attr('action');
            var worktask = $form.serializeObject();
            if (e._widget_assignee.getValue().length > 0) {
                worktask.AssigneeName = e._widget_assignee.getValue()[0][0].value.name;
                worktask.Assignee = e._widget_assignee.getValue()[0][0].value.id;
            }
            if (e._widget_director.getValue().length > 0) {
                worktask.DirectorName = e._widget_director.getValue()[0][0].value.name;
                worktask.Director = e._widget_director.getValue()[0][0].value.id;
            }
            if (e._widget_councilmember.getValue().length > 0) {
                worktask.CouncilMemberName = e._widget_councilmember.getValue()[0][0].value.name;
                worktask.CouncilMember = e._widget_councilmember.getValue()[0][0].value.id;
            }

            var selectedtasks = e.relatedtaskwidget.getValue();
            var selectedtaskcodes = [];
            for (var i = 0; i < selectedtasks.length; i++) {
                selectedtaskcodes.push(selectedtasks[i][0].value.code);
            }
            worktask.RelatedTaskValue = selectedtaskcodes;
            $.ajax({
                url: url,
                type: 'POST',
                data: worktask,
                success: function (response) {
                    if (response.success) {
                        localStorage.setItem("Status", response.message);
                        location.reload();
                    } else {
                        toastr["error"](response.message);
                        if (typeof response.redirectUrl != 'undefined') {
                            window.location.href = response.redirectUrl;
                        }
                    }
                },
                error: function (response) {
                    //Display new Toast Error
                    toastr["error"](response.message);
                }
            });
        })
    },
    CompareDate: function (startDate, endDate) {
       var f1 = startDate.split("/");
       var f2 = endDate.split("/");
       var result = new Date(f1[2], f1[1] - 1, f1[0]) <= new Date(f2[2], f2[1] - 1, f2[0]);
       return result;
    }
}