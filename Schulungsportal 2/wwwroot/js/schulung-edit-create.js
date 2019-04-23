$(() => {
    $('#termine-container').on('click', '.termin-remove', (event) => {
        console.log('asdf');
        $(event.target).parents('.termin').remove();
        $('.termin').each(function (index) {
            $(this).find('.start-date').attr('name', 'TermineVM[' + index + '].StartDate').attr('id', 'TermineVM_' + index + '_StartDate');
            $(this).find('.start-time').attr('name', 'TermineVM[' + index + '].StartTime').attr('id', 'TermineVM_' + index + '_StartTime');
            $(this).find('.end-date').attr('name', 'TermineVM[' + index + '].EndDate').attr('id', 'TermineVM_' + index + '_EndDate');
            $(this).find('.end-time').attr('name', 'TermineVM[' + index + '].EndTime').attr('id', 'TermineVM_' + index + '_EndTime');
        });
    });
    $('#termin-add').on('click', () => {
        var rowCount = $('.termin').length;
        // line of doom, wie in Bearbeiten.cshtml zum dynamisch hinzufügen
        $('#termine-container').append('<div class="termin form-group"><div class="col-md-2" ></div ><div class="col-md-2"><input class="form-control text-box single-line start-date" data-val="true" data-val-date="Das Feld &quot;Start-Termin&quot; muss eine Datumsangabe sein." data-val-required="Das Feld &quot;Start-Termin&quot; ist erforderlich." id="TermineVM_' + rowCount + '_StartDate" name="TermineVM[' + rowCount + '].StartDate" placeholder="dd.mm.yyyy" type="date" value=""><span class="field-validation-valid text-danger" data-valmsg-for="termin.StartDate" data-valmsg-replace="true"></span></div><div class="col-md-2"><input class="form-control text-box single-line start-time" data-val="true" data-val-required="Das Feld &quot;Start-Termin&quot; ist erforderlich." id="TermineVM_' + rowCount + '_StartTime" name="TermineVM[' + rowCount + '].StartTime" placeholder="hh:mm" type="time" value=""><span class="field-validation-valid text-danger" data-valmsg-for="termin.StartTime" data-valmsg-replace="true"></span></div><div class="col-md-1">bis</div><div class="col-md-2"><input class="form-control text-box single-line end-date" data-val="true" data-val-date="Das Feld &quot;End-Termin&quot; muss eine Datumsangabe sein." data-val-required="Das Feld &quot;End-Termin&quot; ist erforderlich." id="TermineVM_' + rowCount + '_EndDate" name="TermineVM[' + rowCount + '].EndDate" placeholder="dd.mm.yyyy" type="date" value=""><span class="field-validation-valid text-danger" data-valmsg-for="termin.EndDate" data-valmsg-replace="true"></span></div><div class="col-md-2"><input class="form-control text-box single-line end-time" data-val="true" data-val-required="Das Feld &quot;End-Uhrzeit&quot; ist erforderlich." id="TermineVM_' + rowCount + '_EndTime" name="TermineVM[' + rowCount + '].EndTime" placeholder="hh:mm" type="time" value=""><span class="field-validation-valid text-danger" data-valmsg-for="termin.EndTime" data-valmsg-replace="true"></span></div><div class="col-md-1"><button class="btn btn-danger termin-remove" title="entfernen" type="button">-</button></div></div >')
    });
})