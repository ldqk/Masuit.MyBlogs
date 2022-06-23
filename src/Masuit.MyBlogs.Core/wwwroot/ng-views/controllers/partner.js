myApp.controller("partner", ["$scope", "$http", "$timeout", "NgTableParams", function ($scope, $http, $timeout, NgTableParams) {
    var self = this;
    $scope.isAdd = true;
    $scope.allowUpload = false;
    $scope.partner = {};
    $scope.kw = "";
    $scope.paginationConf = {
        currentPage: 1,
        itemsPerPage: 10,
        pagesLength: 25,
        perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
        rememberPerPage: 'perPageItems',
        onChange: function () {
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        }
    };
    this.GetPageData = function (page, size) {
        $http.get(`/partner/getpagedata?page=${page}&size=${size}&kw=${$scope.kw}`).then(function (res) {
            $scope.paginationConf.totalItems = res.data.TotalCount;
            $("div[ng-table-pagination]").remove();
            self.tableParams = new NgTableParams({
                count: 50000
            }, {
                filterDelay: 0,
                dataset: res.data.Data
            });
            self.data = res.data.Data;
            $timeout(function () {
                self.data.forEach(item => {
                    xmSelect.render({
                        el: '#category-' + item.Id,
                        tips: '未选择分类',
                        prop: {
                            name: 'Name',
                            value: 'Id',
                            children: 'Children',
                        },
                        //model: { label: { type: 'text' } },
                        tree: {
                            show: true,
                            strict: false,
                            expandedKeys: true,
                        },
                        filterable: true, //搜索功能
                        autoRow: true, //选项过多,自动换行
                        data: $scope.cat,
                        initValue: (item.CategoryIds || "").split(","),
                        on: function (data) {
                            var arr = [];
                            for (var i = 0; i < data.arr.length; i++) {
                                arr.push(data.arr[i].Id);
                            }
                            $http.post(`/partner/${item.Id}/categories?cids=` + arr.join(",")).then(function (res) {
                                if (data.status >= 400) {
                                    layer.msg("操作失败");
                                }
                            });
                        }
                    });
                });
            }, 1);
        });
    }

    $scope.typesDropdown = xmSelect.render({
        el: '#types',
        tips: '请选择推广区域',
        direction: 'up',
        autoRow: true, //选项过多,自动换行
        data: [{ name: "轮播图", value: 1 }, { name: "列表项", value: 2 }, { name: "边栏", value: 3 }, { name: "内页", value: 4 }],
        on: function (data) {
            var arr = [];
            for (var i = 0; i < data.arr.length; i++) {
                arr.push(data.arr[i].value);
            }
            $scope.partner.Types = arr.join(",");
        }
    });

    $scope.regionDropdown = xmSelect.render({
        el: '#regionMode',
        tips: '区域',
        model: {
            icon: 'hidden',
            label: { type: 'text' }
        },
        radio: true,
        clickClose: true,
        direction: 'up',
        filterable: false, //搜索功能
        autoRow: false, //选项过多,自动换行
        data: [{ name: "不限", value: 0 }, { name: "以内", value: 1 }, { name: "以外", value: 2 }],
        on: function (data) {
            if (data.arr.length > 0) {
                $scope.partner.RegionMode = data.arr[0].value;
            }
        }
    });
    $scope.regionDropdown.setValue([0]);
    $scope.getCategory = function () {
        $http.get("/category/getcategories").then(function (res) {
            var data = res.data;
            if (data.Success) {
                $scope.cat = data.Data;
                $scope.categoryDropdown = xmSelect.render({
                    el: '#category',
                    tips: '请选择分类',
                    prop: {
                        name: 'Name',
                        value: 'Id',
                        children: 'Children',
                    },
                    direction: 'up',
                    tree: {
                        show: true,
                        strict: false,
                        expandedKeys: true,
                    },
                    filterable: true, //搜索功能
                    autoRow: true, //选项过多,自动换行
                    data: data.Data,
                    on: function (data) {
                        var arr = [];
                        for (var i = 0; i < data.arr.length; i++) {
                            arr.push(data.arr[i].Id);
                        }
                        $scope.partner.CategoryIds = arr.join(",");
                    }
                });
            } else {
                window.notie.alert({
                    type: 3,
                    text: '获取文章分类失败！',
                    time: 4
                });
            }
        });
    }
    $scope.getCategory();
    $scope.remove = function (partner) {
        layer.closeAll();
        swal({
            title: '确定移除这条广告吗？',
            text: partner.Title,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: '确定',
            cancelButtonText: '取消'
        }).then(function (isConfirm) {
            if (isConfirm) {
                $scope.request("/partner/delete/" + partner.Id, null, function (data) {
                    window.notie.alert({
                        type: 1,
                        text: data.Message,
                        time: 4
                    });
                    self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
                });
            }
        }).catch(swal.noop);
    }
    $scope.add = function () {
        $scope.partner = {
            ExpireTime: "2049-12-31 23:59:59"
        };
        $scope.isAdd = true;
        $scope.allowUpload = false;
        layer.open({
            type: 1,
            zIndex: 20,
            title: '添加广告推广',
            area: (window.screen.width > 650 ? 650 : window.screen.width) + 'px',// '340px'], //宽高
            content: $("#edit"),
            cancel: function (index, layero) {
                setTimeout(function () {
                    $("#edit").css("display", "none");
                }, 500);
                return true;
            }
        });
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            laydate.render({
                elem: '#timespan',
                type: 'datetime',
                calendar: true,
                done: function (value, date, endDate) {
                    $scope.partner.ExpireTime = value;
                }
            });
        });
    }

    $scope.edit = function (item) {
        $scope.partner = angular.copy(item);
        $scope.partner.ExpireTime = $scope.partner.ExpireTime == null ? "2049-12-31 23:59:59" : new Date($scope.partner.ExpireTime).Format("yyyy-MM-dd hh:mm:ss");
        $scope.isAdd = false;
        $scope.allowUpload = false;
        layer.closeAll();
        $scope.categoryDropdown.setValue((item.CategoryIds || "").split(','));
        $scope.typesDropdown.setValue(item.Types.split(','));
        $scope.regionDropdown.setValue([item.RegionMode]);
        layer.open({
            type: 1,
            zIndex: 20,
            title: '保存广告',
            area: (window.screen.width > 650 ? 650 : window.screen.width) + 'px',// '340px'], //宽高
            content: $("#edit"),
            cancel: function (index, layero) {
                setTimeout(function () {
                    $("#edit").css("display", "none");
                }, 500);
                return true;
            }
        });
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            laydate.render({
                elem: '#timespan',
                type: 'datetime',
                calendar: true,
                done: function (value, date, endDate) {
                    $scope.partner.ExpireTime = value;
                }
            });
        });
    }

    $scope.copy = function (item) {
        $scope.partner = angular.copy(item);
        delete $scope.partner.Id;
        $scope.partner.ExpireTime = $scope.partner.ExpireTime == null ? "2049-12-31 23:59:59" : new Date($scope.partner.ExpireTime).Format("yyyy-MM-dd hh:mm:ss");
        $scope.isAdd = true;
        $scope.allowUpload = false;
        layer.closeAll();
        $scope.categoryDropdown.setValue((item.CategoryIds || "").split(','));
        $scope.typesDropdown.setValue(item.Types.split(','));
        $scope.regionDropdown.setValue([0]);
        layer.open({
            type: 1,
            zIndex: 20,
            title: '复制广告推广',
            area: (window.screen.width > 650 ? 650 : window.screen.width) + 'px',// '340px'], //宽高
            content: $("#edit"),
            cancel: function (index, layero) {
                setTimeout(function () {
                    $("#edit").css("display", "none");
                }, 500);
                return true;
            }
        });
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            laydate.render({
                elem: '#timespan',
                type: 'datetime',
                calendar: true,
                done: function (value, date, endDate) {
                    $scope.partner.ExpireTime = value;
                }
            });
        });
    }

    $scope.closeAll = function () {
        layer.closeAll();
        setTimeout(function () {
            $("#edit").css("display", "none");
        }, 500);
    }

    $scope.submit = function (partner) {
        if ($scope.isAdd) {
            partner.Id = 0;
        }
        $scope.request("/partner/save", partner, function (data) {
            $scope.closeAll();
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
            $scope.partner.ImageUrl = "";
            $scope.partner.Description = "";
            $scope.categoryDropdown.setValue([]);
            $scope.typesDropdown.setValue([]);
            $scope.regionDropdown.setValue([0]);
        });
    }
    $scope.uploadImage = function (field) {
        $("#uploadform").ajaxSubmit({
            url: "/Upload",
            type: "post",
            success: function (data) {
                document.getElementById("uploadform").reset();
                $scope.$apply(function () {
                    $scope.partner[field] = data.Data;
                    layer.close(layer.index);
                });
            }
        });
    };

    $scope.upload = function (field) {
        $scope.imgField = field;
        layer.open({
            type: 1,
            zIndex: 20,
            title: '上传图片',
            area: [(window.screen.width > 300 ? 300 : window.screen.width) + 'px', '80px'], //宽高
            content: $("#img-upload"),
            cancel: function (index, layero) {
                return true;
            }
        });
    }

    var _timeout;
    $scope.search = function (kw) {
        if (_timeout) {
            $timeout.cancel(_timeout);
        }
        _timeout = $timeout(function () {
            $scope.kw = kw;
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
            _timeout = null;
        }, 500);
    }

    $scope.changeState = function (row) {
        $scope.request("/partner/ChangeState/" + row.Id, null, function (data) {
            window.notie.alert({
                type: 1,
                text: data.Message,
                time: 4
            });
        });
    }

    $scope.detail = function (item) {
        $scope.partner = angular.copy(item);
        layer.closeAll();
        $scope.typesDropdown.setValue(item.Types.split(','));
        layer.open({
            type: 1,
            zIndex: 20,
            offset: '50px',
            title: item.Title,
            area: (window.screen.width > 850 ? 850 : window.screen.width) + 'px',// '340px'], //宽高
            content: $("#detail"),
            cancel: function (index, layero) {
                return true;
            }
        });
        xmSelect.render({
            el: '#category-view',
            tips: '未选择分类',
            disabled: true,
            prop: {
                name: 'Name',
                value: 'Id',
                children: 'Children',
            },
            tree: {
                show: true,
                strict: false,
                expandedKeys: true,
            },
            autoRow: true, //选项过多,自动换行
            data: $scope.cat,
            initValue: (item.CategoryIds || "").split(",")
        });
        xmSelect.render({
            el: '#types-view',
            disabled: true,
            autoRow: true, //选项过多,自动换行
            data: [{ name: "轮播图", value: 1 }, { name: "列表项", value: 2 }, { name: "边栏", value: 3 }, { name: "内页", value: 4 }],
            initValue: (item.Types || "").split(",")
        });
    }

    $scope.delayShow = function (item) {
        $scope.partner = item;
        $scope.isAdd = false;
        $scope.partner.ExpireTime = new Date($scope.partner.ExpireTime).Format("yyyy-MM-dd hh:mm:ss");
        layer.open({
            type: 1,
            zIndex: 20,
            title: "延期广告",
            content: $("#delay"),
            cancel: function (index, layero) {
                return true;
            }
        });
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            laydate.render({
                elem: '.timespan',
                type: 'datetime',
                calendar: true,
                done: function (value, date, endDate) {
                    $scope.partner.ExpireTime = value;
                }
            });
        });
    }

    $scope.insight = function (row) {
        layer.full(layer.open({
            type: 2,
            title: '广告《' + row.Title + '》洞察分析',
            maxmin: true, //开启最大化最小化按钮
            area: ['893px', '100vh'],
            content: '/partner/' + row.Id + '/insight'
        }));
    }
}]);