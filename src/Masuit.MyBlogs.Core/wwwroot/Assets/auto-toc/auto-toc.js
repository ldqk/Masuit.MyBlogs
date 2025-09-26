class AutoToc {
    constructor(options = {}) {
        const defaults = {
            contentSelector: '#content',
            tocSelector: '',
            minLevel: 2,
            maxLevel: 4,
            scrollOffset: 0,
            observe: true,
            headings: 'h1,h2,h3,h4,h5,h6',
            slugify: defaultSlugify,
            position: 'left',
            float: false,
            offset: { top: 80, left: 32, right: 32 },
            width: 260,
            closeButton: true,
            draggable: false,
            tocTitle: '目录导航',
        };
        this.opts = Object.assign({}, defaults, options);
        this.contentEl = resolveEl(this.opts.contentSelector);

        // 自动创建 toc 容器（全局）
        this.tocEl = resolveEl(this.opts.tocSelector);
        if (!this.tocEl) {
            this.tocEl = document.createElement('div');
            this.tocEl.id = 'auto-toc-global';
            this.tocEl.className = 'auto-toc-container';
            document.body.appendChild(this.tocEl);
        }

        if (!this.contentEl) throw new Error('AutoToc: content container not found.');
        if (!this.tocEl) throw new Error('AutoToc: toc container not found.');

        this._headings = [];
        this._linkMap = new Map();
        this._observer = null;
        this._activeId = null;
        this._onClose = this._onClose.bind(this);
        this._onToggle = this._onToggle.bind(this);
        this._onClick = this._onClick.bind(this);
        this._onDragStart = this._onDragStart.bind(this);
        this._onDragMove = this._onDragMove.bind(this);
        this._onDragEnd = this._onDragEnd.bind(this);
        this._onFoldClick = this._onFoldClick.bind(this);
        this._dragging = false;
        this._dragOffset = { x: 0, y: 0 };
        this._toggleBtn = null;
    }

    build() {
        this._headings = collectHeadings(this.contentEl, this.opts);
        ensureIncrementalIds(this._headings); // 使用自增id实现
        const tree = buildTree(this._headings, this.opts.minLevel, this.opts.maxLevel);

        this.tocEl.innerHTML = '';
        this.tocEl.classList.add('auto-toc-container');
        setTocPosition(this.tocEl, this.opts);

        // TOC标题
        const tocTitleDiv = document.createElement('div');
        tocTitleDiv.className = 'toc-title';
        tocTitleDiv.textContent = this.opts.tocTitle || '目录导航';
        this.tocEl.appendChild(tocTitleDiv);

        // Close button
        if (this.opts.closeButton) {
            const closeBtn = document.createElement('button');
            closeBtn.className = 'toc-close-btn';
            closeBtn.title = '关闭目录';
            closeBtn.innerHTML = '&times;';
            closeBtn.addEventListener('click', this._onClose, false);
            this.tocEl.appendChild(closeBtn);

            // Toggle button
            if (!this._toggleBtn) {
                this._toggleBtn = document.createElement('button');
                this._toggleBtn.className = 'toc-toggle-btn';
                this._toggleBtn.textContent = '显示目录';
                this._toggleBtn.style.display = 'none';
                document.body.appendChild(this._toggleBtn);
                this._toggleBtn.addEventListener('click', this._onToggle, false);
            }
            setToggleBtnPosition(this._toggleBtn, this.opts);
        }

        const nav = document.createElement('nav');
        nav.className = 'toc';
        const ul = renderTreeWithFold(tree, this._linkMap);
        nav.appendChild(ul);
        this.tocEl.appendChild(nav);

        this.tocEl.addEventListener('click', this._onClick, false);
        this.tocEl.addEventListener('click', this._onFoldClick, false);

        if (this.opts.float && this.opts.draggable) {
            this.tocEl.classList.add('draggable');
            this.tocEl.addEventListener('mousedown', this._onDragStart, false);
            document.addEventListener('mousemove', this._onDragMove, false);
            document.addEventListener('mouseup', this._onDragEnd, false);
        }

        if (this.opts.observe) {
            this._setupObserver();
        }
    }

    destroy() {
        if (this._observer) {
            this._observer.disconnect();
            this._observer = null;
        }
        this.tocEl.removeEventListener('click', this._onClick, false);
        this.tocEl.removeEventListener('click', this._onFoldClick, false);
        this.tocEl.removeEventListener('mousedown', this._onDragStart, false);
        document.removeEventListener('mousemove', this._onDragMove, false);
        document.removeEventListener('mouseup', this._onDragEnd, false);
        this._linkMap.clear();
        this._headings = [];
        this._activeId = null;
        if (this._toggleBtn) {
            this._toggleBtn.remove();
            this._toggleBtn = null;
        }
    }

    _onClose() {
        this.tocEl.style.display = 'none';
        if (this._toggleBtn) {
            setToggleBtnPosition(this._toggleBtn, this.opts, this.tocEl);
            this._toggleBtn.style.display = '';
        }
    }
    _onToggle() {
        this.tocEl.style.display = '';
        if (this._toggleBtn) this._toggleBtn.style.display = 'none';
    }

    _onDragStart(e) {
        if (!this.opts.float || !this.opts.draggable) return;
        if (e.target.classList.contains('toc-close-btn')) return;
        this._dragging = true;
        this._dragOffset.x = e.clientX - this.tocEl.offsetLeft;
        this._dragOffset.y = e.clientY - this.tocEl.offsetTop;
        this.tocEl.classList.add('dragging');
        e.preventDefault();
    }
    _onDragMove(e) {
        if (!this._dragging) return;
        this.tocEl.style.left = `${e.clientX - this._dragOffset.x}px`;
        this.tocEl.style.top = `${e.clientY - this._dragOffset.y}px`;
        if (this._toggleBtn && this.tocEl.style.display === 'none') {
            this._toggleBtn.style.left = this.tocEl.style.left;
            this._toggleBtn.style.top = this.tocEl.style.top;
            this._toggleBtn.style.transform = this.tocEl.style.transform;
        }
    }
    _onDragEnd(e) {
        if (!this._dragging) return;
        this._dragging = false;
        this.tocEl.classList.remove('dragging');
    }

    _onClick(e) {
        // 折叠按钮不触发跳转
        if (e.target.classList.contains('toc-fold-btn')) {
            return;
        }
        const a = e.target.closest('a');
        if (!a || !this.tocEl.contains(a)) return;
        const href = a.getAttribute('href') || '';
        if (!href.startsWith('#')) return;

        e.preventDefault();
        const id = decodeURIComponent(href.slice(1));
        const target = document.getElementById(id);
        if (!target) return;

        const top = Math.max(
            0,
            Math.floor(target.getBoundingClientRect().top + window.pageYOffset - this.opts.scrollOffset)
        );
        window.history.pushState(null, '', `#${encodeURIComponent(id)}`);
        window.scrollTo({ top, behavior: 'smooth' });

        //setTimeout(() => {
        //    location.hash = encodeURIComponent(id);
        //}, 400);
    }

    _onFoldClick(e) {
        if (!e.target.classList.contains('toc-fold-btn')) return;
        const li = e.target.closest('li');
        if (!li) return;
        if (li.classList.contains('folded')) {
            li.classList.remove('folded');
            e.target.setAttribute('aria-label', '折叠');
            e.target.innerHTML = '▾';
        } else {
            li.classList.add('folded');
            e.target.setAttribute('aria-label', '展开');
            e.target.innerHTML = '▸';
        }
        //e.stopPropagation();
    }

    _setupObserver() {
        if (!('IntersectionObserver' in window)) {
            const onScroll = () => {
                const pos = window.pageYOffset + this.opts.scrollOffset + 2;
                let current = null;
                for (const h of this._headings) {
                    const y = h.el.offsetTop;
                    if (y <= pos) current = h;
                    else break;
                }
                this._activate(current ? current.id : null);
            };
            this._scrollHandler = onScroll;
            window.addEventListener('scroll', onScroll, { passive: true });
            onScroll();
            return;
        }

        const marginTop = this.opts.scrollOffset + 4;
        const options = {
            root: null,
            rootMargin: `-${marginTop}px 0px -70% 0px`,
            threshold: [0, 1.0],
        };
        this._observer = new IntersectionObserver(this._onIntersect.bind(this), options);
        for (const h of this._headings) {
            this._observer.observe(h.el);
        }
    }

    _onIntersect(entries) {
        entries.forEach((entry) => {
            const id = entry.target.id;
            const meta = this._headings.find((h) => h.id === id);
            if (meta) meta._isIntersecting = entry.isIntersecting;
            meta && (meta._top = entry.target.getBoundingClientRect().top);
        });

        const visible = this._headings
            .filter((h) => h._isIntersecting)
            .sort((a, b) => Math.abs(a._top) - Math.abs(b._top));

        const candidate = visible[0] || nearestAbove(this._headings, this.opts.scrollOffset);
        this._activate(candidate ? candidate.id : null);
    }

    _activate(id) {
        id = id ? decodeURIComponent(id) : id;
        if (id === this._activeId) return;
        if (this._activeId && this._linkMap.has(this._activeId)) {
            const prev = this._linkMap.get(this._activeId);
            prev.classList.remove('active');
            let li = prev.closest('li');
            while (li) {
                li.classList.remove('active-branch');
                li = li.parentElement && li.parentElement.closest('li');
            }
        }

        this._activeId = id;

        if (id && this._linkMap.has(id)) {
            const link = this._linkMap.get(id);
            link.classList.add('active');

            let li = link.closest('li');
            while (li) {
                li.classList.add('active-branch');
                li = li.parentElement && li.parentElement.closest('li');
            }

            const tocScrollContainer = this.tocEl.querySelector('.toc');
            if (tocScrollContainer && typeof link.scrollIntoView === 'function') {
                const linkRect = link.getBoundingClientRect();
                const containerRect = tocScrollContainer.getBoundingClientRect();
                if (linkRect.top < containerRect.top || linkRect.bottom > containerRect.bottom) {
                    link.scrollIntoView({ block: 'center', behavior: 'smooth' });
                }
            }
        }
    }
}

