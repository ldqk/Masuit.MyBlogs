myApp.controller("donate", ["$scope", "$http", "NgTableParams", function($scope, $http, NgTableParams) {
    var self = this;
    $scope.paginationConf = {
        currentPage: 1,
        itemsPerPage: 10,
        pagesLength: 25,
        perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
        rememberPerPage: 'perPageItems',
        onChange: function() {
            self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
        }
    };
    this.GetPageData = function(page, size) {
        $http.post(`/donate/getpagedata?page=${page}&size=${size}`).then(function(res) {
            $scope.paginationConf.totalItems = res.data.TotalCount;
            $("div[ng-table-pagination]").remove();
            self.tableParams = new NgTableParams({
                count: 50000
            }, {
                filterDelay: 0,
                dataset: res.data.Data
            });
        });
    };
    self.del = function(row) {
        swal({
            title: "确认删除这条打赏记录吗？",
            text: row.NickName,
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            showLoaderOnConfirm: true,
            animation: true,
            allowOutsideClick: false
        }).then(function() {
            $scope.request("/donate/delete/" + row.Id, null, function(data) {
                window.notie.alert({
                    type: 1,
                    text: data.Message,
                    time: 4
                });
            });
            _.remove(self.tableParams.settings().dataset, function(item) {
                return row === item;
            });
            self.tableParams.reload().then(function(data) {
                if (data.length === 0 && self.tableParams.total() > 0) {
                    self.tableParams.page(self.tableParams.page() - 1);
                    self.tableParams.reload();
                }
            });
        }, function() {
        }).catch(swal.noop);
    }
    $scope.save = function (row) {
        if (row==null) {
            row = {
                NickName: "",
                DonateTime: "",
                Amount: "",
                Email: "",
                QQorWechat: "",
                Via:""
            };
        }
        swal({
            title: '添加打赏记录',
            html: '<div class="input-group"><span class="input-group-addon">昵称： </span><input type="text" id="name" class="form-control input-lg" placeholder="请输入昵称" value="' + row.NickName+'"></div>' +
            '<div class="input-group"><span class="input-group-addon">打赏时间： </span><input id="date" type="text" class="form-control input-lg date datainp dateicon" readonly placeholder="请输入打赏时间" value="' + row.DonateTime +'"></div>	' +
            '<div class="input-group"><span class="input-group-addon">打赏金额： </span><input id="amount" type="text" class="form-control input-lg" placeholder="请输入金额" value="' + row.Amount +'"></div>' +
            '<div class="input-group"><span class="input-group-addon">打赏方式： </span><input id="via" type="text" class="form-control input-lg" placeholder="请输入打赏方式" value="' + row.Via +'"></div>' +
            '<div class="input-group"><span class="input-group-addon">Email： </span><input type="email" id="email" class="form-control input-lg" placeholder="请输入Email" value="' + row.Email +'"></div>' +
            '<div class="input-group"><span class="input-group-addon">QQ或微信： </span><input type="text" id="qq" class="form-control input-lg" placeholder="请输入QQ或微信" value="' + row.QQorWechat +'"></div>',
            showCloseButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "确定",
            cancelButtonText: "取消",
            showLoaderOnConfirm: true,
            animation: true,
            allowOutsideClick: false,
            preConfirm: function () {
                return new Promise(function (resolve, reject) {
                    row.NickName = $("#name").val();
                    row.DonateTime = $("#date").val();
                    row.Amount = $("#amount").val();
                    row.Via = $("#via").val();
                    row.Email = $("#email").val();
                    row.QQorWechat = $("#qq").val();
                    $http.post("/donate/save", row).then(function (res) {
                        if (res.data.Success) {
                            resolve(res.data);
                        } else {
                            reject(res.data.Message);
                        }
                    }, function (error) {
                        reject("服务请求失败！");
                    });
                });
            }
        }).then(function (result) {
            if (result) {
                if (result.Success) {
                    swal(result.Message, "", "success");
                    self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
                } else {
                    swal(result.Message, "", "error");
                }
            }
        }).catch(swal.noop);
        layui.use('laydate', function(){
          var laydate = layui.laydate;
          laydate.render({
            elem: '.date',
            calendar: true,
            done: function(value, date, endDate) {
                $scope.partner.ExpireTime=value;
                $("#date").val(value);
            }
          });
        });
    }
}]);