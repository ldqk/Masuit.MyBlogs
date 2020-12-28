myApp.controller("seminar", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
    var self = this;
    $scope.paginationConf = {
		currentPage: $scope.currentPage ? $scope.currentPage : 1,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function () {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	this.GetPageData = function (page, size) {
		$http.post("/seminar/getpagedata", {
			page,
			size
		}).then(function (res) {
			$scope.paginationConf.currentPage = page;
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
	self.del = function (row) {
		swal({
			title: "确认删除专题【"+row.Title+"】吗？",
			text: row.Name,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/seminar/delete", {
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
	$scope.save = function (row) {
		if (row == null) {
			row = {};
			row.Title = "";
			row.SubTitle = "";
			row.Description = "";
		}
		swal({
			title: '添加专题',
			html:
				'<div class="input-group"><span class="input-group-addon">专题名称： </span><div class="fg-line"><input id="title" type="text" class="form-control input-lg" autofocus placeholder="请输入专题名称" value="'+row.Title+'"></div></div>' +
			'<div class="input-group"><span class="input-group-addon">子标题： </span><div class="fg-line"><input id="subtitle" type="text" class="form-control input-lg" placeholder="请输入专题子标题" value="' + row.SubTitle +'"></div></div>' +
			'<div class="input-group"><span class="input-group-addon">专题描述：</span><div class="fg-line"><textarea id="desc" rows="5" class="form-control" placeholder="请输入专题描述" value="' + row.Description + '">' + row.Description +'</textarea></div></div>',
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
			preConfirm: function () {
				return new Promise(function (resolve, reject) {
					row.Title= $('#title').val();
					row.SubTitle= $('#subtitle').val();
					row.Description= $('#desc').val();
					$http.post("/Seminar/save", row).then(function (res) {
						resolve(res.data);
						self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
					},function (error) {
						reject("服务请求失败！");
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