myApp.controller("subscribe", ["$scope", "$http", "NgTableParams","$timeout", function ($scope, $http, NgTableParams,$timeout) {
	window.hub.stop();
	var self = this;
	var source = [];
	self.stats = [];
	$scope.loading();
	$scope.paginationConf = {
		currentPage:1,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function () {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	$scope.request("/system/getstatus", null, function (res) {
		$scope.AllStatus = res.Data;
		res.Data.map(function (item, index, array) {
			self.stats.push({
				id: item.e,
				title: item.name
			});
		});
		self.stats = Enumerable.From(self.stats).Distinct().ToArray();
	});
	$scope.SubscribeTypes = [{key:0,value:"广播订阅"},{key:1,value:"文章密码获取"}];
	this.GetPageData = function (page, size) {
		$scope.loading();
		$http.post("/subscribe/getpagedata", {
			page,
			size,
			search:$scope.query
		}).then(function (res) {
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
	self.del = function (row) {
		swal({
			title: "确认删除这条订阅吗？",
			text: row.Name,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/subscribe/delete", {
				id: row.Id
			}, function (data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
			});
			_.remove(self.tableParams.settings().dataset, function (item) {
				return row === item;
			});
			self.tableParams.reload().then(function (data) {
				if (data.length === 0 && self.tableParams.total() > 0) {
					self.tableParams.page(self.tableParams.page() - 1);
					self.tableParams.reload();
				}
			});
		}, function () {
		}).catch(swal.noop);
	}
	self.cancel = function (row, rowForm) {
		var originalRow = resetRow(row, rowForm);
		angular.extend(row, originalRow);
	};
	self.save = function (row, rowForm) {
		swal({
			title: "确认修改?",
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/subscribe/save", row, function (data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				var originalRow = resetRow(row, rowForm);
				angular.extend(originalRow, row);
				self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
			});
		}, function () {
		}).catch(swal.noop);
	};
	function resetRow(row, rowForm) {
		row.isEditing = false;
		rowForm.$setPristine();
		self.tableTracker.untrack(row);
		return _.findWhere(source, function (r) {
			return r.id === row.id;
		});
	}
	$scope.add = function () {
		swal({
			title: '添加订阅', 
			input: 'email',
			inputPlaceholder: '请输入邮箱地址',
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
					if (/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value)) {
						resolve();
					} else {
						reject('邮箱格式不正确！');
					}
				});
			},
			preConfirm: function (value) {
				return new Promise(function (resolve) {
					$scope.request("/subscribe/save", {
						Email: value,
						Status:7
					}, function (res) {
						resolve(res);
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
	}
}]);