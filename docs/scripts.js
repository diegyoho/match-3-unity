// Set height of body to match the inner height of window
function setInnerHeight() {
    let vh = window.innerHeight * 0.01;
    document.documentElement.style.setProperty('--vh', `${vh}px`);
}

const screen = document.querySelector('#unityContainer');
const footer = document.querySelector('footer');

console.log(`Footer ${footer.height}`);

// Calculate the canvas size 
function resizeCanvas() {
    setInnerHeight();
    
    const width = Number(screen.style.width.replace('px', ''));
    const height = Number(screen.style.height.replace('px', ''));
    const aspectRatio = height / width;

    let minDimensionScreen = Math.min(window.innerWidth, window.innerHeight);

    if(minDimensionScreen === window.innerHeight) {
        minDimensionScreen *= .875;

        screen.style.width = `${ minDimensionScreen / aspectRatio }px`
        screen.style.height = `${ minDimensionScreen }px`
    } else {
        screen.style.width = `${ minDimensionScreen }px`
        screen.style.height = `${ window.innerHeight * .875 }px`
    }

    document.documentElement.style.setProperty('--map-width', `${minDimensionScreen * 0.01}px`)
}

resizeCanvas()

window.addEventListener('resize', () => setTimeout(resizeCanvas, 1))

