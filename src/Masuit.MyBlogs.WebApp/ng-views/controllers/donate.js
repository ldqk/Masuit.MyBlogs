myApp.controller("donate", ["$scope", "$http", "NgTableParams", function($scope, $http, NgTableParams) {
	window.hub.disconnect();
	var self = this;
	var source = [];
	$scope.loading();
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
		$scope.loading();
		$http.post("/donate/getpagedata", {
			page,
			size
		}).then(function(res) {
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$("div[ng-table-pagination]").remove();
			self.tableParams = new NgTableParams({
				count: 50000
			}, {
				filterDelay: 0,
				dataset: res.data.Data
			});
			source = angular.copy(res.data.Data);
			$scope.loadingDone();
		});
	};
	self.del = function(row) {
		swal({
			title: "确认删除这条捐赠记录吗？",
			text: row.NickName,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/donate/delete", {
				id: row.Id
			}, function(data) {
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
				EmailDisplay: "",
				QQorWechat: "",
				QQorWechatDisplay: "",
				Via:""
			};
		}
		swal({
			title: '添加捐赠记录',
			html: '<div class="input-group"><span class="input-group-addon">昵称： </span><input type="text" id="name" class="form-control input-lg" placeholder="请输入昵称" value="' + row.NickName+'"></div>' +
			'<div class="input-group"><span class="input-group-addon">捐赠时间： </span><input id="date" type="text" class="form-control input-lg date datainp dateicon" readonly placeholder="请输入捐赠时间" value="' + row.DonateTime +'"></div>	' +
			'<div class="input-group"><span class="input-group-addon">捐赠金额： </span><input id="amount" type="text" class="form-control input-lg" placeholder="请输入金额" value="' + row.Amount +'"></div>' +
			'<div class="input-group"><span class="input-group-addon">捐赠方式： </span><input id="via" type="text" class="form-control input-lg" placeholder="请输入捐赠方式" value="' + row.Via +'"></div>' +
			'<div class="input-group"><span class="input-group-addon">Email： </span><input type="email" id="email" class="form-control input-lg" placeholder="请输入Email" value="' + row.Email +'"></div>' +
			'<div class="input-group"><span class="input-group-addon">QQ或微信： </span><input type="text" id="qq" class="form-control input-lg" placeholder="请输入QQ或微信" value="' + row.QQorWechat +'"></div>' +
			'<div class="input-group"><span class="input-group-addon">显示Email： </span><input type="text" id="demail" class="form-control input-lg" placeholder="请输入显示Email" value="' + row.EmailDisplay +'"></div>' +
			'<div class="input-group"><span class="input-group-addon">显示QQ或微信： </span><input type="text" id="dqq" class="form-control input-lg" placeholder="请输入显示QQ或微信" value="' + row.QQorWechatDisplay +'"></div>',
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
					row.EmailDisplay = $("#demail").val();
					row.QQorWechatDisplay = $("#dqq").val();
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
		$(".date").jeDate({
			isinitVal: true,
			format: "YYYY-MM-DD",
			okfun: function (elem) {
				$("#date").val(elem.val);
			}
		});
	}
	$scope.loadingDone();
}]);