/* ========== Helpers ========== */

function resolveEl(selOrEl) {
    if (!selOrEl) return null;
    if (typeof selOrEl === 'string' && selOrEl) return document.querySelector(selOrEl);
    if (selOrEl instanceof Element) return selOrEl;
    return null;
}

// 用自增id生成锚点id
function ensureIncrementalIds(headings) {
    let seq = 1;
    headings.forEach((h) => {
        if (h.el.id && typeof h.el.id === 'string' && h.el.id.trim()) {
            h.id = h.el.id;
        } else {
            h.id = `toc-anchor-${seq++}`;
            h.el.id = h.id;
        }
    });
}

function defaultSlugify(text) {
    const normalized = text.normalize('NFKD').replace(/[\u0300-\u036f]/g, '').trim().toLowerCase();
    const keepCJK = /[一-龥\u3400-\u9FFF\uF900-\uFAFF]/g;
    const cjkOnly = (s) => s.match(keepCJK)?.join('') ?? '';
    let slug = normalized.replace(/\s+/g, '-').replace(/[^\w\-一-龥\u3400-\u9FFF\uF900-\uFAFF]/g, '');
    if (!slug) slug = cjkOnly(text).replace(/\s+/g, '-');
    return slug || 'section';
}

function collectHeadings(root, opts) {
    const all = Array.from(root.querySelectorAll(opts.headings));
    const filtered = all
        .map((el) => {
            const tag = el.tagName.toLowerCase();
            const level = parseInt(tag.replace('h', ''), 10);
            return { el, level, text: getHeadingText(el) };
        })
        .filter((h) => h.level >= opts.minLevel && h.level <= opts.maxLevel);
    return filtered;
}

