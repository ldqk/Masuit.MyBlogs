angular.module('tm.pagination', []).directive('tmPagination', [function () {
	window.timeout = null;
	return {
		restrict: 'EA',
		template: '<div class="page-list">' +
			'<ul class="pagination" ng-show="conf.totalItems > 0">' +
			'<li ng-class="{disabled: conf.currentPage == 1}" ng-click="prevPage()"><span>&laquo;</span></li>' +
			'<li ng-repeat="item in pageList track by $index" ng-class="{active: item == conf.currentPage, separate: item == \'...\'}" ' +
			'ng-click="changeCurrentPage(item)">' +
			'<span>{{ item }}</span>' +
			'</li>' +
			'<li ng-class="{disabled: conf.currentPage == conf.numberOfPages}" ng-click="nextPage()"><span>&raquo;</span></li>' +
			'</ul>' +
			'<div class="page-total" ng-show="conf.totalItems > 0">' +
			'第<input type="text" ng-model="jumpPageNum"  ng-keyup="jumpToPage($event)"/>页 / 共<strong>{{ totalPage }}</strong>页，' +
			'每页<select ng-model="conf.itemsPerPage" ng-options="option for option in conf.perPageOptions " ng-change="changeItemsPerPage()"></select>' +
			'条，合计<strong>{{ conf.totalItems }}</strong>条' +
			'</div>' +
			'<div class="no-items" ng-show="conf.totalItems <= 0">暂无数据</div>' +
			'</div>',
		replace: true,
		scope: {
			conf: '='
		},
		link: function (scope, element, attrs) {
			// 变更当前页
			scope.changeCurrentPage = function (item) {
				if (item == '...') {
					return;
				} else {
					scope.conf.currentPage = item;
				}
			};
			// 定义分页的长度必须为奇数 (default:9)
			scope.conf.pagesLength = parseInt(scope.conf.pagesLength) ? parseInt(scope.conf.pagesLength) : 9;
			if (scope.conf.pagesLength % 2 === 0) {
				// 如果不是奇数的时候处理一下
				scope.conf.pagesLength = scope.conf.pagesLength - 1;
			}
			if (!scope.conf.perPageOptions) {
				scope.conf.perPageOptions = [10, 15, 20, 30, 50];
			}
			// pageList数组
			function getPagination() {
			scope.totalPage = Math.ceil(scope.conf.totalItems / scope.conf.itemsPerPage);
				// conf.currentPage
				scope.conf.currentPage = parseInt(scope.conf.currentPage) ? parseInt(scope.conf.currentPage) : 1;
				// conf.totalItems
				scope.conf.totalItems = parseInt(scope.conf.totalItems);
				// conf.itemsPerPage (default:15)
				// 先判断一下本地存储中有没有这个值
				if (scope.conf.rememberPerPage) {
					if (!parseInt(localStorage[scope.conf.rememberPerPage])) {
						localStorage[scope.conf.rememberPerPage] = parseInt(scope.conf.itemsPerPage) ? parseInt(scope.conf.itemsPerPage) : 15;
					}
					scope.conf.itemsPerPage = parseInt(localStorage[scope.conf.rememberPerPage]);
				} else {
					scope.conf.itemsPerPage = parseInt(scope.conf.itemsPerPage) ? parseInt(scope.conf.itemsPerPage) : 15;
				}
				// numberOfPages
				scope.conf.numberOfPages = Math.ceil(scope.conf.totalItems / scope.conf.itemsPerPage);
				// judge currentPage > scope.numberOfPages
				if (scope.conf.currentPage < 1) {
					scope.conf.currentPage = 1;
				}
				if (scope.conf.currentPage > scope.conf.numberOfPages) {
					scope.conf.currentPage = scope.conf.numberOfPages;
				}
				// jumpPageNum
				scope.jumpPageNum = scope.conf.currentPage;
				// 如果itemsPerPage在不在perPageOptions数组中，就把itemsPerPage加入这个数组中
				var perPageOptionsLength = scope.conf.perPageOptions.length;
				// 定义状态
				var perPageOptionsStatus;
				for (var i = 0; i < perPageOptionsLength; i++) {
					if (scope.conf.perPageOptions[i] == scope.conf.itemsPerPage) {
						perPageOptionsStatus = true;
					}
				}
				// 如果itemsPerPage在不在perPageOptions数组中，就把itemsPerPage加入这个数组中
				if (!perPageOptionsStatus) {
					scope.conf.perPageOptions.push(scope.conf.itemsPerPage);
				}
				// 对选项进行sort
				scope.conf.perPageOptions.sort(function (a, b) { return a - b });
				scope.pageList = [];
				if (scope.conf.numberOfPages <= scope.conf.pagesLength) {
					// 判断总页数如果小于等于分页的长度，若小于则直接显示
					for (i = 1; i <= scope.conf.numberOfPages; i++) {
						scope.pageList.push(i);
					}
				} else {
					// 总页数大于分页长度（此时分为三种情况：1.左边没有...2.右边没有...3.左右都有...）
					// 计算中心偏移量
					var offset = (scope.conf.pagesLength - 1) / 2;
					if (scope.conf.currentPage <= offset) {
						// 左边没有...
						for (i = 1; i <= offset + 1; i++) {
							scope.pageList.push(i);
						}
						scope.pageList.push('...');
						scope.pageList.push(scope.conf.numberOfPages);
					} else if (scope.conf.currentPage > scope.conf.numberOfPages - offset) {
						scope.pageList.push(1);
						scope.pageList.push('...');
						for (i = offset + 1; i >= 1; i--) {
							scope.pageList.push(scope.conf.numberOfPages - i);
						}
						scope.pageList.push(scope.conf.numberOfPages);
					} else {
						// 最后一种情况，两边都有...
						scope.pageList.push(1);
						scope.pageList.push('...');

						for (i = Math.ceil(offset / 2); i >= 1; i--) {
							scope.pageList.push(scope.conf.currentPage - i);
						}
						scope.pageList.push(scope.conf.currentPage);
						for (i = 1; i <= offset / 2; i++) {
							scope.pageList.push(scope.conf.currentPage + i);
						}

						scope.pageList.push('...');
						scope.pageList.push(scope.conf.numberOfPages);
					}
				}
				if (scope.conf.onChange) {
					if (window.timeout) {
						clearTimeout(window.timeout);
					}
					window.timeout = setTimeout(function() {
						if (scope.conf.currentPage > 0) {
							scope.conf.onChange();
							window.timeout = null;
						}
					}, 100);
				}
				scope.$parent.conf = scope.conf;
			}
			// prevPage
			scope.prevPage = function () {
				if (scope.conf.currentPage > 1) {
					scope.conf.currentPage -= 1;
				}
			};
			// nextPage
			scope.nextPage = function () {
				if (scope.conf.currentPage < scope.conf.numberOfPages) {
					scope.conf.currentPage += 1;
				}
			};
			// 跳转页
			scope.jumpToPage = function () {
				scope.jumpPageNum = scope.jumpPageNum.replace(/[^0-9]/g, '');
				if (scope.jumpPageNum !== '') {
					scope.conf.currentPage = scope.jumpPageNum;
				}
			};
			// 修改每页显示的条数
			scope.changeItemsPerPage = function () {
				// 清除本地存储的值方便重新设置
				if (scope.conf.rememberPerPage) {
					localStorage.removeItem(scope.conf.rememberPerPage);
				}
			};
			scope.$watch(function () {
				var newValue = scope.conf.currentPage + ' ' + scope.conf.totalItems + ' ';
				// 如果直接watch perPage变化的时候，因为记住功能的原因，所以一开始可能调用两次。
				//所以用了如下方式处理
				if (scope.conf.rememberPerPage) {
					// 由于记住的时候需要特别处理一下，不然可能造成反复请求
					// 之所以不监控localStorage[scope.conf.rememberPerPage]是因为在删除的时候会undefind
					// 然后又一次请求
					if (localStorage[scope.conf.rememberPerPage]) {
						newValue += localStorage[scope.conf.rememberPerPage];
					} else {
						newValue += scope.conf.itemsPerPage;
					}
				} else {
					newValue += scope.conf.itemsPerPage;
				}
				return newValue;
			}, getPagination);
		}
	};
}]);