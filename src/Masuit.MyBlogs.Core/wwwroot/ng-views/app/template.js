myApp.directive('changeLayout', function() {
	return {
		restrict: 'A',
		scope: {
			changeLayout: '='
		},
		link: function(scope, element, attr) {
			if (scope.changeLayout === '1') {
				element.prop('checked', true);
			}
			element.on('change', function() {
				if (element.is(':checked')) {
					localStorage.setItem('ma-layout-status', 1);
					scope.$apply(function() {
						scope.changeLayout = '1';
					});
				} else {
					localStorage.setItem('ma-layout-status', 0);
					scope.$apply(function() {
						scope.changeLayout = '0';
					});
				}
			});
		}
	}
}).directive('toggleSidebar', function() {
	return {
		restrict: 'A',
		scope: {
			modelLeft: '=',
			modelRight: '='
		},
		link: function(scope, element, attr) {
			element.on('click', function() {
				if (element.data('target') === 'mainmenu') {
					if (scope.modelLeft === false) {
						scope.$apply(function() {
							scope.modelLeft = true;
						});
					} else {
						scope.$apply(function() {
							scope.modelLeft = false;
						});
					}
				}
				if (element.data('target') === 'chat') {
					if (scope.modelRight === false) {
						scope.$apply(function() {
							scope.modelRight = true;
						});
					} else {
						scope.$apply(function() {
							scope.modelRight = false;
						});
					}

				}
			});
		}
	}
}).directive('toggleSubmenu', function() {
	return {
		restrict: 'A',
		link: function(scope, element, attrs) {
			element.click(function() {
				element.next().slideToggle(200);
				element.parent().toggleClass('toggled');
			});
		}
	}
}).directive('stopPropagate', function() {
	return {
		restrict: 'C',
		link: function(scope, element) {
			element.on('click', function(event) {
				event.stopPropagation();
			});
		}
	}
}).directive('aPrevent', function() {
	return {
		restrict: 'C',
		link: function(scope, element) {
			element.on('click', function(event) {
				event.preventDefault();
			});
		}
	}
}).directive('print', function() {
	return {
		restrict: 'A',
		link: function(scope, element) {
			element.click(function() {
				window.print();
			});
		}
	}
})