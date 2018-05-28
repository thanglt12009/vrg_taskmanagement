
$(".reset-button").on("click", function () {
    // Reset form inputted data
    
    $(this).parents('form:first')[0].reset();

	// safely remove all error validation span on form
	$("span.field-validation-valid > span").remove();
    //reset value for Assignee
	if(typeof $("#Assignee") != 'undefined'){
	    $("#Assignee").val("");
	}
	if (typeof $(".ajax-file-upload-container") != 'undefined') {
	    $(".ajax-file-upload-container").remove();
	    var temp = $.parseJSON(localStorage.getItem("temp"));
	    if (typeof (temp) != 'undefined' && temp != null) {
	        for (var i = 0; i < temp.length; i++) {
	            //remove file on server
	            $.ajax({
	                url: '/Worktask/DeleteUploaderFile?path=' + temp[i].StoredPath + '&id=0',
	                type: 'POST',
	                async: false,
	                success: function (response) {
	                    if (response.success) {
	                        toastr["success"](response.message);
	                    } else {
	                        toastr["error"](response.message);
	                        if (typeof response.redirectUrl != 'undefined') {
	                            window.location.href = response.redirectUrl;
	                        }
	                    }
	                }
	            })
	        }
	    }
	    localStorage.setItem("temp", "");
	}
});