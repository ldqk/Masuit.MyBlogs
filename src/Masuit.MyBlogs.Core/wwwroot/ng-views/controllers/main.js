myApp.controller('main', ["$timeout", "$state", "$scope", "$http", function($timeout, $state, $scope, $http) {
	$scope.post = {};
	Waves.init();
	iziToast.settings({
		timeout:15000,
		// position: 'center',
		// imageWidth: 50,
		pauseOnHover:true,
		// resetOnHover: true,
		close:true,
		progressBar:true,
		// layout: 1,
		// balloon: true,
		// target: '.target',
		// icon: 'material-icons',
		// iconText: 'face',
		// animateInside: false,
		// transitionIn: 'flipInX',
		// transitionOut: 'flipOutX',
	});
	localStorage.setItem('ma-layout-status', 1);
	if(/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
		angular.element('html').addClass('ismobile');
	}
	
	this.sidebarToggle = {
		left:false,
		right:false
	}

	this.layoutType = localStorage.getItem('ma-layout-status');

	this.$state = $state;

	this.sidebarStat = function(event) {
		if(!angular.element(event.target).parent().hasClass('active')) {
			this.sidebarToggle.left = false;
		}
	}

	this.listviewSearchStat = false;

	this.lvSearch = function() {
		this.listviewSearchStat = true;
	}

	this.lvMenuStat = false;
	this.wallCommenting = [];

	this.wallImage = false;
	this.wallVideo = false;
	this.wallLink = false;

	this.currentSkin = 'blue';

	this.skinList = [
		'lightblue',
		'bluegray',
		'cyan',
		'teal',
		'green',
		'orange',
		'blue',
		'purple'
	];

	this.skinSwitch = function(color) {
		this.currentSkin = color;
	}
	this.openSearch = function() {
		angular.element('#header').addClass('search-toggled');
		angular.element('#top-search-wrap').find('input').focus();
	}

	this.closeSearch = function() {
		angular.element('#header').removeClass('search-toggled');
	}
	this.clearNotification = function($event) {
		$event.preventDefault();
		$http.post("/msg/MarkRead", {
			id:_.max($scope.InternalMsgs, m => m.Id).Id
		}).then(function(res) {
			var data = res.data;
			if(data.Success) {
				var x = angular.element($event.target).closest('.listview');
				var y = x.find('.lv-item');
				var z = y.length;

				angular.element($event.target).parent().fadeOut();

				x.find('.list-group').prepend('<i class="grid-loading hide-it"></i>');
				x.find('.grid-loading').fadeIn(1500);
				var w = 0;

				y.each(function() {
					var z = $(this);
					$timeout(function() {
						z.addClass('animated fadeOutRightBig').delay(1000).queue(function() {
							z.remove();
						});
					}, w += 150);
				});

				$timeout(function() {
					angular.element('#notifications').addClass('empty');
					$scope.InternalMsgs.length = 0;
				}, (z * 150) + 200);
			}
		});
	}

	this.clearLocalStorage = function() {
		swal({
			title:"清除网站缓存?",
			text:"网站缓存即将被清除",
			type:"warning",
			showCancelButton:true,
			showCloseButton:true,
			confirmButtonColor:"#F44336",
			confirmButtonText:"确定",
			cancelButtonText:"取消",
			closeOnConfirm:false
		}).then(function() {
			localStorage.clear();
			swal("清除成功", "网站缓存已经被清除", "success");
		}).catch(swal.noop);

	}

	this.fullScreen = function() {
		function launchIntoFullscreen(element) {
			if(element.requestFullscreen) {
				element.requestFullscreen();
			} else if(element.mozRequestFullScreen) {
				element.mozRequestFullScreen();
			} else if(element.webkitRequestFullscreen) {
				element.webkitRequestFullscreen();
			} else if(element.msRequestFullscreen) {
				element.msRequestFullscreen();
			}
		}

		function exitFullscreen() {
			if(document.exitFullscreen) {
				document.exitFullscreen();
			} else if(document.mozCancelFullScreen) {
				document.mozCancelFullScreen();
			} else if(document.webkitExitFullscreen) {
				document.webkitExitFullscreen();
			}
		}

		if(exitFullscreen()) {
			launchIntoFullscreen(document.documentElement);
		} else {
			launchIntoFullscreen(document.documentElement);
		}
	}

	$http.post("/passport/getuserinfo", null).then(function(res) {
		if(res.data.Success) {
			$scope.user = res.data.Data;
		}
	});

	$scope.request = function(url, data, success, error) {
		$http.post(url, data, {
			'Content-Type':'application/x-www-form-urlencoded'
		}).then(function(res) {
			if(res.data.Success && res.data.IsLogin) {
				success(res.data);
			} else {
				if(error) {
					error(res.data);
				} else {
					window.notie.alert({
						type:3,
						text:res.data.Message,
						time:4
					});
				}
				$scope.CheckLogin(res.data);
			}
		}, function(e) {
            if (e.data) {
                window.notie.alert({
				    type:3,
				    text:e.data.Message,
				    time:4
			    });
            } else {
                window.notie.alert({
				    type:3,
				    text:'服务请求失败！',
				    time:4
			    });
            }
		});
	}

	$scope.get = function(url, success, error) {
		$http.get(url).then(function(res) {
			if(res.data.Success) {
				success(res.data);
			} else {
				if(error) {
					error(res.data);
				} else {
					window.notie.alert({
						type:3,
						text:res.data.Message,
						time:4
					});
				}
				$scope.CheckLogin(res.data);
			}
		}, function() {
			window.notie.alert({
				type:3,
				text:'服务请求失败！',
				time:4
			});
		});
	}

	this.request = $scope.request;
	$scope.CheckLogin = function(data) {
		if(!data.IsLogin) {
			window.location.href = "/passport/login";
		}
	}
	this.CheckLogin = $scope.CheckLogin;
	this.getmsg = function() {
		$scope.request("/dashboard/getmessages", null, function(res) {
			$scope.Msgs = res.Data;
			if(res.Data.post.length > 0) {
				iziToast.info({
					title:'待审核文章',
					message:'有' + res.Data.post.length + '篇新文章待审核哦!',
					position:'topRight',
					transitionIn:'bounceInRight',
				});
			}
			if(res.Data.msgs.length > 0) {
				iziToast.info({
					title:'待审核留言',
					message:'有' + res.Data.msgs.length + '条新留言待审核哦!',
					position:'topRight',
					transitionIn:'bounceInRight',
				});
			}
			if(res.Data.comments.length > 0) {
				iziToast.info({
					title:'待审核文章评论',
					message:'有' + res.Data.comments.length + '条新文章评论待审核哦!',
					position:'topRight',
					transitionIn:'bounceInRight',
				});
			}
		});

		$http.post("/msg/GetUnreadMsgs").then(function(res) {
			$scope.InternalMsgs = res.data.Data;
			if($scope.InternalMsgs.length > 0) {
				iziToast.info({
					title:'未读消息',
					message:'有' + $scope.InternalMsgs.length + '条未读消息!',
					position:'topRight',
					transitionIn:'bounceInRight',
				});
			}
		});
	}
	this.getmsg();
	$scope.read = function(id) {
		$http.post("/msg/read", {
			id
		});
	}
	$scope.changeUsername = function() {
		swal({
			title:'请输入新的用户名',
			input:'text',
			inputPlaceholder:'请输入用户名',
			showCloseButton:true,
			showCancelButton:true,
			confirmButtonColor:"#DD6B55",
			confirmButtonText:"确定",
			cancelButtonText:"取消",
			inputValue:$scope.user.Username,
			inputValidator:function(value) {
				return new Promise(function(resolve, reject) {
					if(value) {
						resolve();
					} else {
						reject('用户名不能为空');
					}
				});
			}
		}).then(function(result) {
			if(result) {
				$scope.user.Username = result;
				$scope.request("/user/changeUsername", {
					id:$scope.user.Id,
					username:result
				}, function(data) {
					swal({
						type:'success',
						html:data.Message
					});
				});
			}
		}).catch(swal.noop);
	}
	$scope.changeNickname = function() {
		swal({
			title:'请输入新的昵称',
			input:'text',
			inputPlaceholder:'请输入昵称',
			showCloseButton:true,
			showCancelButton:true,
			confirmButtonColor:"#DD6B55",
			confirmButtonText:"确定",
			cancelButtonText:"取消",
			inputValue:$scope.user.NickName,
			inputValidator:function(value) {
				return new Promise(function(resolve, reject) {
					if(value) {
						resolve();
					} else {
						reject('昵称不能为空');
					}
				});
			}
		}).then(function(result) {
			if(result) {
				$scope.user.NickName = result;
				$scope.request("/user/changeNickname", {
					id:$scope.user.Id,
					username:result
				}, function(data) {
					swal({
						type:'success',
						html:data.Message
					});
				});
			}
		}).catch(swal.noop);
	}
	$scope.submit = function() {
        var form = new FormData(document.getElementById("fileform"));
		$http({
            url: "upload",
            method: "post",
            data: form,
            transformRequest: angular.identity,
            headers: {
                'Content-Type': undefined
            }
        }).then(function(response) {
            $http.post("/user/changeavatar", {
				id:$scope.user.Id,
				path:response.data.Data
			}).then(function(res2) {
				swal("头像修改成功", "", "success");
				layer.closeAll();
			}, function(error) {
				swal("头像修改失败", "请求失败，错误码：" + error.status, "error");
			});
        }, function(response) {
			swal("头像修改失败", "", "error");
        });
    };
	
	$scope.changeAvatar = function() {
		layui.use("layer", function() {
		    var layer = layui.layer;
		    layer.open({
                type: 1,
                title: '上传新头像',
			    area: ['420px', '150px'], //宽高
                content: $("#upfile")
		    });
	    });
	}
	$scope.changePassword = function() {
		swal({
			title:'修改密码',
			html:
				'<div class="input-group"><span class="input-group-addon"> 旧  密  码： </span><input id="old" type="password" class="form-control input-lg" autofocus placeholder="请输入旧密码"></div>' +
					'<div class="input-group"><span class="input-group-addon"> 新  密  码： </span><input id="new1" type="password" class="form-control input-lg" placeholder="请输入新密码"></div>' +
					'<div class="input-group"><span class="input-group-addon">确认新密码：</span><input id="new2" type="password" class="form-control input-lg" placeholder="请确认新密码"></div>',
			showCloseButton:true,
			showCancelButton:true,
			confirmButtonColor:"#DD6B55",
			confirmButtonText:"确定",
			cancelButtonText:"取消",
			showLoaderOnConfirm:true,
			allowOutsideClick:false,
			preConfirm:function() {
				return new Promise(function(resolve, reject) {
					var old = $('#old').val(), new1 = $('#new1').val(), new2 = $('#new2').val();
					$http.post("/user/changepassword", {
						id:$scope.user.Id,
						old,
						pwd:new1,
						pwd2:new2
					}).then(function(res) {
						if(res.data.Success) {
							resolve(res.data.Message);
						} else {
							reject(res.data.Message);
						}
					}, function(error) {
						reject("操作失败");
					});
				});
			}
		}).then(function(result) {
			if(result) {
				swal(result, "", "success");
			}
		}).catch(swal.noop);
	}
	$scope.logout = function() {
		swal({
			title:'确认退出系统吗？',
			text:"您将退出管理系统并不再自动登录",
			type:'warning',
			showCancelButton:true,
			showCloseButton:true,
			confirmButtonColor:'#3085d6',
			cancelButtonColor:'#d33',
			confirmButtonText:'确认',
			cancelButtonText:"取消",
			preConfirm:function() {
				return new Promise(function(resolve, reject) {
					$http.post("/passport/logout", null).then(function(res) {
						if(res.data.Success) {
							resolve();
						} else {
							reject("退出登录失败！");
						}
					});
				});
			}
		}).then(function() {
			setTimeout(function() {
				window.location.href = "/";
			}, 2000);
			swal(
				'退出成功',
				'即将返回网站首页',
				'success'
			);
		}).catch(swal.noop);
	}
	
}]);

