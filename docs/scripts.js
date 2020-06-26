// Set height of body to match the inner height of window
function setInnerHeight() {
    let vh = (window.innerHeight) * 0.01;
    document.documentElement.style.setProperty('--vh', `${vh}px`);
}

const screen = document.querySelector('#unityContainer');
const footer = document.querySelector('footer');

// Calculate the canvas size 
function resizeCanvas() {
    
    const width = Number(screen.style.width.replace('px', ''));
    const height = Number(screen.style.height.replace('px', ''));
    const aspectRatio = height / width;
    screen.style.width = screen.style.height = '0px';
    setInnerHeight();
    
    let minDimensionScreen = Math.min(window.innerWidth, window.innerHeight);
    
    if(minDimensionScreen === window.innerHeight) {
        minDimensionScreen *= .875;
        
        screen.style.width = `${ minDimensionScreen / aspectRatio }px`;
        screen.style.height = `${ minDimensionScreen }px`;
    } else {
        screen.style.width = `${ window.innerWidth }px`;
        screen.style.height = `${ window.innerHeight }px`;
    }
}

resizeCanvas()

window.addEventListener('resize', () => setTimeout(resizeCanvas, 1))
