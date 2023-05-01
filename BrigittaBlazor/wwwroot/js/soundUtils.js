var lastPlayedTime = 0;

function playSound(soundFile) {
    var currentTime = Date.now();
    if (currentTime - lastPlayedTime < 500) {
        console.log("playSound: " + soundFile + " skipped");
        return;
    }

    var audio = new Audio("Sounds/" + soundFile);
    audio.volume = 0.5;
    audio.play();

    console.log("playSound: " + soundFile + " " + audio.volume);

    lastPlayedTime = currentTime;
}