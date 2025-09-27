const slides = document.querySelectorAll('.carousel-slide');
const leftBtn = document.querySelector('.carousel-btn.left');
const rightBtn = document.querySelector('.carousel-btn.right');
const indicator = document.querySelector('.carousel-indicator');
const animationClasses = ['fade-in', 'slide-in-left', 'slide-in-right', 'zoom-in', 'rotate-in'];
let current = 0, timer;

function setIndicator() {
  indicator.innerHTML = '';
  for (let i = 0; i < slides.length; i++) {
    const dot = document.createElement('span');
    dot.className = i === current ? 'active' : '';
    dot.onclick = () => showSlide(i, true);
    indicator.appendChild(dot);
  }
}

function randomAnimation(index, direction = null) {
  let classes = animationClasses.slice();
  if (direction === 'left') {
    classes = ['slide-in-left', 'fade-in', 'zoom-in', 'rotate-in'];
  } else if (direction === 'right') {
    classes = ['slide-in-right', 'fade-in', 'zoom-in', 'rotate-in'];
  }
  return classes[Math.floor(Math.random() * classes.length)];
}

function clearAnimation(i) {
  animationClasses.forEach(cls => slides[i].classList.remove(cls));
}

function showSlide(idx, manual = false, direction = null) {
  if (idx === current) return;
  // 自动方向判断（点击指示器时更优体验）
  if (manual && direction == null) {
    direction = idx > current ? 'right' : 'left';
  }
  clearAnimation(current);
  slides[current].classList.remove('active');
  clearAnimation(idx);
  slides[idx].classList.add('active');
  slides[idx].classList.add(randomAnimation(idx, direction));
  setIndicator();
  current = idx;
  if (timer) clearInterval(timer);
  timer = setInterval(() => nextSlide(), 5000);
}

function nextSlide() {
  let nextIdx = (current + 1) % slides.length;
  showSlide(nextIdx, false, 'right');
}

function prevSlide() {
  let prevIdx = (current - 1 + slides.length) % slides.length;
  showSlide(prevIdx, false, 'left');
}

leftBtn.onclick = () => prevSlide();
rightBtn.onclick = () => nextSlide();

setIndicator();
timer = setInterval(() => nextSlide(), 5000);

// 支持触屏左右滑动
let startX = 0;
document.querySelector('.carousel').addEventListener('touchstart', e => {
  startX = e.touches[0].clientX;
});
document.querySelector('.carousel').addEventListener('touchend', e => {
  let endX = e.changedTouches[0].clientX;
  if (endX - startX > 50) prevSlide();
  else if (endX - startX < -50) nextSlide();
});

// 动画结束后清理动画class
slides.forEach((slide, idx) => {
  slide.addEventListener('animationend', () => clearAnimation(idx));
});