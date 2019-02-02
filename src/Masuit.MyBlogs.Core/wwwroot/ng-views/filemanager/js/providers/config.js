(function(angular) {
    'use strict';
    angular.module('FileManagerApp').provider('fileManagerConfig', function() {

        var values = {
            appName: '懒得勤快的博客',
            defaultLang: 'zh_cn',

            listUrl: 'file/handle',
            uploadUrl: 'file/upload',
            renameUrl: 'file/handle',
            copyUrl: 'file/handle',
            moveUrl: 'file/handle',
            removeUrl: 'file/handle',
            editUrl: 'file/handle',
            getContentUrl: 'file/handle',
            createFolderUrl: 'file/handle',
            downloadFileUrl: 'file/handle',
            downloadMultipleUrl: 'file/handle',
            compressUrl: 'file/handle',
            extractUrl: 'file/handle',
            permissionsUrl: 'file/handle',
            basePath: '/',

            searchForm: true,
            sidebar: true,
            breadcrumb: true,
            allowedActions: {
                upload: true,
                rename: true,
                move: true,
                copy: true,
                edit: true,
                changePermissions: true,
                compress: true,
                compressChooseName: true,
                extract: true,
                download: true,
                downloadMultiple: true,
                preview: true,
                remove: true,
                createFolder: true,
                pickFiles: true,
                pickFolders: true
            },

            multipleDownloadFileName: 'files.zip',
            filterFileExtensions: [],
            showExtensionIcons: true,
            showSizeForDirectories: false,
            useBinarySizePrefixes: false,
            downloadFilesByAjax: true,
            previewImagesInModal: true,
            enablePermissionsRecursive: true,
            compressAsync: false,
            extractAsync: false,
            pickCallback: null,

            isEditableFilePattern: /\.(txt|diff?|patch|svg|asc|cnf|cfg|conf|html?|.\w{0,2}html|cfm|cgi|aspx?|ini|pl|py|md|css|cs|js|jsx|ts|tsx|jsp|log|htaccess|htpasswd|gitignore|gitattributes|env|json|atom|eml|rss|markdown|sql|\w{0,5}xml|xslt?|sh|rb|as|bat|cmd|cob|for|ftn|frm|frx|inc|lisp|scm|coffee|php[3-6]?|java|c|cbl|go|h|scala|vb|tmpl|lock|go|yml|yaml|tsv|lst|config|ashx|asax|\w{0,2}proj|map)$/i,
            isImageFilePattern: /\.(jpe?g|gif|bmp|png|svg|webp|ico|tiff?)$/i,
            isExtractableFilePattern: /\.(gz|tar|rar|g?zip|7z)$/i,
            tplPath: 'ng-views/filemanager/templates'
        };

        return {
            $get: function() {
                return values;
            },
            set: function (constants) {
                angular.extend(values, constants);
            }
        };

    });
})(angular);