function getHeadingText(el) {
    const clone = el.cloneNode(true);
    clone.querySelectorAll('a, .anchor, .header-anchor, .hash-link').forEach((n) => n.remove());
    return (clone.textContent || '').trim();
}

function buildTree(headings, minLevel, maxLevel) {
    const root = { children: [], level: minLevel - 1 };
    const stack = [root];
    headings.forEach((h) => {
        const node = { id: h.id, text: h.text, level: h.level, children: [] };
        while (stack.length && h.level <= stack[stack.length - 1].level) {
            stack.pop();
        }
        stack[stack.length - 1].children.push(node);
        stack.push(node);
    });
    return root.children;
}

// 渲染带折叠按钮的目录树
function renderTreeWithFold(tree, linkMap) {
    const ul = document.createElement('ul');
    ul.className = 'toc-list';
    tree.forEach((node) => {
        ul.appendChild(renderNodeWithFold(node, linkMap));
    });
    return ul;
}

function renderNodeWithFold(node, linkMap) {
    const li = document.createElement('li');
    li.className = `toc-item level-${node.level}`;

    // 折叠按钮（仅有子节点才显示）
    let foldBtn = null;
    if (node.children && node.children.length) {
        foldBtn = document.createElement('button');
        foldBtn.className = 'toc-fold-btn';
        foldBtn.setAttribute('aria-label', '折叠');
        foldBtn.innerHTML = '▾';
        li.appendChild(foldBtn);
    }

    const a = document.createElement('a');
    a.href = `#${encodeURIComponent(node.id)}`;
    a.textContent = node.text || 'Untitled';
    a.className = 'toc-link';
    linkMap.set(node.id, a);
    li.appendChild(a);

    if (node.children && node.children.length) {
        const ul = document.createElement('ul');
        ul.className = 'toc-list';
        node.children.forEach((child) => ul.appendChild(renderNodeWithFold(child, linkMap)));
        li.appendChild(ul);
    }
    return li;
}

