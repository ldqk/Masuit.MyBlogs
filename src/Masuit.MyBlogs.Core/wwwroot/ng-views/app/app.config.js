var myApp = angular.module('myApp', ["ui.router", "oc.lazyLoad", 'ngAnimate', 'ngResource', 'ui.bootstrap', 'angular-loading-bar', 'ng.ueditor', "ngTable", "tm.pagination", 'ui.tree', 'ui.bootstrap','ngFileUpload']);
myApp.config(["$provide", "$compileProvider", "$controllerProvider", "$filterProvider",function($provide, $compileProvider, $controllerProvider, $filterProvider) {
		myApp.controller = $controllerProvider.register;
		myApp.directive = $compileProvider.directive;
		myApp.filter = $filterProvider.register;
		myApp.factory = $provide.factory;
		myApp.service = $provide.service;
		myApp.constant = $provide.constant;
}]);
myApp.directive("trackedTable", function () {
	return {
		restrict: "A",
		priority: -1,
		require: "ngForm",
		controller:
		function ($scope, $parse, $attrs) {
			var self = this;
			var dirtyCellsByRow = [];
			var invalidCellsByRow = [];
			init();

			function init() {
				var setter = $parse($attrs.trackedTable).assign;
				setter($scope, self);
				$scope.$on("$destroy", function () {
					setter(null);
				});

				self.reset = function () {
					dirtyCellsByRow = [];
					invalidCellsByRow = [];
					setInvalid(false);
				};
				self.isCellDirty = function (row, cell) {
					var rowCells = getCellsForRow(row, dirtyCellsByRow);
					return rowCells && rowCells.cells.indexOf(cell) !== -1;
				};
				self.setCellDirty = function (row, cell, isDirty) {
					setCellStatus(row, cell, isDirty, dirtyCellsByRow);
				};
				self.setCellInvalid = function (row, cell, isInvalid) {
					setCellStatus(row, cell, isInvalid, invalidCellsByRow);
					setInvalid(invalidCellsByRow.length > 0);
				};
				self.untrack = function (row) {
					_.remove(invalidCellsByRow, function (item) {
						return item.row === row;
					});
					_.remove(dirtyCellsByRow, function (item) {
						return item.row === row;
					});
					setInvalid(invalidCellsByRow.length > 0);
				};
			}

			function getCellsForRow(row, cellsByRow) {
				return _.find(cellsByRow, function (entry) {
					return entry.row === row;
				})
			}

			function setCellStatus(row, cell, value, cellsByRow) {
				var rowCells = getCellsForRow(row, cellsByRow);
				if (!rowCells && !value) {
					return;
				}

				if (value) {
					if (!rowCells) {
						rowCells = {
							row: row,
							cells: []
						};
						cellsByRow.push(rowCells);
					}
					if (rowCells.cells.indexOf(cell) === -1) {
						rowCells.cells.push(cell);
					}
				} else {
					_.remove(rowCells.cells, function (item) {
						return cell === item;
					});
					if (rowCells.cells.length === 0) {
						_.remove(cellsByRow, function (item) {
							return rowCells === item;
						});
					}
				}
			}

			function setInvalid(isInvalid) {
				self.$invalid = isInvalid;
				self.$valid = !isInvalid;
			}
		}
	};
});
myApp.filter('htmlString', ['$sce', function ($sce) {
	　　return function (text) {
		return $sce.trustAsHtml(text);
	　　};
}]);