function getFile(obj, inputName) {
	var file_name = $(obj)[0].files[0].name;
	$("input[name='" + inputName + "']").val(file_name);
}

///**
// * 将带pid/ParentId的json数据转换成带children树形json
// * @param {any} a 源数据
// * @param {any} idStr  id字段
// * @param {any} pidStr  pid字段
// * @param {any} chindrenStr  children字段
// */
//function transData(a, idStr, pidStr, chindrenStr) {
//	var r = [], hash = {}, id = idStr, pid = pidStr, children = chindrenStr, i = 0, j = 0, len = a.length;
//	for(; i < len; i++) {
//		hash[a[i][id]] = a[i];
//	}
//	for(; j < len; j++) {
//		var aVal = a[j], hashVP = hash[aVal[pid]];
//		if(hashVP) {
//			!hashVP[children] && (hashVP[children] = []);
//			aVal[children] = [];
//			hashVP[children].push(aVal);
//		} else {
//			aVal[children] = [];
//			r.push(aVal);
//		}
//	}
//	return r;
//}

//var jsonDataTree = transData(jsonData, 'id', 'pid', 'chindren');
Date.prototype.Format = function(fmt) {//author: meizz 
	var o = {
		"M+":this.getMonth() + 1, //月份 
		"d+":this.getDate(), //日 
		"h+":this.getHours(), //小时 
		"m+":this.getMinutes(), //分 
		"s+":this.getSeconds(), //秒 
		"q+":Math.floor((this.getMonth() + 3) / 3), //季度 
		"S":this.getMilliseconds() //毫秒 
	};
	if(/(y+)/.test(fmt)) {
		fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
	}
	for(var k in o) {
		if(new RegExp("(" + k + ")").test(fmt)) {
			fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
		}
	}
	return fmt;
}
Array.prototype.remove = function(val) {
	var index = this.indexOf(val);
	if(index > -1) {
		this.splice(index, 1);
	}
};