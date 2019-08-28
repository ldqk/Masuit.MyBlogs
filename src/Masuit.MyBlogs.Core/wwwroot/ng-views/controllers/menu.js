myApp.controller("menu", ["$scope", "$http", "$timeout", function($scope, $http, $timeout) {
	window.hub.stop();
	$scope.loading();
	$scope.menu = {};
	$scope.init = function() {
		$scope.request("/menu/getmenus", null, function(data) {
			$scope.data = transData(data.Data, "Id", "ParentId", "nodes");
			$scope.collapse = true;
			$timeout(function() {
				$scope.expandAll();
			}, 0);
		});
	}
	var sourceId, destId, index, parent, sourceIndex;
	$scope.treeOptions = {
		beforeDrop: function(e) {
			index = e.dest.index;
			if (e.dest.nodesScope.$parent.$modelValue) {
				parent = e.dest.nodesScope.$parent.$modelValue; //找出父级元素
				if (parent.Url && parent.Url != "#") {
					swal("异常操作！", "菜单【" + parent.Name + "】是一个有链接的菜单，不能作为父级菜单", "error");
					return false;
				}
			}
		},
		dropped: function(e) {
			var dest = e.dest.nodesScope;
			destId = dest.$id;
			var pid = dest.node ? dest.node.id : 0; //pid
			var prev = null;
			var next = null;
			if (index > sourceIndex) {
				next = dest.$modelValue[index + 1], prev = dest.$modelValue[index];
			} else if (index < sourceIndex) {
				next = dest.$modelValue[index], prev = dest.$modelValue[index - 1];
			} else {
				next = dest.$modelValue[index];
			}
			var current = e.source.nodeScope.$modelValue;
			if (destId == sourceId) {
				if (index == sourceIndex) {
					//位置没改变
					return;
				}
				//同级内改变位置，找出兄弟结点，排序号更新
				if (prev || next) {
					//有多个子节点
					if (next) {
						//console.log("自己：", current, "后一个元素：", next);
						current.ParentId = pid;
						current.Sort = next.Sort - 1;
						$scope.request("/menu/save", current, function(data) {
							window.notie.alert({
								type: 1,
								text: data.Message,
								time: 3
							});
						});
					} else if (prev) {
						//console.log("自己：", current, "前一个元素：", prev);
						current.ParentId = pid;
						current.Sort = prev.Sort + 1;
						$scope.request("/menu/save", current, function (data) {
							window.notie.alert({
								type: 1,
								text: data.Message,
								time: 3
							});
						});
					}
				}
			} else {
				//层级位置改变
				if (parent) {
					//非顶级元素
					//找兄弟结点
					next = dest.$modelValue[index], prev = dest.$modelValue[index - 1];
					if (prev || next) {
						//有多个子节点
						if (next) {
							//console.log("自己：", current, "后一个元素：", next);
							current.ParentId = parent.Id;
							current.Sort = next.Sort - 1;
							$scope.request("/menu/save", current, function (data) {
								window.notie.alert({
									type: 1,
									text: data.Message,
									time: 3
								});
							});
						} else if (prev) {
							//console.log("自己：", current, "前一个元素：", prev);
							current.ParentId = parent.Id;
							current.Sort = prev.Sort + 1;
							$scope.request("/menu/save", current, function (data) {
								window.notie.alert({
									type: 1,
									text: data.Message,
									time: 3
								});
							});
						}
					} else {
						//只有一个元素
						//console.log("自己：", current, "父亲元素：", parent);
						current.ParentId = parent.Id;
						current.Sort = parent.Sort * 10;
						$scope.request("/menu/save", current, function (data) {
							window.notie.alert({
								type: 1,
								text: data.Message,
								time: 3
							});
						});
					}
				} else {
					//顶级元素
					sourceIndex = e.source.nodesScope.$parent.index();
					if (index < sourceIndex) {
						next = dest.$modelValue[index + 1], prev = dest.$modelValue[index];
					} else {
						next = dest.$modelValue[index], prev = dest.$modelValue[index - 1];
					}
					//console.log("后一个元素：", next, "前一个元素：", prev, "自己：", current);
					if (next) {
						current.ParentId = pid;
						current.Sort = next.Sort - 1;
						$scope.request("/menu/save", current, function (data) {
							window.notie.alert({
								type: 1,
								text: data.Message,
								time: 3
							});
						});
					} else if (prev) {
						current.ParentId = pid;
						current.Sort = prev.Sort + 1;
						$scope.request("/menu/save", current, function (data) {
							window.notie.alert({
								type: 1,
								text: data.Message,
								time: 3
							});
						});
					}
				}
				parent = null;
			}
		},
		dragStart: function(e) {
			sourceId = e.dest.nodesScope.$id;
			sourceIndex = e.dest.index;
		}
	};
	$scope.findNodes = function () {
		
	};
	$scope.visible = function (item) {
		return !($scope.query && $scope.query.length > 0 && item.Name.indexOf($scope.query) == -1);
	};
	$scope.menu = {};
	$scope.newItem = function() {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '修改菜单信息',
			area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',// '340px'], //宽高
			content: $("#modal"),
			success: function(layero, index) {
				$scope.menu = {};
			},
			end: function() {
				$("#modal").css("display", "none");
			}
		});
		var nodeData = $scope.data[$scope.data.length - 1];
		$scope.menu.Sort = nodeData.Sort + (nodeData.nodes.length + 1) * 10;
		$scope.menu.ParentId  = 0;
		$scope.menu.MenuType  = 0;
	};
	$scope.submenu = {};

	$scope.closeAll = function() {
		layer.closeAll();
		setTimeout(function() {
			$("#modal").css("display", "none");
		}, 500);
	}
	$scope.newSubItem = function (scope) {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '修改菜单信息',
			area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',// '340px'], //宽高
			content: $("#modal"),
			success: function(layero, index) {
				$scope.menu = {};
			},
			end: function() {
				$("#modal").css("display", "none");
			}
		});
		var nodeData = scope.$modelValue;
		$scope.submenu = nodeData;
		if (nodeData.Url && nodeData.Url != "#") {
			swal("异常操作！", "菜单【" + nodeData.Name + "】是一个有链接的菜单，不能作为父级菜单", "error");
			return false;
		}
		$scope.menu.Sort = (nodeData.Sort + nodeData.nodes.length + 1) * 10;
		$scope.menu.MenuType  = nodeData.MenuType;
		$scope.menu.ParentId = nodeData.Id;
	};
	$scope.expandAll = function() {
		if ($scope.collapse) {
			$scope.$broadcast('angular-ui-tree:collapse-all');
		} else {
			$scope.$broadcast('angular-ui-tree:expand-all');
		}
		$scope.collapse = !$scope.collapse;
	};
	
	$scope.del = function(scope) {
		var model = scope.$nodeScope.$modelValue;
		var id = model.Id;
		swal({
			title: "确认删除这个菜单吗？",
			text: model.Name,
			showCancelButton: true,
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/menu/delete", {
				id: id
			}, function(data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				scope.remove();
			});
		}, function() {
		}).catch(swal.noop);
	}
	
	$scope.getMenuType = function() {
		$scope.request("/menu/getmenutype", null, function(data) {
			$scope.MenuType = data.Data;
		});
	}
	$scope.edit= function(menu) {
		$scope.menu = menu;
		//Custombox.open({
		//	target: '#menu',
		//	zIndex: 100,
		//	height: 900,
		//	close: function () {
		//		$scope.menu = {};
		//	},
		//	overlayOpacity: 0.5
		//});
		layer.open({
			type: 1,
			zIndex: 20,
			title: '修改菜单信息',
			area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',// '340px'], //宽高
			//area: ['600px', '270px'], //宽高
			content: $("#modal"),
			success: function(layero, index) {
				$scope.menu = menu;
			},
			end: function() {
				$("#modal").css("display", "none");
			}
		});
	}
	
	$scope.submit = function (menu) {
		if (menu.Icon==""||menu.Icon==null||menu.Icon==undefined) {
			menu.Icon = null;
		}
		if (menu.Id) {
			//修改
			$scope.request("/menu/save", menu, function (data) {
				swal(data.Message, null, 'info');
				$scope.menu = {};
				//Custombox.close();
				$scope.closeAll();
			});
		}else {
			//添加
			$scope.request("/menu/save", menu, function (data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 3
				});
				$scope.menu = {};
				$scope.closeAll();
				$scope.init();
			});
		}
	}
	$scope.uploadImage = function() {
		$scope.loading();
        $("#uploadform").ajaxSubmit({
			url: "/Upload",
			type: "post",
			success: function(data) {
				$scope.loadingDone();
				document.getElementById("uploadform").reset();
				$scope.$apply(function () {
			     　$scope.allowUpload=false;
					$scope.menu.Icon = data.Data;
			    });
			}
		});
    };
	$scope.upload = function() {
		$scope.allowUpload=true;
	}
	$scope.init();
	$scope.getMenuType();
}]);