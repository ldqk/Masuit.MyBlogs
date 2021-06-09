myApp.controller("partner", ["$scope", "$http", "$timeout","NgTableParams", function ($scope, $http, $timeout,NgTableParams) {
    var self = this;
	$scope.isAdd = true;
	$scope.allowUpload=false;
	$scope.partner = {};
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
		$http.post("/partner/getpagedata", {
			page,
			size,
			kw: $scope.kw
		}).then(function(res) {
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$("div[ng-table-pagination]").remove();
			self.tableParams = new NgTableParams({
				count: 50000
			}, {
				filterDelay: 0,
				dataset: res.data.Data
			});
			self.data = res.data.Data;
		});
	}
    $('.ui.dropdown.types').dropdown({
		onChange: function (value) {
			$scope.partner.Types = value;
		}
	});
    $scope.getCategory = function () {
		$http.post("/category/getcategories", null).then(function (res) {
			var data = res.data;
			if (data.Success) {
				$scope.cat = data.Data;
				$('.ui.dropdown.category').dropdown({
					onChange: function (value) {
						$scope.partner.CategoryIds = value;
					},
					message: {
						maxSelections: '最多选择 {maxCount} 项',
						noResults: '无搜索结果！'
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
	$scope.remove = function(partner) {
		layer.closeAll();
		swal({
			title: '确定移除这条广告吗？',
			text: partner.Title,
			type: 'warning',
			showCancelButton: true,
			confirmButtonColor: '#3085d6',
			cancelButtonColor: '#d33',
			confirmButtonText: '确定',
			cancelButtonText: '取消'
		}).then(function(isConfirm) {
			if (isConfirm) {
				$scope.request("/partner/delete/"+partner.Id, null, function(data) {
					swal(data.Message, null, 'success');
			        self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
				});
			}
		}).catch(swal.noop);
	}
	$scope.add = function() {
		$scope.partner = {
			ExpireTime:"2099-12-31"
        };
        $scope.isAdd = true;
		$scope.allowUpload=false;
		layer.open({
			type: 1,
			zIndex: 20,
			title: '添加广告推广',
			area: (window.screen.width > 650 ? 650 : window.screen.width) + 'px',// '340px'], //宽高
			content: $("#edit"),
			cancel: function(index, layero) {
				setTimeout(function() {
					$("#edit").css("display", "none");
				}, 500);
				return true;
			}
		});
        $timeout(function () {
		    $('.ui.dropdown.category').dropdown('clear');
		    $('.ui.dropdown.types').dropdown('clear');
		}, 10);
	}

	$scope.edit = function (item) {
		$scope.partner = angular.copy(item);
		$scope.partner.ExpireTime=$scope.partner.ExpireTime == null?"2099-12-31":$scope.partner.ExpireTime;
		$scope.isAdd = false;
		$scope.allowUpload=false;
		layer.closeAll();
        $timeout(function () {
		    $('.ui.dropdown.category').dropdown('clear');
		    $('.ui.dropdown.types').dropdown('clear');
			$('.ui.dropdown.category').dropdown('set selected', (item.CategoryIds+"").split(','));
		    $('.ui.dropdown.types').dropdown('set selected', item.Types.split(','));
		}, 10);
		layer.open({
			type: 1,
			zIndex: 20,
			title: '保存广告',
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

	$scope.copy = function (item) {
		$scope.partner = angular.copy(item);
		delete $scope.partner.Id;
		$scope.partner.ExpireTime=$scope.partner.ExpireTime == null?"2099-12-31":$scope.partner.ExpireTime;
		$scope.isAdd = true;
		$scope.allowUpload=false;
		layer.closeAll();
        $timeout(function () {
		    $('.ui.dropdown.category').dropdown('clear');
		    $('.ui.dropdown.types').dropdown('clear');
			$('.ui.dropdown.category').dropdown('set selected', (item.CategoryIds+"").split(','));
		    $('.ui.dropdown.types').dropdown('set selected', item.Types.split(','));
		}, 10);
		layer.open({
			type: 1,
			zIndex: 20,
			title: '复制广告推广',
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

	$scope.closeAll= function() {
		layer.closeAll();
		setTimeout(function() {
			$("#edit").css("display", "none");
		}, 500);
	}

	$scope.submit = function(partner) {
		if ($scope.isAdd) {
			partner.Id = 0;
		}
		$scope.request("/partner/save", partner, function(data) {
			$scope.closeAll();
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
			$scope.partner.ImageUrl = "";
			$scope.partner.Description = "";
		});
	}
	$scope.uploadImage = function(field) {
        $("#uploadform").ajaxSubmit({
			url: "/Upload",
			type: "post",
			success: function(data) {
				document.getElementById("uploadform").reset();
				$scope.$apply(function () {
					$scope.partner[field] = data.Data;
                    layer.close(layer.index);
			    });
			}
		});
    };
	
	$scope.upload = function(field) {
		$scope.imgField=field;
        layer.open({
			type: 1,
			zIndex: 20,
			title: '上传图片',
			area: [(window.screen.width > 300 ? 300 : window.screen.width) + 'px', '80px'], //宽高
			content: $("#img-upload"),
			cancel: function(index, layero) {
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

    $scope.changeState= function(row) {
        $scope.request("/partner/ChangeState/"+row.Id, null, function(data) {
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
		$('.ui.dropdown.types').dropdown('clear');
		$('.ui.dropdown.types').dropdown('set selected', item.Types.split(','));
		layer.open({
			type: 1,
			zIndex: 20,
            offset: '50px',
			title: item.Title,
			area: (window.screen.width > 850 ? 850 : window.screen.width) + 'px',// '340px'], //宽高
			content: $("#detail"),
			cancel: function(index, layero) {
				return true;
			}
		});
	}
	jeDate('#timespan',{
		isinitVal: true,
		festival: true,
		isTime: true,
		ishmsVal: true,
		format: 'YYYY-MM-DD hh:mm:ss',
		minDate: new Date().Format("yyyy-MM-dd 00:00:00"),
		maxDate: '2099-12-31 23:59:59',
		donefun: function (obj) {
			$scope.partner.ExpireTime = obj.val;
		}
	});
}]);