const fs = require('fs');
const path = require('path');
const fse = require('fs-extra');

const source = path.join(__dirname, '..', 'src', 'Masuit.MyBlogs.Core', 'wwwroot', 'dashboard', 'UEditorPlus');

if (fs.existsSync(source)) {
  fse.remove(source);
}
