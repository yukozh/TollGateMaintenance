var hub = $.connection.tgmHub;

hub.client.onAnalyzeFinished = function (id) {
    $('[data-id="' + id + '"]').find('.progress-outer').remove();
    $.get('/Report/Issues/' + id, {}, function (data) {
        $('[data-id="' + id + '"]').find('form').append(data);
    });
}

hub.client.onAnalyzeProcessChanged = function (id, progress) {
    var p = parseInt(progress * 100);
    var bar = $('[data-id="' + id + '"]').find('.progress-bar');
    bar.attr('aria-valuenow', p);
    bar.attr('style', 'width:' + p + '%');
}

$.connection.hub.start(null, function () {
});