function nearestAbove(headings, offset) {
    const y = window.pageYOffset + offset + 2;
    let best = null;
    for (const h of headings) {
        const top = h.el.offsetTop;
        if (top <= y) best = h;
        else break;
    }
    return best;
}

function setTocPosition(tocEl, opts) {
    tocEl.style.position = opts.float ? 'fixed' : 'sticky';
    tocEl.style.zIndex = opts.float ? 2 : '';
    tocEl.style.width = opts.width + 'px';
    tocEl.style.left = '';
    tocEl.style.right = '';
    tocEl.style.top = '';
    tocEl.style.transform = '';
    if (opts.float) {
        if (opts.position === 'right') {
            tocEl.style.right = (opts.offset?.right ?? 32) + 'px';
            tocEl.style.left = '';
        } else if (opts.position === 'top') {
            tocEl.style.top = (opts.offset?.top ?? 32) + 'px';
            tocEl.style.left = '50%';
            tocEl.style.transform = 'translateX(-50%)';
        } else if (opts.position === 'custom') {
            tocEl.style.top = (opts.offset?.top ?? 32) + 'px';
            tocEl.style.left = (opts.offset?.left ?? 32) + 'px';
        } else {
            tocEl.style.left = (opts.offset?.left ?? 32) + 'px';
            tocEl.style.right = '';
        }
        tocEl.style.top = (opts.offset?.top ?? 80) + 'px';
        tocEl.style.boxShadow = '0 2px 12px rgba(0,0,0,0.12)';
        tocEl.style.background = '#fff';
        tocEl.style.borderRadius = '8px';
        tocEl.style.border = '1px solid #eee';
        tocEl.style.overflow = 'auto';
        tocEl.style.maxHeight = 'calc(100vh - 32px)';
    } else {
        tocEl.style.top = 'var(--toc-sticky-top, 16px)';
    }
}

function setToggleBtnPosition(toggleBtn, opts, tocEl) {
    toggleBtn.style.position = 'fixed';
    toggleBtn.style.width = opts.width + 'px';
    toggleBtn.style.zIndex = 9999;
    toggleBtn.style.transform = '';
    if (tocEl) {
        toggleBtn.style.left = tocEl.style.left || '';
        toggleBtn.style.right = tocEl.style.right || '';
        toggleBtn.style.top = tocEl.style.top || '';
        toggleBtn.style.transform = tocEl.style.transform || '';
    } else {
        toggleBtn.style.left = '';
        toggleBtn.style.right = '';
        toggleBtn.style.top = '';
        toggleBtn.style.transform = '';
        if (opts.float) {
            if (opts.position === 'right') {
                toggleBtn.style.right = (opts.offset?.right ?? 32) + 'px';
                toggleBtn.style.left = '';
            } else if (opts.position === 'top') {
                toggleBtn.style.top = (opts.offset?.top ?? 32) + 'px';
                toggleBtn.style.left = '50%';
                toggleBtn.style.transform = 'translateX(-50%)';
            } else if (opts.position === 'custom') {
                toggleBtn.style.top = (opts.offset?.top ?? 32) + 'px';
                toggleBtn.style.left = (opts.offset?.left ?? 32) + 'px';
            } else {
                toggleBtn.style.left = (opts.offset?.left ?? 32) + 'px';
                toggleBtn.style.right = '';
            }
            toggleBtn.style.top = (opts.offset?.top ?? 80) + 'px';
        } else {
            toggleBtn.style.top = 'var(--toc-sticky-top, 16px)';
        }
    }
}