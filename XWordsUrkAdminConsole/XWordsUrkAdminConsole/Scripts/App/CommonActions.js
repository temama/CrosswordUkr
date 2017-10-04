function ShowProgress() {
    $("#global-progress-bar").show();
}

function HideProgress() {
    $("#global-progress-bar").hide();
}

var avatarColors = [
    "#FFB900",
    "#D83B01",
    "#B50E0E",
    "#E81123",
    "#B4009E",
    "#5C2D91",
    "#0078D7",
    "#00B4FF",
    "#008272",
    "#107C10"
];

function GetAvatarColor(initials) {
    var sum = 0;
    for (index in initials) {
        sum += initials.charCodeAt(index);
    }
    return avatarColors[sum % avatarColors.length];
}

// Bootstrap modals works not quite fine when open few at time. This should fix that
function SortOutModals(modalName) {
    $(modalName).on("hidden.bs.modal", function (e) {
        if ($('.modal:visible').length) {
            $('.modal-backdrop').first().css('z-index', parseInt($('.modal:visible').last().css('z-index')) - 10);
            $('body').addClass('modal-open');
        }
    }).on("show.bs.modal", function (e) {
        if ($('.modal:visible').length) {
            $('.modal-backdrop.in').first().css('z-index', parseInt($('.modal:visible').last().css('z-index')) + 10);
            $(this).css('z-index', parseInt($('.modal-backdrop.in').first().css('z-index')) + 10);
        }
    });
}

function ShowModalWithScrolling(modalName) {
    $(modalName).modal("show");
    SortOutModals(modalName);
}



/*  Layout   */
function OpenActionInLayoutModal(action) {
    var url = action;
    $.get(url, function (htmlData) {
        $("#layout-modal-dialog").html(htmlData);
        $("#layout-modal").modal("show");
    });
}