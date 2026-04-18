const canvas = document.getElementById("c2canvas");
const ctx = canvas.getContext("2d");

let gravityStrength = 0.5;
let friction = 0.8;

// Gravity vectors: 0 means no pull, 1 or -1 means pull in that direction
let gravityX = 0;
let gravityY = 1; // Start with standard downward gravity

let player = {
    x: 400,
    y: 300,
    width: 30,
    height: 30,
    velX: 0,
    velY: 0,
    speed: 5,
    jumpForce: 12,
    onGround: false
};

let keys = {};
window.addEventListener("keydown", (e) => keys[e.code] = true);
window.addEventListener("keyup", (e) => keys[e.code] = false);

function update() {
    // 1. Horizontal/Vertical Controls (Relative to current gravity)
    // If gravity is vertical (Y), use A/D to move X
    if (gravityY !== 0) {
        if (keys["ArrowRight"] || keys["KeyD"]) { if (player.velX < player.speed) player.velX++; }
        if (keys["ArrowLeft"] || keys["KeyA"]) { if (player.velX > -player.speed) player.velX--; }
    } 
    // If gravity is horizontal (X), use W/S to move Y
    else {
        if (keys["ArrowDown"] || keys["KeyS"]) { if (player.velY < player.speed) player.velY++; }
        if (keys["ArrowUp"] || keys["KeyW"]) { if (player.velY > -player.speed) player.velY--; }
    }

    // 2. Apply Physics
    player.velX *= friction;
    player.velY *= friction;
    
    player.velX += gravityX * gravityStrength;
    player.velY += gravityY * gravityStrength;

    player.x += player.velX;
    player.y += player.velY;

    // 3. Collision & 4-Way Gravity Switch
    
    // Hit Bottom
    if (player.y + player.height >= canvas.height) {
        player.y = canvas.height - player.height;
        if (gravityY === 0) { gravityY = 1; gravityX = 0; } // Switch to down
        player.velY = 0;
        player.onGround = true;
    }

    // Hit Top
    if (player.y <= 0) {
        player.y = 0;
        if (gravityY === 0) { gravityY = -1; gravityX = 0; } // Switch to up
        player.velY = 0;
        player.onGround = true;
    }

    // Hit Left Wall
    if (player.x <= 0) {
        player.x = 0;
        if (gravityX === 0) { gravityX = -1; gravityY = 0; } // Switch to left
        player.velX = 0;
        player.onGround = true;
    }

    // Hit Right Wall
    if (player.x + player.width >= canvas.width) {
        player.x = canvas.width - player.width;
        if (gravityX === 0) { gravityX = 1; gravityY = 0; } // Switch to right
        player.velX = 0;
        player.onGround = true;
    }

    // 4. Jump Logic (Always jumps opposite to current gravity)
    if ((keys["Space"] || keys["KeyK"]) && player.onGround) {
        if (gravityY !== 0) player.velY = -player.jumpForce * gravityY;
        if (gravityX !== 0) player.velX = -player.jumpForce * gravityX;
        player.onGround = false;
    }
}

function draw() {
    // Clear Screen
    ctx.fillStyle = "#1a1a1a";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Draw Player (The Square)
    ctx.fillStyle = "#00ffcc";
    ctx.fillRect(player.x, player.y, player.width, player.height);
    
    // NO TEXT INDICATORS HERE
}

function gameLoop() {
    update();
    draw();
    requestAnimationFrame(gameLoop);
}

gameLoop();
