export function currentTime(){
    var result = new Date();
    //
    var hour = result.getHours();
    var hourString = hour.toString();
    if (hour < 10) {
        hourString = '0' + hourString;
    }
    //
    var minute = result.getMinutes()
    var minuteString = minute.toString();
    if (minute < 10) {
        minuteString = "0" + minuteString;
    }
    //
    var second = result.getSeconds()
    var secondString = second.toString();
    if (second < 10) {
        secondString = "0" + secondString;
    }
    //
    return hourString + ":" + minuteString + "." + secondString;
}

export function versionClient(){
    return "v0.33 Client";
}
