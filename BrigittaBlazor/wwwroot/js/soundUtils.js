function playSound(soundFile) {
    var audio = new Audio("Sounds/" + soundFile);
    audio.volume = 0.5;
    audio.play();
    
    console.log("playSound: " + soundFile + " " + audio.volume);
}
