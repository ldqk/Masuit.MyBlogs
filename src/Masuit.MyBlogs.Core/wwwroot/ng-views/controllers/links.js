myApp.controller("links", ["$scope", "$http", "NgTableParams", function($scope, $http, NgTableParams) {
	var self = this;
	var source = [];
	this.load = function() {
		$http.post("/links/get", null).then(function(res) {
			self.tableParams = new NgTableParams({}, {
				filterDelay: 0,
				dataset: res.data.Data
			});
			source = res.data.Data;
		});
	};
	this.load();

	self.del = function(row) {
		swal({
			title: "确认删除这条友情链接吗？",
			text: row.Name,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/links/delete", {
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
	self.cancel = function (row, rowForm) {
		var originalRow = resetRow(row, rowForm);
		angular.extend(row, originalRow);
	};
	self.save = function (row, rowForm) {
		swal({
			title: "确认修改" + row.Name+"地址为：",
			text: row.Url,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function () {
			$scope.request("/links/edit", row, function (data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				var originalRow = resetRow(row, rowForm);
				angular.extend(originalRow, row);
			});
		}, function () {
		}).catch(swal.noop);
	};
	function resetRow(row, rowForm) {
		row.isEditing = false;
		rowForm.$setPristine();
		self.tableTracker.untrack(row);
		return _.findWhere(source, function(r) {
			return r.id === row.id;
		});
	}
	$scope.add = function () {
		swal({
			title: '添加联系方式',
			html:'<div class="input-group"><span class="input-group-addon">链接名称</span><input id="title" class="form-control" autofocus placeholder="链接名称"></div>' +
			'<div class="input-group"><span class="input-group-addon">地址</span><input id="url" class="form-control" placeholder="地址"></div>',
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false,
			preConfirm: function () {
				return new Promise(function (resolve) {
					$scope.request("/links/add", {
						Name: $('#title').val(),
						Url: $('#url').val()
					}, function (res) {
						resolve(res);
					});
				});
			}
		}).then(function (result) {
			if (result) {
				if (result.Success) {
					swal(result.Message, "", "success");
					self.load();
				} else {
					swal(result.Message, "", "error");
				}
			}
		}).catch(swal.noop);
	}
	self.check= function(link) {
		$scope.request("/links/check", {
			link:link.Url
		}, function (data) {
			layer.tips(data.Message, '#link-'+link.Id, {
                tips: [1, '#3595CC'],
                time: 5000
            });
		});
	}
	$scope.toggleWhite= function(row) {
		$scope.request("/links/ToggleWhitelist", {
			id:row.Id
		}, function (data) {
        });
	}
	$scope.toggleState= function(row) {
		$scope.request("/links/Toggle", {
			id:row.Id
		}, function (data) {
			
		});
	}

	$scope.toggleRecommend = function(row) {
		$scope.request("/links/ToggleRecommend", {
			id:row.Id
		}, function (data) {
        });
	}
}]);