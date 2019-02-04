indexnumber = 0;
jQuery.fn.highlight = function(pat) {
	function innerHighlight(node, pat) {
		var skip = 0;
		if (node.nodeType == 3) {
			var pos = node.data.toUpperCase().indexOf(pat);
			if (pos >= 0) {
				var spannode = document.createElement('a');
				spannode.className = 'highlight';
				spannode.id = 'highlight';
				var middlebit = node.splitText(pos);
				var endbit = middlebit.splitText(pat.length);
				var middleclone = middlebit.cloneNode(true);
				spannode.appendChild(middleclone);
				middlebit.parentNode.replaceChild(spannode, middlebit);
				skip = 1;
			}
		} else if (node.nodeType == 1 && node.childNodes && !/(script|style)/i.test(node.tagName)) {
			for (var i = 0; i < node.childNodes.length; ++i) {
				i += innerHighlight(node.childNodes[i], pat);
			}
		}
		return skip;
	}

	return this.each(function() {
		var skip = innerHighlight(this, pat.toUpperCase());
		if (skip == 0) {
			highlightAcross(pat.toUpperCase());
		}
	});
};

function highlightAcross(searchTerm) {
	var chil = $("#article").children().children();
	var searchSplit = searchTerm.split("");
	for (var i = 0; i < searchSplit.length; i++) {
	}
	var currentMapTem = {};
	var currentMap = {};
	var currentInclude = "";
	var searchSplitnumber = 0;
	var iTem = 0
	for (var i = 0; i < chil.length; i++) {
		var countobj = $(chil.get(i));
		var count = $(chil.get(i)).text();
		var countText = count.split("");
		for (var j = 0; j < countText.length; j++) {
			if (countText[j] == searchSplit[searchSplitnumber] && searchSplitnumber != searchSplit.length - 1) {
				for (var k = searchSplitnumber; k < searchSplit.length && j < countText.length; k++) {
					if (countText[j + k - searchSplitnumber] == searchSplit[k]) {
						currentInclude += countText[j + k - searchSplitnumber]
					}
					if ((j + k - searchSplitnumber) == countText.length) {
						currentMapTem[i] = currentInclude;
						searchSplitnumber = k;
						currentInclude = "";
						break;
					}
					if (k != 0 &&
						countText[j + k - searchSplitnumber] != searchSplit[k] &&
						(j + k - searchSplitnumber) != countText.length - 1 &&
						k != searchSplit.length - 1) {
						currentMapTem = {};
						currentInclude = "";
						break;
					}
					if (countText[j + k - searchSplitnumber] == searchSplit[k] && k == searchSplit.length - 1) {
						currentMapTem[i] = currentInclude + "☆code☆end☆";
						$.each(currentMapTem, function(key, values) { currentMap[key] = values });
						searchSplitnumber = k;
						currentInclude = "";
						break;
					}
				}
			} else {
				if (searchSplitnumber != 0 && j == 0) {
					currentMapTem = {};
					currentInclude = "";
					searchSplitnumber = 0;
				}
			}
		}
		if (searchSplitnumber == searchSplit.length - 1) {
			searchSplitnumber = 0;
		}
	}
	var endContent = "";
	$.each(currentMap,
		function(key, values) {
			console.log(key);
			console.log(values);
			if (values.indexOf("☆code☆end☆") > 0) {
				values = values.replace("☆code☆end☆", "");
				var str = $(chil.get(key)).html().replace(values, "");
				$(chil.get(key)).html(str);
				endContent += values;
				$(chil.get(key)).prepend("<" +
					$(chil.get(key))[0].tagName +
					" class='highlight " +
					$(chil.get(key))[0].className +
					"' id='" +
					$(chil.get(key)).attr("id") +
					"' style=" +
					$(chil.get(key)).attr("style") +
					">" +
					endContent +
					"</span>");
				endContent = "";
			} else {
				var str = $(chil.get(key)).html().replace(values, "");
				$(chil.get(key)).html(str);
				endContent += values;
			}
		});
}

function next() {
	var highlight = $(".highlight");
	if (indexnumber == highlight.size()) {
		return;
	}
	var offsetParent = highlight.eq(indexnumber).offset();
	highlight.eq(indexnumber).css("background-color", "#DC143C");
	if (indexnumber != 0) {
		highlight.eq(indexnumber - 1).css("background-color", "#fff34d");
	}
	indexnumber++;
	window.scrollTo(offsetParent.left, offsetParent.top);
};

jQuery.fn.removeHighlight = function() {
	function newNormalize(node) {
		for (var i = 0, children = node.childNodes, nodeCount = children.length; i < nodeCount; i++) {
			var child = children[i];
			if (child.nodeType == 1) {
				newNormalize(child);
				continue;
			}
			if (child.nodeType != 3) {
				continue;
			}
			var next = child.nextSibling;
			if (next == null || next.nodeType != 3) {
				continue;
			}
			var combined_text = child.nodeValue + next.nodeValue;
			new_node = node.ownerDocument.createTextNode(combined_text);
			node.insertBefore(new_node, child);
			node.removeChild(child);
			node.removeChild(next);
			i--;
			nodeCount--;
		}
	}

	return this.find("span.highlight").each(function() {
		var thisParent = this.parentNode;
		thisParent.replaceChild(this.firstChild, this);
		newNormalize(thisParent);
	}).end();
};