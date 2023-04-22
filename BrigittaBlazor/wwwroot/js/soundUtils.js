function playSound(soundFile) {
    var audio = new Audio(soundFile);
    audio.volume = 0.5;
    audio.play();
    
    console.log("playSound: " + soundFile);
    console.log("volume: " + audio.volume);
}
