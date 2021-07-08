myApp.controller("postlist", ["$scope", "$http", "NgTableParams", "$timeout", function ($scope, $http, NgTableParams, $timeout) {
    var self = this;
	self.stats = [];
	self.data = {};
	$scope.kw = "";
	$scope.orderby = 1;
	$scope.CategoryId = "";
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

	$('.orderby').dropdown('set value', $scope.orderby);
	$('.orderby').dropdown({
		allowAdditions: false,
		onChange: function (value) {
			$scope.orderby = value;
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	});
	$http.post("/post/Statistic").then(function(res) {
		if(res.data.Success) {
			$scope.agg = res.data.Data;
		} else {
			window.notie.alert({
				type:3,
				text:res.data.Message,
				time:4
			});
		}
	});

	$http.post("/category/getcategories", null).then(function (res) {
		var data = res.data;
		if (data.Success) {
			$scope.cat = data.Data;
			$('.ui.dropdown.category').dropdown({
				onChange: function (value) {
					$scope.CategoryId = value;
					self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
				},
				message: {
					maxSelections: '最多选择 {maxCount} 项',
					noResults: '无搜索结果！'
				}
			});
			
            var params = JSON.parse(localStorage.getItem("postlist-params"));
	        if (params) {
		        $scope.kw = params["kw"];
		        $scope.paginationConf.currentPage= params["page"];
				$timeout(() => {
                    $('.category').dropdown('set selected', params["cid"]);
	                $('.orderby').dropdown('set selected', params["orderby"]);
                },10);
            }
		} else {
			window.notie.alert({
				type: 3,
				text: '获取文章分类失败！',
				time: 4
			});
		}
	});

	this.GetPageData = function (page, size) {
		var params={ page, size, kw: $scope.kw, orderby:$scope.orderby, cid:$scope.CategoryId };
		$http.post("/post/getpagedata", params).then(function(res) {
			$scope.paginationConf.totalItems = res.data.TotalCount;
			$("div[ng-table-pagination]").remove();
			self.tableParams = new NgTableParams({ count: 50000 }, {
				filterDelay: 0,
				dataset: res.data.Data
			});
			self.data = res.data.Data;
			Enumerable.From(res.data.Data).Select(e => e.Status).Distinct().ToArray().map(function(item, index, array) {
				self.stats.push({
					id: item,
					title: item
				});
			});
			self.stats = Enumerable.From(self.stats).Distinct().ToArray();
			
			localStorage.setItem("postlist-params",JSON.stringify(params));
		});
	}

	self.del = function(row) {
		swal({
			title: "确认删除这篇文章吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/post/delete", {
				id: row.Id
			}, function(data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
			});
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}, function() {
		}).catch(swal.noop);
	}
	self.truncate = function(row) {
		swal({
			title: "确认要彻底删除这篇文章吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/post/truncate", {
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
	self.pass = function(row) {
		$scope.request("/post/pass", row, function(data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			self.stats = [];
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		});
	}
	self.restore = function(row) {
		$scope.request("/post/restore", {
			id: row.Id
		}, function(data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			self.stats = [];
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		});
	}
	self.fixtop = function(id) {
		$scope.request("/post/Fixtop", {
			id: id
		}, function(data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			self.stats = [];
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
	
    $scope.toggleDisableComment= function(row) {
        $scope.request("/post/DisableComment", {
			id: row.Id
		}, function(data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
		});
    }

    $scope.toggleDisableCopy= function(row) {
        $scope.request("/post/DisableCopy", {
			id: row.Id
		}, function(data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
		});
    }

    $scope.rssSwitch= function(id) {
        $scope.request("/post/"+id+"/rss-switch",null, function(data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
		});
    }
}]);
myApp.controller("writeblog", ["$scope", "$http", "$timeout","$location", function ($scope, $http, $timeout,$location) {
	clearInterval(window.interval);
	$scope.post = {
		Title: "",
		schedule: false,
		Content: "",
		CategoryId: 1,
		Label: "",
		Seminars: "",
		Keyword:""
	};
	
	$scope.post.Author = $scope.user.NickName || $scope.user.Username;
	$scope.post.Email = $scope.user.Email;
	var refer = $location.search()['refer'];
    if (refer) {
        $scope.request("/post/get", {
		    id: refer
	    }, function (data) {
            $scope.post = data.Data;
			delete $scope.post.Id;
            $('.ui.dropdown.keyword').dropdown({
			    allowAdditions: true,
			    onChange: function(value) {
				    $scope.post.Keyword = value;
			    }
		    });
		    $('.ui.dropdown.keyword').dropdown('set selected', $scope.post.Keyword.split(','));
	    });
    }
	$scope.getCategory = function () {
        $http.post("/category/getcategories", null).then(function (res) {
            var data = res.data;
			if (data.Success) {
				$scope.cat = data.Data;
				$('.ui.dropdown.category').dropdown({
					onChange: function (value) {
						$scope.post.CategoryId = value;
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
	$scope.request("/post/gettag", null, function(res) {
		$scope.Tags = res.Data;
		$('.ui.dropdown.tags').dropdown({
			allowAdditions: true,
			maxSelections: 5,
			message: {
				maxSelections: '最多选择 {maxCount} 项'
			},
			onChange: function(value) {
				$scope.post.Label = value;
			}
		});
	});
	$scope.request("/seminar/getall", null, function(res) {
		$scope.Seminars = res.Data;
		$('.ui.dropdown.seminar').dropdown({
			allowAdditions: false,
			onChange: function(value) {
				$scope.post.Seminars = value;
			}
		});
		$timeout(function () {
			$('.ui.dropdown.category').dropdown("set selected", [1]);
		}, 100);
	});
	
	$('.ui.dropdown.keyword').dropdown({
		allowAdditions: true,
		onChange: function(value) {
			$scope.post.Keyword = value;
		}
	});
	//上传Word文档
	$scope.upload = function() {
        $("#docform").ajaxSubmit({
			url: "/Upload/UploadWord",
			type: "post",
			success: function(data) {
                console.log(data);
				if (data.Success) {
					window.notie.alert({
						type: 1,
						text: '文档上传成功!',
						time: 2
					});
					$scope.$apply(function() {
						$scope.post.Content = data.Data.Content;
						$scope.post.Title = data.Data.Title;
					});
					layer.closeAll();
				} else {
					window.notie.alert({
						type: 3,
						text: data.Message,
						time: 4
					});
				}
			}
		});
		$scope.selectFile = false;
	}

	//文件上传
	$scope.showupload = function() {
		layui.use("layer", function() {
			var layer = layui.layer;
			layer.open({
				type: 1,
				title: '上传Word文档',
				area: ['420px', '150px'], //宽高
				content: $("#docfile")
			});
		});
	}

	//异步提交表单开始
	$scope.submit = function(post) {
		Object.keys(post).forEach(key => post[key] == undefined||post[key] == null ? delete post[key] : '');
		if (!post.Label) {
			post.Label = null;
		}
		if (post.Title.trim().length <= 2 || post.Title.trim().length > 128) {
			window.notie.alert({
				type: 3,
				text: '文章标题必须在2到128个字符以内！',
				time: 4
			});
			
			return;
		}
		$http.post("/Post/write", post).then(function(res) {
			var data = res.data;
            if (data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				$scope.post.Content = "";
				$scope.post.Title = "";
				clearInterval(window.interval);
				localStorage.removeItem("write-post-draft");
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		});
	}

	// 定时发布
	$scope.Scheduled= function() {
        if ($scope.post.schedule) {
            jeDate('#timespan',{
				isinitVal: true,
				festival: true,
				isTime: true,
				ishmsVal: true,
				format: 'YYYY-MM-DD hh:mm:ss',
				minDate: new Date().Format("yyyy-MM-dd 00:00:00"),
				maxDate: '2099-06-16 23:59:59',
				donefun: function (obj) {
					$scope.post.timespan = obj.val;
				},
		        clearfun: function(elem, val) {
                    delete $scope.post.timespan;
                }
			});
        }
	}

	//检查草稿
    if (localStorage.getItem("write-post-draft")) {
        notie.confirm({
            text: "检查到上次有未提交的草稿，是否加载？",
            submitText: "确定",
            cancelText: "取消",
            position: "bottom",
            submitCallback: function () {
                $scope.post = JSON.parse(localStorage.getItem("write-post-draft"));
                $scope.$apply();
                $timeout(function () {
                    $('.ui.dropdown.category').dropdown('set selected', [$scope.post.CategoryId]);
                    if ($scope.post.Label) {
                        $('.ui.dropdown.tags').dropdown('set selected', $scope.post.Label.split(','));
                    }
                    if ($scope.post.Keyword) {
						$('.ui.dropdown.keyword').dropdown('set selected', $scope.post.Keyword.split(','));
                    }
                    if ($scope.post.Seminars) {
                        $('.ui.dropdown.seminar').dropdown('set selected', $scope.post.Seminars.split(','));
                    }
                }, 10);
                window.interval = setInterval(function () {
		            localStorage.setItem("write-post-draft",JSON.stringify($scope.post));
	            },5000);
            },
            cancelCallback: function() {
                window.interval = setInterval(function () {
		            localStorage.setItem("write-post-draft",JSON.stringify($scope.post));
	            },5000);
            }
        });
    } else {
        window.interval = setInterval(function () {
		    localStorage.setItem("write-post-draft",JSON.stringify($scope.post));
	    },5000);
    }
}]);
myApp.controller("postedit", ["$scope", "$http", "$location", "$timeout", function ($scope, $http, $location, $timeout) {
	$scope.id = $location.search()['id'];
	
	$scope.reserve = true;
	$scope.request("/post/get", {
		id: $scope.id
	}, function (data) {
		$scope.post = data.Data;
		$scope.request("/post/gettag", null, function (res) {
			$scope.Tags = res.Data;
			$('.ui.dropdown.tags').dropdown({
				allowAdditions: true,
				maxSelections: 5,
				message: {
					maxSelections: '最多选择 {maxCount} 项'
				},
				onChange: function (value) {
					$scope.post.Label = value;
				}
			});
		});
		$('.ui.dropdown.tags').dropdown('set value', $scope.post.Label);
		$scope.request("/seminar/getall", null, function (res) {
			$scope.Seminars = res.Data;
			$('.ui.dropdown.seminar').dropdown({
				allowAdditions: true,
				maxSelections: 5,
				message: {
					maxSelections: '最多选择 {maxCount} 项'
				},
				onChange: function (value) {
					$scope.post.Seminars = value;
				}
			});
			if ($scope.post.Seminars) {
				$timeout(function () {
					$('.ui.dropdown.seminar').dropdown('set selected', $scope.post.Seminars.split(','));
				}, 10);
			}
		});
		$scope.getCategory();
		
		$('.ui.dropdown.keyword').dropdown({
			allowAdditions: true,
			onChange: function(value) {
				$scope.post.Keyword = value;
			}
		});
		$('.ui.dropdown.keyword').dropdown('set selected', $scope.post.Keyword.split(','));
	});
	$scope.getCategory = function () {
        $http.post("/category/getcategories", null).then(function (res) {
            var data = res.data;
			if (data.Success) {
				$scope.cat = data.Data;
				$('.ui.dropdown.category').dropdown({
					onChange: function (value) {
						$scope.post.CategoryId = value;
					},
					message: {
						maxSelections: '最多选择 {maxCount} 项',
						noResults: '无搜索结果！'
					}
				});
				$timeout(function () {
					$('.ui.dropdown.category').dropdown('set selected', [$scope.post.CategoryId]);
				}, 10);
			} else {
				window.notie.alert({
					type: 3,
					text: '获取文章分类失败！',
					time: 4
				});
			}
		});
	}
	//上传Word文档
	$scope.upload = function () {
		
		$("#docform").ajaxSubmit({
			url: "/Upload/UploadWord",
			type: "post",
			success: function (data) {
				
				console.log(data);
				if (data.Success) {
					window.notie.alert({
						type: 1,
						text: '文档上传成功!',
						time: 2
					});
					$scope.$apply(function () {
						$scope.post.Content = data.Data.Content;
						$scope.post.Title = data.Data.Title;
					});
					layer.closeAll();
				} else {
					window.notie.alert({
						type: 3,
						text: data.Message,
						time: 4
					});
				}
			}
		});
		$scope.selectFile = false;
	}
	//文件上传
	$scope.showupload = function () {
		layui.use("layer", function () {
			var layer = layui.layer;
			layer.open({
				type: 1,
				title: '上传Word文档',
				area: ['420px', '150px'], //宽高
				content: $("#docfile")
			});
		});
	}

	//发布
	$scope.submit = function (post) {
		Object.keys(post).forEach(key => post[key] == undefined||post[key] == null ? delete post[key] : '');
		
		if (!post.Label) {
			post.Label = null;
		}
		if (post.Title.trim().length <= 2 || post.Title.trim().length > 128) {
			window.notie.alert({
				type: 3,
				text: '文章标题必须在2到128个字符以内！',
				time: 4
			});
			
			return;
		}
		if (post.Author.trim().length <= 0 || post.Author.trim().length > 20) {
			window.notie.alert({
				type: 3,
				text: '再怎么你也应该留个合理的名字吧，非主流的我可不喜欢！',
				time: 4
			});
			
			return;
		}
		if (!/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/
			.test(post.Email.trim())) {
			window.notie.alert({
				type: 3,
				text: '请输入正确的邮箱格式！',
				time: 4
			});
			
			return;
		}
		if (post.Content.length < 20 || post.Content.length > 1000000) {
			window.notie.alert({
				type: 3,
				text: '文章内容过短或者超长，请修改后再提交！',
				time: 4
			});
			
			return;
		}
		post.reserve = $scope.reserve;
		$http.post("/Post/edit", post).then(function (res) {
			var data = res.data;
			
			if (data.Success) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				$scope.post = data.Data;
				clearInterval(window.interval);
				localStorage.removeItem("post-draft-"+$scope.id);
			} else {
				window.notie.alert({
					type: 3,
					text: data.Message,
					time: 4
				});
			}
		});
	}
	
	//检查草稿
    if (localStorage.getItem("post-draft-" + $scope.id)) {
        notie.confirm({
            text: "检查到上次有未提交的草稿，是否加载？",
            submitText: "确定",
            cancelText: "取消",
            position: "bottom",
            submitCallback: function () {
                $scope.post = JSON.parse(localStorage.getItem("post-draft-" + $scope.id));
                $scope.$apply();
                $timeout(function () {
                    $('.ui.dropdown.category').dropdown('set selected', [$scope.post.CategoryId]);
					if ($scope.post.Label) {
                        $('.ui.dropdown.tags').dropdown('set selected', $scope.post.Label.split(','));
                    }
                    if ($scope.post.Keyword) {
						$('.ui.dropdown.keyword').dropdown('set selected', $scope.post.Keyword.split(','));
                    }
                    if ($scope.post.Seminars) {
                        $('.ui.dropdown.seminar').dropdown('set selected', $scope.post.Seminars.split(','));
                    }
                }, 10);
                window.interval = setInterval(function () {
			        localStorage.setItem("post-draft-"+$scope.id,JSON.stringify($scope.post));
		        },5000);
            },
            cancelCallback: function() {
                window.interval = setInterval(function () {
			        localStorage.setItem("post-draft-"+$scope.id,JSON.stringify($scope.post));
		        },5000);
            }
        });
    } else {
        window.interval = setInterval(function () {
			localStorage.setItem("post-draft-"+$scope.id,JSON.stringify($scope.post));
		},5000);
    }
}]);
myApp.controller("category", ["$scope", "$http", "NgTableParams", function ($scope, $http, NgTableParams) {
	var self = this;
	var cats = [];
	self.data = {};
	this.load = function() {
		$scope.request("/category/GetCategories", null, function(res) {
			self.tableParams = new NgTableParams({}, {
				filterDelay: 0,
				dataset: res.Data
			});
			cats = res.Data;
		});
	}
	self.load();
	self.del = function(row) {
		var select = {};
		Enumerable.From(cats).Where(e => e.Name != row.Name).Select(e => {
			return {
				id: e.Id,
				name: e.Name
			}
		}).Distinct().ToArray().map(function(item, index, array) {
			select[item.id] = item.name;
		});
		swal({
			title: '确定删除这个分类吗？',
			text: "删除后将该分类下的所有文章移动到：",
			input: 'select',
			inputOptions: select,
			inputPlaceholder: '请选择分类',
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			inputValidator: function(value) {
				return new Promise(function(resolve, reject) {
					if (value == '') {
						reject('请选择一个分类');
					} else {
						resolve();
					}
				});
			}
		}).then(function(result) {
			if (result) {
				if (row.Id == 1) {
					swal({
						type: 'error',
						html: "默认分类不能被删除！"
					});
				} else {
					$scope.request("/category/delete", {
						id: row.Id,
						cid:result
					}, function(data) {
						swal({
							type: 'success',
							html: data.Message
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
				}
			}
		}).catch(swal.noop);
	}
	self.edit = function (row) {
		swal({
			title: '修改分类',
			html:
			'<div class="input-group"><span class="input-group-addon">分类名称： </span><input id="name" type="text" class="form-control input-lg" autofocus placeholder="请输入新的分类名" value="'+row.Name+'"></div>' +
			'<div class="input-group"><span class="input-group-addon">分类描述： </span><input id="desc" type="text" class="form-control input-lg" placeholder="请输入分类描述" value="'+row.Description+'"></div>',
			showCloseButton: true,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			allowOutsideClick: false,
			preConfirm: function () {
				return new Promise(function (resolve, reject) {
					row.Name = $('#name').val();
					row.Description = $('#desc').val();
					$http.post("/category/edit", row).then(function (res) {
						if (res.data.Success) {
							resolve(res.data.Message);
						} else {
							reject(res.data.Message);
						}
					}, function (error) {
						reject("操作失败");
					});
				});
			}
		}).then(function (msg) {
			if (msg) {
				swal({
					type: 'success',
					html: msg
				});
				self.load();
			}
		}).catch(swal.noop);
	}
	self.add = function() {
		swal({
			title: '请输入分类名',
			input: 'text',
			inputPlaceholder: '请输入分类',
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			inputValidator: function(value) {
				return new Promise(function(resolve, reject) {
					if (value) {
						resolve();
					} else {
						reject('分类名不能为空');
					}
				});
			}
		}).then(function(result) {
			if (result) {
				$scope.request("/category/add", {
					Name: result
				}, function(data) {
					swal({
						type: 'success',
						html: data.Message
					});
					self.load();
				});
			}
		}).catch(swal.noop);
	}
}]);
myApp.controller("postpending", ["$scope", "$http", "NgTableParams", "$timeout", function ($scope, $http, NgTableParams, $timeout) {
	var self = this;
	
	$scope.kw = "";
	$scope.orderby = 1;
	$scope.paginationConf = {
		currentPage: $scope.currentPage ? $scope.currentPage : 1,
		//totalItems: $scope.total,
		itemsPerPage: 10,
		pagesLength: 25,
		perPageOptions: [1, 5, 10, 15, 20, 30, 40, 50, 100, 200],
		rememberPerPage: 'perPageItems',
		onChange: function() {
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage);
		}
	};
	this.GetPageData = function(page, size) {
		
		$http.post("/post/GetPending", {
			page,
			size,
			search:$scope.kw
		}).then(function(res) {
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
	self.del = function(row) {
		swal({
			title: "确认删除这篇文章吗？",
			text: row.Title,
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/post/delete", {
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
	self.pass = function(row) {
		$scope.request("/post/pass", row, function(data) {
			window.notie.alert({
				type: 1,
				text: data.Message,
				time: 4
			});
			self.stats = [];
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
			self.GetPageData($scope.paginationConf.currentPage, $scope.paginationConf.itemsPerPage, $scope.kw);
			_timeout = null;
		}, 500);
	}
	
	$scope.addToBlock= function(row) {
		swal({
			title: "确认添加恶意名单吗？",
			text: "将"+row.Email+"添加到恶意名单",
			showCancelButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			animation: true,
			allowOutsideClick: false,
			showLoaderOnConfirm: true,
			preConfirm: function () {
				return new Promise(function (resolve, reject) {
					$http.post("/post/block/"+row.Id).then(function(res) {
						resolve(res.data);
					}, function() {
						reject("请求服务器失败！");
					});
				});
			}
		}).then(function (data) {
			if (data.Success) {
				swal("添加成功",'','success');
			} else {
				swal("添加失败",'','error');
			}
		}).catch(swal.noop);
	}
}]);

myApp.controller("share", ["$scope", "NgTableParams", function ($scope, NgTableParams) {
	var self = this;
	self.data = {};
	this.load = function() {
		$scope.request("/share", null, function(res) {
			self.tableParams = new NgTableParams({}, {
				filterDelay: 0,
				dataset: res.Data
			});
			shares = res.Data;
		});
	}
	self.load();
	$scope.closeAll = function() {
		layer.closeAll();
		setTimeout(function() {
			$("#modal").css("display", "none");
		}, 500);
	}
	$scope.submit = function (share) {
		if (share.Id) {
			//修改
			$scope.request("/share/update", share, function (data) {
				swal(data.Message, null, 'info');
				$scope.share = {};
				$scope.closeAll();
				self.load();
			});
		}else {
			$scope.request("/share/add", share, function (data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 3
				});
				$scope.share = {};
				$scope.closeAll();
				self.load();
			});
		}
	}
	self.del = function(row) {
		swal({
			title: "确认删除这个分享吗？",
			text: row.Title,
			showCancelButton: true,
			showCloseButton: true,
			confirmButtonColor: "#DD6B55",
			confirmButtonText: "确定",
			cancelButtonText: "取消",
			showLoaderOnConfirm: true,
			animation: true,
			allowOutsideClick: false
		}).then(function() {
			$scope.request("/share/remove", {
				id: row.Id
			}, function(data) {
				window.notie.alert({
					type: 1,
					text: data.Message,
					time: 4
				});
				self.load();
			});
		}, function() {
		}).catch(swal.noop);
	}
	self.edit = function (row) {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '修改快速分享',
			area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',
			content: $("#modal"),
			success: function(layero, index) {
				$scope.share = row;
			},
			end: function() {
				$("#modal").css("display", "none");
			}
		});
	}
	self.add = function() {
		layer.open({
			type: 1,
			zIndex: 20,
			title: '添加快速分享',
			area: (window.screen.width > 600 ? 600 : window.screen.width) + 'px',
			content: $("#modal"),
			success: function(layero, index) {
				$scope.share = {};
			},
			end: function() {
				$("#modal").css("display", "none");
			}
		});
	}
}]);