/** html as text */
function htmlEncode(html) {
    return $("<div></div>").text(html).html();
}
/** text as html */
function htmlDecode(text) {
    return $("<div></div>").html(text).text();
}
/** set name/value to cookie */
function setCookie(name, value, expireDays) {
    var expires = "";
    if (expireDays) {
        var now = new Date();
        now.setTime(now.getTime() + (expireDays * 24 * 60 * 60 * 1000));
        expires = ";expires=" + now.toUTCString();
    }
    document.cookie = name + "=" + escape(value) + expires + ";path=/";
}
/** get value by name from cookie */
function getCookie(name) {
    name = name + "=";
    var cookies = document.cookie.split(';');
    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i];
        while (cookie.charAt(0) == ' ') {
            cookie = cookie.substring(1);
        }
        if (cookie.indexOf(name) == 0) {
            return unescape(cookie.substring(name.length, cookie.length));
        }
    }
    return "";
}
/** delete cookie by name */
function deleteCookie(name) {
    setCookie(name, "", -1);
}