myApp.controller("users", ["$scope", "$http", "$location", "$timeout","NgTableParams", function ($scope, $http, $location, $timeout,NgTableParams) {
	window.hub.stop();
	var self = this;
	$scope.isAdd = true;
	$scope.allowUpload=false;
    $scope.userinfo = {};
	$scope.kw = "";
	$scope.paginationConf = {
		currentPage:  1,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function() {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};

	this.GetPageData = function (page, size) {
        $http.get("/user/getusers?page="+page+"&size="+size+"&search="+$scope.kw).then(function(res) {
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$("div[ng-table-pagination]").remove();
			self.tableParams = new NgTableParams({count: 50000}, { filterDelay: 0, dataset: res.data.Data });
			self.data = res.data.Data;
        });
	}

	$scope.remove = function(userinfo) {
		layer.closeAll();
		swal({
			title: '确定彻底删除这个用户吗？',
			text: userinfo.Username,
			type: 'warning',
			showCancelButton: true,
			confirmButtonColor: '#3085d6',
			cancelButtonColor: '#d33',
			confirmButtonText: '确定',
			cancelButtonText: '取消'
		}).then(function(isConfirm) {
			if (isConfirm) {
                $scope.request("/user/delete?id="+userinfo.Id, null, function(data) {
					swal(data.Message, null, 'success');
			        self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
				});
			}
		}).catch(swal.noop);
	}

	$scope.add = function() {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '添加用户',
			area: (window.screen.width > 650 ? 650 : window.screen.width) + 'px',// '340px'], //宽高
			content: $("#edit"),
			cancel: function(index, layero) {
				setTimeout(function() {
					$("#edit").css("display", "none");
				}, 500);
				return true;
			}
		});
	}

	$scope.edit = function (userinfo) {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '编辑用户',
			area: (window.screen.width > 650 ? 650 : window.screen.width) + 'px',// '340px'], //宽高
			content: $("#edit"),
			cancel: function(index, layero) {
				setTimeout(function() {
					$("#edit").css("display", "none");
				}, 500);
				return true;
			}
		});
		$scope.userinfo=userinfo;
	}

	$scope.closeAll= function() {
		layer.closeAll();
		setTimeout(function() {
			$("#edit").css("display", "none");
		}, 500);
	}

	$scope.submit = function(userinfo) {
		$scope.request("/user/save", userinfo, function(data) {
			$scope.closeAll();
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
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

	$scope.resetPwd = function (row) {
		swal({
			title: '重置密码', 
			input: 'text',
			inputPlaceholder: '请输入新密码',
			showCancelButton: true,
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
			inputValidator: function (value) {
				return new Promise(function (resolve, reject) {
						resolve();
				});
			},
			preConfirm: function (value) {
				return new Promise(function (resolve) {
					$scope.request("/user/ResetPassword", {
						name: row.Username,
						pwd:value
					}, function (res) {
						resolve(res);
					});
				});
			}
		}).then(function (result) {
			if (result) {
				if (result.Success) {
					swal(result.Message, "", "success");
                } else {
					swal(result.Message, "", "error");
				}
			}
		}).catch(swal.noop);
	}
}]);