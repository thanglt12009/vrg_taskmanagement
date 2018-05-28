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
var s, WorkTasks = {
    setting: {
        //defined element here
        form: $("#formCreateTask"),
        modal: $("#modal-container-createmodal"),
        fud: $("#file-uploader"),
        extraButton: $("#create-extrabutton"),
        modalAttach: $("#create-editAttachment"),
        cancelUpload: $("#create-cancel-upload"),
        attachmentForm: $("#create-attachmentForm"),
        assignee: $("#Assignee"),
        //reporter: $("#Reporter"),
        director: $("#Director"),
        councilmember: $("#CouncilMember"),
        validationMess: $("#validation-message"),
        endDate: $("#endDate"),
        startDate: $("#startDate"),
        edittovrgdate: $('#tovrgdate'),
        edittodepartmentdate: $('#todepartmentdate'),
        flag: false,
        widget_assignee: '',
        widget_director: '',
        widget_councilmember: '',
    },
    init: function () {
        //kick thing off
        s = this.setting;
        //this.initRules();
        this.initDatepicker();
        this.initAutocomplete();
        this.initEvent();

    },
    initDatepicker: function () {
        s.startDate.datepicker({
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
            s.endDate.datepicker("setStartDate", $(this).val());
            //$('.datepicker').hide();
        }).removeAttr("data-val-date");

        s.endDate.datepicker({
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
            s.startDate.datepicker("setEndDate", $(this).val());
        }).removeAttr("data-val-date");
        s.edittovrgdate.datepicker({
            dateFormat: 'dd/mm/yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            keyboardNavigation: false,
            forceParse: false,
            calendarWeeks: true,
            language: 'vi',
        }).removeAttr("data-val-date");
        s.edittodepartmentdate.datepicker({
            dateFormat: 'dd/mm/yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            keyboardNavigation: false,
            forceParse: false,
            calendarWeeks: true,
            language: 'vi',
        }).removeAttr("data-val-date");


    },
    initRules: function () {
        //init rules
        s.assignee.val("");
        $.validator.unobtrusive.adapters.add(
            'validationextension', ['propertytested', 'allowequaldates'], function (options) {
                options.rules['validationextension'] = options.params;
                options.messages['validationextension'] = options.message;
            });
        //
        $.validator.addMethod("validationextension", function (value, element, params) {
            s.flag = WorkTasks.compareDate(true, value, element, params);
            return s.flag;
        });
       
    },
    initAutocomplete: function () {
        // for assignee
        $.ajax({
            type: 'GET',
            url: '/Worktask/GetAccount?role=0',
            success: function (data) {
                var onChange = function (newValue, oldValue) {
                    s.validationMess.hide();
                    if (newValue.length > 1) {
                        //remove first element of array
                        newValue.shift();
                        return newValue;
                    }

                };
                var Assignee = [];
                for (var i = 0; i < data.source.length; i++) {
                    Assignee.push({ value: data.source[i] });
                }
                var config = {
                    onChange: onChange,
                    lists: {
                        Assignee: {
                            optionHTML: '{name}',
                            options: Assignee,
                            noResultsHTML: "Không có Người thực hiện phù hợp."
                        }
                    }
                };
                s.widget_assignee = new AutoComplete('search_bar_assignee', config);
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
                var onChange = function (newValue, oldValue) {
                    s.validationMess.hide();
                    if (newValue.length > 1) {
                        //remove first element of array
                        newValue.shift();
                        return newValue;
                    }

                };
                var Assignee = [];
                for (var i = 0; i < data.source.length; i++) {
                    Assignee.push({ value: data.source[i] });
                }
                var config = {
                    onChange: onChange,
                    lists: {
                        Assignee: {
                            optionHTML: '{name}',
                            options: Assignee,
                            noResultsHTML: "Không có PTGĐ/TGĐ phù hợp."
                        }
                    }
                };
                s.widget_director = new AutoComplete('search_bar_director', config);
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
                var onChange = function (newValue, oldValue) {
                    s.validationMess.hide();
                    if (newValue.length > 1) {
                        //remove first element of array
                        newValue.shift();
                        return newValue;
                    }

                };
                var Assignee = [];
                for (var i = 0; i < data.source.length; i++) {
                    Assignee.push({ value: data.source[i] });
                }
                var config = {
                    onChange: onChange,
                    lists: {
                        Assignee: {
                            optionHTML: '{name}',
                            options: Assignee,
                            noResultsHTML: "Không có HĐTV phù hợp."
                        }
                    }
                };
                s.widget_councilmember = new AutoComplete('search_bar_councilmember', config);
            },
            error: function (msg) {
                alert("error");
            }
        });

        var createTaskListOptions = function (data) {

            var options = [];
            for (var i = 0; i < data.source.length; i++) {
                options.push({ value: data.source[i] });
            }

            return options;
        };
        var wtid = $('#WorktaskID').val();
        if ((typeof wtid) == 'undefined') wtid = 0;
        var config = {
            lists: {
                tasks: {
                    ajaxOpts: {
                        preProcess: createTaskListOptions,
                        url: '/Worktask/GetTaskList?boardcode=' + taskmgr_current_board_code + '&taskid=' + wtid + '&prompt={input}'
                    },
                    optionHTML: '{name}',
                    ajaxLoadingHTML: "Đang tải...",
                    noResultsHTML: "Không có Công việc liên quan phù hợp."
                }
            }
        };
        s.relatedtaskwidget = new AutoComplete('search_bar_related_tasks', config);
    },
    initEvent: function () {
        var formSubmit = {
            AssigneeName: "",
            Assignee: "",
            Director: "",
            CouncilMember: "",
            Description: "",
            PlannedEndDate: "",
            PlannedStartDate: "",
            TaskType: "",
            Priority: "",
            Title: "",
            MetaInfoDocumentNo: "",
            MetaInfoToDepartmentDateTime: "",
            MetaInfoToVRGDateTime: "",
            MetaInfoCompany: "",
            __RequestVerificationToken: ""

        };
        formSubmit.Attachment = [];
        s.form.submit(function (event) {
            //stop submit normally
            event.preventDefault();
            //get attribute of form
            var $form = $(this);
            // change value for assignee
            if (s.widget_assignee.getValue() == "") {
                s.assignee.val("");
            }
            else {
                s.assignee.val(0);
            }
            // change value for director
            if (s.widget_director.getValue() == "") {
                s.director.val("");
            }
            else {
                s.director.val(0);
            }
            // change value for councilmember
            if (s.widget_councilmember.getValue() == "") {
                s.councilmember.val("");
            }
            else {
                s.councilmember.val(0);
            }
            if (!$form.valid()) {
                s.validationMess.show();
                return false
            };
            var url = $form.attr('action');
            var worktask = $form.serializeObject();
            //binding data
            formSubmit.Description = worktask.Description;
            formSubmit.Priority = worktask.Priority;
            formSubmit.TaskType = worktask.TaskType;
            formSubmit.PlannedEndDate = worktask.PlannedEndDate;
            formSubmit.PlannedStartDate = worktask.PlannedStartDate;
            formSubmit.Title = worktask.Title;
            if (s.widget_assignee.getValue().length > 0) {
                formSubmit.Assignee = s.widget_assignee.getValue()[0][0].value.id;
            }
            if (s.widget_director.getValue().length > 0) {
                formSubmit.Director = s.widget_director.getValue()[0][0].value.id;
            }
            if (s.widget_councilmember.getValue().length > 0) {
                formSubmit.CouncilMember = s.widget_councilmember.getValue()[0][0].value.id;
            }
            //formSubmit.Priority = worktask.Priority;

            var widget = s.relatedtaskwidget;
            var selectedtasks = widget.getValue();
            var selectedtaskcodes = [];
            for (var i = 0; i < selectedtasks.length; i++) {
                selectedtaskcodes.push(selectedtasks[i][0].value.code);
            }
            formSubmit.RelatedTaskValue = selectedtaskcodes;
            formSubmit.MetaInfoDocumentNo = worktask.MetaInfoDocumentNo;
            formSubmit.MetaInfoToDepartmentDateTime = worktask.MetaInfoToDepartmentDateTime;
            formSubmit.MetaInfoToVRGDateTime = worktask.MetaInfoToVRGDateTime;
            formSubmit.MetaInfoCompany = worktask.MetaInfoCompany;
            
            formSubmit.__RequestVerificationToken = worktask.__RequestVerificationToken;
            //ajax submit
            $.ajax({
                url: url,
                type: 'POST',
                data: formSubmit,
                success: function (response) {
                    if (response.success) {
                        localStorage.setItem("temp", "");
                        window.location.href = response.redirectUrl;
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
        var extraObj = s.fud.uploadFile({
            url: "/Worktask/SaveUploadedFile",
            multiple: true,
            dragDrop: true,
            showDelete: true,
            fileName: "myfile",
            onSelect: function (files) {
                s.modalAttach.modal();
            },
            deleteCallback: function (data, pd) {
                //
                var ls = formSubmit.Attachment.length;
                for (var i = 0; i < ls; i++) {
                    if (formSubmit.Attachment[i].guid === data.guid) {
                        formSubmit.Attachment.splice(i, 1);
                        localStorage.setItem("temp", JSON.stringify(formSubmit.Attachment));
                    }
                }
                $.ajax({
                    url: '/Worktask/DeleteUploaderFile?path=' + data.Message + '&id=0',
                    type: 'POST',
                    success: function (response) {
                        if (response.success) {
                            toastr["success"](response.message);
                            pd.statusbar.hide();
                        }
                    }
                })
            },
            onSuccess: function (files, data, xhr, pd) {
                if (data.success) {
                    var obj = s.attachmentForm.serializeObject();
                    //parse format
                    var f = obj.Date.split("/");
                    obj.Date = new Date(f[2], f[1] - 1, f[0]).toISOString();
                    obj.guid = data.guid;
                    obj.StoredPath = data.Message;
                    obj.Name = data.Name;
                    
                    formSubmit.Attachment.push(obj);
                    localStorage.setItem("temp", JSON.stringify(formSubmit.Attachment));
                }
            },
            autoSubmit: false
        });
        s.attachmentForm.submit(function (event) {
            console.log("create");
            //stop submit normally
            event.preventDefault();
            //get attribute of form
            var $form = $(this);
            if (!$form.valid()) return false;
            s.modalAttach.modal('hide');
            extraObj.startUpload();
        });
        s.cancelUpload.click(function () {
            extraObj.stopUpload();
        })

    },

    compareDate: function (isEndDate, value, element, params) {
        var f1, f2;
        var parts = element.name.split(".");
        var prefix = "";
        if (parts.length > 1)
            prefix = parts[0] + ".";
        var startdatevalue = $('input[name="' + prefix + params.propertytested + '"]').val();
        if (!value || !startdatevalue) {
            return true;
        }
        if (isEndDate) {
            f1 = startdatevalue.split("/");
            f2 = value.split("/");
        }
        else {
            f2 = startdatevalue.split("/");
            f1 = value.split("/");
        }
        var result = (params.allowequaldates) ? new Date(f1[2], f1[1] - 1, f1[0]) <= new Date(f2[2], f2[1] - 1, f2[0]) : new Date(f1[2], f1[1] - 1, f1[0]) < new Date(f2[2], f2[1] - 1, f2[0]);
        return result;
    }
}