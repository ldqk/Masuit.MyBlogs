const fs = require('fs');
const path = require('path');
const fse = require('fs-extra');

const target = path.join(__dirname, 'public', 'UEditorPlus');
const source = path.join(__dirname, '..', 'src', 'Masuit.MyBlogs.Core', 'wwwroot', 'UEditorPlus');

if (!fs.existsSync(target)) {
  if (fs.existsSync(source)) {
    fse.copySync(source, target);
    console.log('UEditorPlus 已拷贝到 public 目录');
  } else {
    console.error('源目录不存在:', source);
    process.exit(1);
  }
}
