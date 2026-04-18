setInterval(function() {
    if (typeof cr_getC2Runtime !== "undefined") {
        var runtime = cr_getC2Runtime();
        
        // Find the object that has the 'player-sheet0.png' visual
        var player = Object.values(runtime.types).find(t => 
            t.animations && 
            t.animations[0] && 
            t.animations[0].frames[0].texture_img.src.includes("player-sheet0.png")
        );

        if (player && player.instances[0]) {
            var inst = player.instances[0];
            var pBody = inst.behavior.Platform;

            // Trigger gravity flip when hitting boundaries
            if (inst.y <= 10 && pBody.g > 0) {
                pBody.g = -Math.abs(pBody.g); // Flip Up
            }
            if (inst.y >= (runtime.height - 50) && pBody.g < 0) {
                pBody.g = Math.abs(pBody.g); // Flip Down
            }
            if (inst.x <= 10 || inst.x >= (runtime.width - 50)) {
                pBody.g *= -1; // Flip on walls
            }
        }
    }
}, 16);
