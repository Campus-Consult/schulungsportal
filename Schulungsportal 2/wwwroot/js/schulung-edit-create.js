$(() => {
    // the validation library doesn't work with dynamic forms, so this is a workaround
    function resetValidation() {
        var form = $("form")
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin*/

        $.validator.unobtrusive.parse(form);
    }
    // dozenten
    function fixDozentIdAndName() {
        $('#dozenten-container .dozent').each(function (index) {
            // fix name + id for the input
            $(this).find('.dozent-name').attr('name', 'Dozenten[' + index + '].Name').attr('id', 'Dozenten_' + index + '_Name')
                .siblings().first().attr('data-valmsg-for', 'Dozenten[' + index + '].Name');
            $(this).find('.dozent-nummer').attr('name', 'Dozenten[' + index + '].Nummer').attr('id', 'Dozenten_' + index + '_Nummer')
                .siblings().first().attr('data-valmsg-for', 'Dozenten[' + index + '].Nummer');
            $(this).find('.dozent-email').attr('name', 'Dozenten[' + index + '].EMail').attr('id', 'Dozenten_' + index + '_EMail')
                .siblings().first().attr('data-valmsg-for', 'Dozenten[' + index + '].EMail');
        });
        resetValidation();
    }
    $('#dozenten-container').on('click', '.dozent-remove', (event) => {
        $(event.target).parents('.dozent').remove();
        fixDozentIdAndName();
    });
    $('#dozent-add').on('click', () => {
        // load template
        let template = $('#dozent-template > div:first-child').clone();
        $('#dozenten-container').append(template);
        fixDozentIdAndName();
    });
    // termine
    function fixTerminIdAndName() {
        $('#termine-container .termin').each(function (index) {
            // fix name + id for the input
            $(this).find('.start-date').attr('name', 'TermineVM[' + index + '].StartDate').attr('id', 'TermineVM_' + index + '_StartDate').attr('aria-describedby', 'TermineVM_' + index + '_StartDate')
            // fix name + id for the validation text
                .siblings().first().attr('data-valmsg-for', 'TermineVM[' + index + '].StartDate');
            $(this).find('.start-time').attr('name', 'TermineVM[' + index + '].StartTime').attr('id', 'TermineVM_' + index + '_StartTime').attr('aria-describedby', 'TermineVM_' + index + '_StartTime')
                .siblings().first().attr('data-valmsg-for', 'TermineVM[' + index + '].StartTime');
            $(this).find('.end-date').attr('name', 'TermineVM[' + index + '].EndDate').attr('id', 'TermineVM_' + index + '_EndDate')
                .siblings().first().attr('data-valmsg-for', 'TermineVM[' + index + '].EndDate').attr('aria-describedby', 'TermineVM_' + index + '_EndDate');
            $(this).find('.end-time').attr('name', 'TermineVM[' + index + '].EndTime').attr('id', 'TermineVM_' + index + '_EndTime')
                .siblings().first().attr('data-valmsg-for', 'TermineVM[' + index + '].EndTime').attr('aria-describedby', 'TermineVM_' + index + '_EndTime');
        });
        resetValidation();
    }
    $('#termine-container').on('click', '.termin-remove', (event) => {
        $(event.target).parents('.termin').remove();
        fixTerminIdAndName();
    });
    $('#termin-add').on('click', () => {
        // load template
        let template = $('#termin-template > div:first-child').clone();
        $('#termine-container').append(template);
        fixTerminIdAndName();
    });
})