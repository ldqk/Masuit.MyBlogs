(function () {
    const carousel = document.querySelector('.carousel');
    const slides = Array.from(carousel.querySelectorAll('.slide'));
    const prevBtn = carousel.querySelector('.nav.prev');
    const nextBtn = carousel.querySelector('.nav.next');
    const indicatorsWrap = carousel.querySelector('#carousel-indicators');

    if (!slides.length) return;

    let current = Math.max(0, slides.findIndex(s => s.classList.contains('is-active')));
    if (current === -1) current = 0;

    const ANIMS = ['fade', 'slide-left', 'slide-right', 'zoom', 'flip'];
    const DURATION_MS = 650;          // 动画时长（需与 CSS 保持一致）
    const AUTO_INTERVAL = 5000;       // 自动切换间隔
    let autoTimer = null;
    let isAnimating = false;

    // 将任意 CSS 长度解析为像素（支持 px/vh/rem/% 等）
    function toPx(lengthStr, ref = document.body) {
        if (!lengthStr) return NaN;
        const v = String(lengthStr).trim();
        if (!v) return NaN;
        if (/^-?\d+(\.\d+)?px$/.test(v)) return parseFloat(v);
        const el = document.createElement('div');
        el.style.position = 'absolute';
        el.style.visibility = 'hidden';
        el.style.height = v;
        (ref || document.body).appendChild(el);
        const px = el.getBoundingClientRect().height;
        el.remove();
        return px || NaN;
    }

    // 解析最大高度（像素），默认 700px
    function resolveMaxPx() {
        const cs = getComputedStyle(carousel);
        const varMax = cs.getPropertyValue('--max-height').trim();
        let maxH = toPx(varMax, document.body);
        if (!isFinite(maxH) || maxH <= 0) maxH = 700;
        return maxH;
    }

    // 指示器
    const indicators = [];
    (function buildIndicators() {
        if (!indicatorsWrap) return;
        indicatorsWrap.innerHTML = '';
        slides.forEach((_, idx) => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'indicator';
            btn.setAttribute('role', 'tab');
            btn.setAttribute('aria-label', `跳转到第 ${idx + 1} 张`);
            btn.dataset.index = String(idx);
            btn.addEventListener('click', () => {
                if (isAnimating) return;
                const target = Number(btn.dataset.index);
                stopAuto();
                goTo(target, { randomAnim: true });
                startAuto();
            });
            indicatorsWrap.appendChild(btn);
            indicators.push(btn);
        });

        // 键盘导航
        indicatorsWrap.addEventListener('keydown', (e) => {
            const active = document.activeElement;
            const i = indicators.indexOf(active);
            if (i === -1) return;
            if (e.key === 'ArrowRight') {
                e.preventDefault();
                indicators[(i + 1) % indicators.length].focus();
            } else if (e.key === 'ArrowLeft') {
                e.preventDefault();
                indicators[(i - 1 + indicators.length) % indicators.length].focus();
            } else if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                active.click();
            }
        });
    })();

    function updateIndicators(activeIndex) {
        if (!indicators.length) return;
        indicators.forEach((btn, i) => {
            if (i === activeIndex) {
                btn.classList.add('is-active');
                btn.setAttribute('aria-selected', 'true');
            } else {
                btn.classList.remove('is-active');
                btn.setAttribute('aria-selected', 'false');
            }
        });
    }

    // 根据图片比例更新容器高度；当超过最大高度时启用“居中裁剪”
    function updateHeightFor(index) {
        const activeSlide = slides[index];
        if (!activeSlide) return;

        const img = activeSlide.querySelector('img');
        if (!img || !img.complete || !img.naturalWidth) {
            img && img.addEventListener('load', () => updateHeightFor(index), { once: true });
            return;
        }

        const containerWidth = carousel.clientWidth || window.innerWidth;
        const scaledHeight = img.naturalHeight * (containerWidth / img.naturalWidth);
        const maxH = resolveMaxPx();
        const desired = Math.min(scaledHeight, maxH);

        carousel.style.height = `${Math.round(desired)}px`;

        // 裁剪仅作用于当前激活图
        slides.forEach((s, i) => {
            const im = s.querySelector('img');
            if (!im) return;
            if (i === index) {
                if (scaledHeight > maxH) im.classList.add('crop');
                else im.classList.remove('crop');
            } else {
                im.classList.remove('crop');
            }
        });
    }

    function pickRandomAnim() { return ANIMS[Math.floor(Math.random() * ANIMS.length)]; }
    function clearAnimClasses(slideEl) {
        slideEl.classList.remove(
            'anim-in-fade', 'anim-out-fade',
            'anim-in-slide-left', 'anim-out-slide-left',
            'anim-in-slide-right', 'anim-out-slide-right',
            'anim-in-zoom', 'anim-out-zoom',
            'anim-in-flip', 'anim-out-flip'
        );
    }

    function goTo(nextIndex, opts = { randomAnim: true }) {
        if (isAnimating) return;
        const normalized = (nextIndex + slides.length) % slides.length;
        if (normalized === current) return;
        isAnimating = true;

        const currSlide = slides[current];
        const nextSlide = slides[normalized];

        // 先按“目标图”更新高度，避免动画过程中跳变
        updateHeightFor(normalized);

        const anim = opts.randomAnim ? pickRandomAnim() : 'fade';
        slides.forEach(clearAnimClasses);

        // 确保两张图都显示在层内，做过渡
        currSlide.classList.add('is-active');
        nextSlide.classList.add('is-active');

        // 强制回流以重启动画
        // eslint-disable-next-line no-unused-expressions
        currSlide.offsetWidth;

        currSlide.classList.add(`anim-out-${anim}`);
        nextSlide.classList.add(`anim-in-${anim}`);

        const done = () => {
            slides.forEach((s, i) => {
                if (i === normalized) s.classList.add('is-active');
                else s.classList.remove('is-active');
                clearAnimClasses(s);
            });
            current = normalized;
            updateIndicators(current);
            isAnimating = false;
        };

        const timer = setTimeout(done, DURATION_MS + 40);
        let ended = 0;
        function onEnd() {
            ended += 1;
            if (ended >= 1) {
                clearTimeout(timer);
                currSlide.removeEventListener('animationend', onEnd);
                nextSlide.removeEventListener('animationend', onEnd);
                done();
            }
        }
        currSlide.addEventListener('animationend', onEnd, { once: true });
        nextSlide.addEventListener('animationend', onEnd, { once: true });
    }

    function next() { goTo(current + 1, { randomAnim: true }); }
    function prev() { goTo(current - 1, { randomAnim: true }); }

    // 自动播放
    function startAuto() { stopAuto(); autoTimer = setInterval(next, AUTO_INTERVAL); }
    function stopAuto() { if (autoTimer) { clearInterval(autoTimer); autoTimer = null; } }

    // 左右按钮
    prevBtn.addEventListener('click', prev);
    nextBtn.addEventListener('click', next);

    // 悬停暂停（桌面端）
    const hoverable = window.matchMedia('(hover: hover)').matches;
    if (hoverable) {
        carousel.addEventListener('mouseenter', stopAuto);
        carousel.addEventListener('mouseleave', startAuto);
    }

    // 触摸滑动
    let touchStartX = 0, touchStartY = 0, touchMoved = false;
    const SWIPE_THRESHOLD = 45;

    carousel.addEventListener('touchstart', (e) => {
        stopAuto();
        const t = e.changedTouches[0];
        touchStartX = t.clientX; touchStartY = t.clientY; touchMoved = false;
    }, { passive: true });

    carousel.addEventListener('touchmove', (e) => {
        const t = e.changedTouches[0];
        const dx = t.clientX - touchStartX;
        const dy = t.clientY - touchStartY;
        if (Math.abs(dx) > Math.abs(dy) && Math.abs(dx) > 10) {
            touchMoved = true;
            e.preventDefault();
        }
    }, { passive: false });

    carousel.addEventListener('touchend', (e) => {
        const t = e.changedTouches[0];
        const dx = t.clientX - touchStartX;
        if (touchMoved && Math.abs(dx) > SWIPE_THRESHOLD) {
            if (dx < 0) next(); else prev();
        }
        startAuto();
    });

    // 尺寸变化时，按当前图片重新计算高度与裁剪
    window.addEventListener('resize', () => updateHeightFor(current));

    // 图片加载后初始化高度
    slides.forEach((s, i) => {
        const img = s.querySelector('img');
        if (!img) return;
        if (img.complete) { if (i === current) updateHeightFor(current); }
        else {
            img.addEventListener('load', () => { if (i === current) updateHeightFor(current); });
            img.addEventListener('error', () => { if (i === current) updateHeightFor(current); });
        }
    });

    // 初始化
    updateHeightFor(current);
    updateIndicators(current);
    startAuto();
})();