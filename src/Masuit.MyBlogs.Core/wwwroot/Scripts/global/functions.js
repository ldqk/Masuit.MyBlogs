// 清除选中
var clearSelect = "getSelection" in window ? function () {
  window.getSelection().removeAllRanges();
} : function () {
  if (document.selection) {
    document.selection.empty();
  }
};

// 复制剪贴板 hack
function hackClip() {
  let transfer = document.createElement('input');
  document.body.appendChild(transfer);
  transfer.value = '1';
  transfer.select();
  try {
    document.execCommand('copy');
  } catch (e) {
    // execCommand 已弃用，现代浏览器建议使用 Clipboard API
    if (navigator.clipboard) {
      navigator.clipboard.writeText('1').catch(function () { });
    }
  }
  document.body.removeChild(transfer);
}

// 键盘判断工具
function getKey(e) {
  if (typeof e.code === "string") {
    return e.code;
  }
  if (typeof e.key === "string") {
    return e.key;
  }
  return e.keyCode;
}

// 反调试，频繁触发debugger
function antiDebug() {
  setInterval(function () {
    // 触发debugger
    (function () { }["constructor"]("debugger")());
    // 通过控制台检测（如有需要可拓展）
    if (window.outerWidth - window.innerWidth > 100 || window.outerHeight - window.innerHeight > 100) {
      (function () { }["constructor"]("debugger")());
    }
  }, 500);
}

/**禁止复制（增加反调试） */
function CopyrightProtect() {
  antiDebug(); // 启动反调试
  setInterval(function () {
    try {
      document.querySelectorAll('.article-content').forEach(function (el) {
        el.addEventListener('keydown', function (e) {
          const key = getKey(e);
          if (
            key === "F12" ||
            ((e.ctrlKey && (key === "KeyC" || key === "c" || key === "C")) ||
              (e.ctrlKey && (key === "KeyS" || key === "s" || key === "S")) ||
              (e.ctrlKey && (key === "KeyU" || key === "u" || key === "U")))
          ) {
            clearSelect();
            e.stopPropagation();
            e.preventDefault();
            return true;
          }
        });
        el.addEventListener('copy', function (e) {
          e.preventDefault();
          hackClip();
          return false;
        });
      });

      document.onkeydown = function (e) {
        const key = getKey(e);
        if (
          key === "F12" ||
          ((e.ctrlKey && (key === "KeyA" || key === "a" || key === "A")) ||
            (e.ctrlKey && (key === "KeyS" || key === "s" || key === "S")) ||
            (e.ctrlKey && (key === "KeyU" || key === "u" || key === "U")) ||
            (e.ctrlKey && e.shiftKey) ||
            e.altKey)
        ) {
          clearSelect();
          e.stopPropagation();
          e.preventDefault();
          return false;
        }
        return true;
      };
      document.ondragstart = function (e) {
        e.preventDefault();
        hackClip();
        return false;
      };
      document.oncontextmenu = function (e) {
        e.preventDefault();
        hackClip();
        return false;
      };
    } catch (ex) {
      console.error(ex);
    }
  }, 500);
}

/**禁止编辑器内复制 */
function CopyrightProtect4Editor() {
  antiDebug(); // 启动反调试
  setInterval(function () {
    try {
      var editorFrame = document.getElementById("ueditor_0");
      if (!editorFrame || !editorFrame.contentWindow || !editorFrame.contentWindow.document.body) return;
      var body = editorFrame.contentWindow.document.body;
      body.onkeydown = function (e) {
        const key = getKey(e);
        if (
          key === "F12" ||
          ((e.ctrlKey && (key === "KeyC" || key === "c" || key === "C")) ||
            (e.ctrlKey && (key === "KeyS" || key === "s" || key === "S")) ||
            (e.ctrlKey && (key === "KeyU" || key === "u" || key === "U")) ||
            (e.ctrlKey && (key === "KeyX" || key === "x" || key === "X")) ||
            (e.ctrlKey && e.shiftKey) ||
            e.altKey)
        ) {
          clearSelect();
          e.stopPropagation();
          e.preventDefault();
          return false;
        }
        return true;
      };
      body.ondragstart = function (e) {
        e.preventDefault();
        hackClip();
        return false;
      };
      body.oncopy = function (e) {
        e.preventDefault();
        hackClip();
        return false;
      };
    } catch (ex) {
      console.error(ex);
    }
  }, 500);
}

