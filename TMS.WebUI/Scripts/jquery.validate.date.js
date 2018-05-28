$(function () {
    $.validator.addMethod('date',
        function (value, element) {
            if (this.optional(element)) {
                return true;
            }
            var ok = true;
            try {
                ok = value.match(/^\d\d?\/\d\d?\/\d\d\d\d$/);
            }
            catch (err) {
                ok = false;
            }
            return ok;
        });
});