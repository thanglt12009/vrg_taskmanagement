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
var d, DetailWorkTasks = {
    setting: {
        comment: $("#sendComment"),
        taskId: $("#WorktaskID"),
        commentContent: $("#CommentContent"),
        listComment: $("#listComment"),
        currentUser : $("#currentUser"),
        fileUploader: $("#fileuploader"),
        timeComment: $("#timeComment"),
        dateComment: $("#dateComment"),
        modalAttach: $("#detail-editAttachment"),
        cancelUpload: $("#detail-cancel-upload"),
        extraButton: $("#detail-extrabutton"),
        attachmentForm: $("#detail-attachmentForm"),
        editForm : $("#formEditTask"),
        flag : [],
    },
    init: function (modelAttachment) {
        //kick thing off
        d = this.setting;
        d.flag = modelAttachment;
        this.initNotify();
        this.initEvent();
    },
    initNotify : function(){
        //get it if Status key found
        if (localStorage.getItem("Status")) {
            toastr["success"](localStorage.getItem("Status"));
            localStorage.clear();
        }
    },

    initEvent: function () {

        var attachObj = d.fileUploader.uploadFile({
            url: "/Worktask/SaveUploadedFile",
            multiple: true,
            dragDrop: true,
            showDelete: true,
            fileName: "myfile",
            onSelect: function (files) {
                for (var i = 0; i < d.flag.length; i++){
                    if(d.flag[i].Name === files[0].name){
                        toastr["error"]("Đã tồn tại file này!");
                        return false;
                    }
                }
                d.modalAttach.modal();
            },
            deleteCallback: function (data, pd) {
                var removeId;
                var fName = data.Message.Name;
                for (var i = 0; i < d.flag.length; i++) {
                    if (d.flag[i].Name === fName) {
                        removeId = d.flag[i].AttachmentID;
                        d.flag.splice(i, 1);
                    }
                }
                
                $.ajax({
                    type: "POST",
                    url: "/Worktask/DeleteUploaderFile?path=" + data.Message + "&id=" + removeId + "&taskId=" + d.taskId.val(),
                    success: function (response) {
                        if (response.success) {
                            toastr["success"](response.message);
                            pd.statusbar.hide();
                        } else {
                            toastr["error"](response.message);
                            if (typeof response.redirectUrl != 'undefined') {
                                window.location.href = response.redirectUrl;
                            }
                        }
                    },
                    error: function (response) {
                        
                    }
                })
            },
            onSuccess: function (files, data, xhr, pd) {
                if (data.success) {
                    //attachment 
                    var obj = d.attachmentForm.serializeObject();
                    var f = obj.Date.split("/");
                    obj.Date = new Date(f[2], f[1] - 1, f[0]).toISOString();
                    obj.StoredPath = data.Message;
                    obj.Name = data.Name;
                    obj.GUID = data.GUID;
                    console.log(obj);
                    //worktask 
                    var wt = d.editForm.serializeObject();
                    wt.Attachment = [];
                    wt.Attachment.push(obj);
                    console.log(wt);
                    $.ajax({
                        type: "POST",
                        url: "/Worktask/InsertTask",
                        data: wt,
                        success: function (response) {
                            if (response.success) {
                                location.reload();
                            } else {
                                toastr["error"](response.message);
                                if (typeof response.redirectUrl != 'undefined') {
                                    window.location.href = response.redirectUrl;
                                }
                            }
                        },
                        error: function (response) {
                            console.log("Error API");
                        }
                    })
                }
            },
            autoSubmit: false
        });
        d.cancelUpload.click(function () {
            attachObj.stopUpload();
        })
        d.attachmentForm.submit(function (event) {
            console.log("detail");
            //stop submit normally
            event.preventDefault();
            //get attribute of form
            var $form = $(this);
            if (!$form.valid()) return false;
            d.modalAttach.modal('hide');
            attachObj.startUpload();
        })

        d.comment.click(function () {
            var data = {
                TaskID: d.taskId.val(),
                //Remove when integrate with login
                CommentContent: d.commentContent.val()
            };
            $.ajax({
                type: 'POST',
                url: '/Worktask/SaveComment',
                data: data,
                success: function (response) {
                    if (response.success) {
                        var html = '<div class="feed-element">' +
                                      '<div class="media-body ">' +
                                          '<small class="pull-right">1 second ago</small>' +
                                            '<strong>' + d.currentUser.val() + '</strong> posted a comment' +
                                               '<small class="text-muted"> '+d.timeComment.val()+' - '+d.dateComment.val()+'</small>' +
                                                   '<div class="well">' +
                                                       '' + d.commentContent.val() +
                                                      '</div>' +
                                                        '</div>' +
                                                            '</div>';
                        d.commentContent.val('');
                        d.listComment.prepend(html).hide().fadeIn(1000);

                    } else {
                        toastr["error"](response.message);
                        if (typeof response.redirectUrl != 'undefined') {
                            window.location.href = response.redirectUrl;
                        }
                    }
                },
                error: function (response) {

                }
            })
        });
    }
}
function RemoveAttachment(url) {
    $.ajax({
        type: "Post",
        url: url,
        success: function (response) {
            if (response.success) {
                location.reload();
                localStorage.setItem("Status", response.message);
            } else {
                toastr["error"](response.message);
                if (typeof response.redirectUrl != 'undefined') {
                    window.location.href = response.redirectUrl;
                }
            }
        },
        error: function (response) {
        }
    })
}
function DownloadAttachment(url) {
    $.ajax({
        type: "POST",
        url: url,
        success: function (response) {
            window.location = "/test/test";
        },
        error: function (response) {
        }
    })
}