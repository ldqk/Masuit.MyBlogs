myApp.directive('fgLine', function() {
	return {
		restrict: 'C',
		link: function(scope, element) {
			if ($('.fg-line')[0]) {
				$('body').on('focus', '.form-control', function() {
					$(this).closest('.fg-line').addClass('fg-toggled');
				})
				$('body').on('blur', '.form-control', function() {
					var p = $(this).closest('.form-group');
					var i = p.find('.form-control').val();

					if (p.hasClass('fg-float')) {
						if (i.length == 0) {
							$(this).closest('.fg-line').removeClass('fg-toggled');
						}
					} else {
						$(this).closest('.fg-line').removeClass('fg-toggled');
					}
				});
			}
		}
	}
}).directive('autoSize', function() {
	return {
		restrict: 'A',
		link: function(scope, element) {
			if (element[0]) {
				autosize(element);
			}
		}
	}
}).directive('selectPicker', function() {
	return {
		restrict: 'A',
		link: function(scope, element, attrs) {
			element.selectpicker();
		}
	}
}).directive('inputMask', function() {
	return {
		restrict: 'A',
		scope: {
			inputMask: '='
		},
		link: function(scope, element) {
			element.mask(scope.inputMask.mask);
		}
	}
}).directive('colordPicker', function() {
	return {
		restrict: 'A',
		link: function(scope, element, attrs) {
			$(element).each(function() {
				var colorOutput = $(this).closest('.cp-container').find('.cp-value');
				$(this).farbtastic(colorOutput);
			});
		}
	}
}).directive('formControl', function() {
	return {
		restrict: 'C',
		link: function(scope, element, attrs) {
			if (angular.element('html').hasClass('ie9')) {
				$('input, textarea').placeholder({
					customClass: 'ie9-placeholder'
				});
			}
		}
	}
})