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