/**禁止全局复制 */
function GlobalCopyrightProtect() {
  antiDebug(); // 启动反调试
  setInterval(function () {
    try {
      document.querySelectorAll('.article-content').forEach(function (el) {
        el.addEventListener('keydown', function (e) {
          const key = getKey(e);
          if (
            key === "F12" ||
            ((e.ctrlKey && (key === "KeyC" || key === "c" || key === "C")) ||
              (e.ctrlKey && (key === "KeyS" || key === "s" || key === "S")) ||
              (e.ctrlKey && (key === "KeyU" || key === "u" || key === "U")))
          ) {
            clearSelect();
            e.stopPropagation();
            e.preventDefault();
            return false;
          }
          return true;
        });
        el.addEventListener('copy', function (e) {
          e.preventDefault();
          hackClip();
          return false;
        });
      });
      document.onkeydown = function (e) {
        const key = getKey(e);
        if (
          key === "F12" ||
          ((e.ctrlKey && (key === "KeyA" || key === "a" || key === "A")) ||
            (e.ctrlKey && (key === "KeyS" || key === "s" || key === "S")) ||
            (e.ctrlKey && (key === "KeyU" || key === "u" || key === "U")) ||
            (e.ctrlKey && e.shiftKey) ||
            e.altKey)
        ) {
          clearSelect();
          e.stopPropagation();
          e.preventDefault();
          return false;
        }
        return true;
      };
      document.ondragstart = function (e) {
        e.preventDefault();
        hackClip();
        return false;
      };
      document.oncontextmenu = function (e) {
        e.preventDefault();
        hackClip();
        return false;
      };
    } catch (ex) {
      console.error(ex);
    }
  }, 500);
}

function GetOperatingSystem(os) {
  if (os) {
    if (os.indexOf("Windows") >= 0) {
      return '<i class="icon-windows8"></i>' + os;
    } else if (os.indexOf("Mac") >= 0) {
      return '<i class="icon-apple"></i>' + os;
    } else if (os.indexOf("Chrome") >= 0) {
      return '<i class="icon-chrome"></i>' + os;
    } else if (os.indexOf("Android") >= 0) {
      return '<i class="icon-android"></i>' + os;
    } else {
      return '<i class="icon-stats"></i>' + os;
    }
  } else {
    return '<i class="icon-stats"></i>未知操作系统';
  }
}

function GetBrowser(browser) {
  if (browser) {
    if (browser.indexOf("Chrome") >= 0) {
      return '<i class="icon-chrome"></i>' + browser;
    } else if (browser.indexOf("Firefox") >= 0) {
      return '<i class="icon-firefox"></i>' + browser;
    } else if (browser.indexOf("IE") >= 0) {
      return '<i class="icon-IE"></i>' + browser;
    } else if (browser.indexOf("Edge") >= 0) {
      return '<i class="icon-edge"></i>' + browser;
    } else if (browser.indexOf("Opera") >= 0) {
      return '<i class="icon-opera"></i>' + browser;
    } else if (browser.indexOf("Safari") >= 0) {
      return '<i class="icon-safari"></i>' + browser;
    } else {
      return '<i class="icon-browser2"></i>' + browser;
    }
  } else {
    return '<i class="icon-browser2"></i>未知浏览器';
  }
}

async function blockCategory(id, name) {
  let value = Cookies.get("HideCategories") || "0";
  if (value.split(",").indexOf(id + "") > -1) {
    window.dialog.info({
      title: "确认移除屏蔽【" + name + "】吗？",
      content: '移除屏蔽之后可能会出现一些引起不适的内容，请谨慎操作，确认关闭吗？',
      positiveText: '确定',
      negativeText: '取消',
      onPositiveClick: async () => {
        Cookies.set("HideCategories", value.split(",").filter(function (item) { return item != id }).join(","), { expires: 365 });
        window.message.success("取消屏蔽成功");
      }
    });
  } else {
    window.dialog.info({
      title: "确认屏蔽【" + name + "】吗？",
      content: '屏蔽之后将不再收到该分类的相关推送！若需要取消屏蔽，清除本站的浏览器缓存即可。',
      positiveText: '确定',
      negativeText: '取消',
      onPositiveClick: async () => {
        Cookies.set("HideCategories", id + "," + value, { expires: 365 });
        window.message.success("屏蔽成功");
      }
    });
  }
}

async function disableSafemode() {
  window.dialog.warning({
    title: "确认关闭安全模式吗？",
    content: "关闭安全模式后可能会出现一些引起不适的内容，请谨慎操作，确认关闭吗？",
    positiveText: "确定",
    negativeText: "取消",
    onPositiveClick: async () => {
      Cookies.set("Nsfw", 0, { expires: 3650 });
      location.reload();
    }
  });
}

async function enableSafemode() {
  Cookies.set("Nsfw", 1, { expires: 3650 });
  window.message.success("已开启安全模式");
  location.reload();
}

/*默认安全模式*/
document.addEventListener('DOMContentLoaded', function () {
  const { createDiscreteApi } = naive;
  const { message, dialog } = createDiscreteApi(["message", "dialog"]);
  window.message = message;
  window.dialog = dialog;
  if (Cookies.get("Nsfw") != "0") {
    let safeModeBtn = document.createElement('a');
    safeModeBtn.style.position = 'fixed';
    safeModeBtn.style.left = '0';
    safeModeBtn.style.bottom = '0';
    safeModeBtn.style.color = 'black';
    safeModeBtn.style.zIndex = '10';
    safeModeBtn.style.textShadow = '0px 0px 1px #000';
    safeModeBtn.innerText = '安全模式';
    safeModeBtn.onclick = disableSafemode;
    document.body.appendChild(safeModeBtn);
  }
});