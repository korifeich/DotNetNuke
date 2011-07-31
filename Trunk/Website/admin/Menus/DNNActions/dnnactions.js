(function (c, b, a) { c.fn.dnnActionMenu = function (d) { var f = c.extend({}, c.fn.dnnActionMenu.defaultOptions, d), e = this; e.each(function () { var j = c(this); function i(n, k, m) { var l = n.children("." + f.borderClassName); if (l.size() === 0) { l = c('<div class="' + f.borderClassName + '"></div>').prependTo(n).css({ opacity: 0 }) } n.attr("style", "z-index:904;"); if (m) { n.find(f.menuActionSelector).fadeTo(f.fadeSpeed, k) } n.children("." + f.borderClassName).fadeTo(f.fadeSpeed, k) } function g(m, k, l) { m.removeAttr("style"); m.children("." + f.borderClassName).stop().fadeTo(f.fadeSpeed, 0); if (l) { m.find(f.menuActionSelector).stop().fadeTo(f.fadeSpeed, k) } } function h(k) { var n = j.find(f.menuSelector).show(), p = n.height(), o = c(b).height(), m = k.offset().top, l = (o - ((m - c(b).scrollTop()) + k.height())); if ((p > l) && (p <= m)) { n.position({ my: "left bottom", at: "left top", of: k, collision: "none" }) } else { n.position({ my: "left top", at: "left bottom", of: k, collision: "none" }) } } if (j.find(f.menuSelector).size() > 0) { j.hoverIntent({ sensitivity: f.hoverSensitivity, timeout: f.hoverTimeout, interval: f.hoverInterval, over: function () { i(c(this).data("intentExpressed", true), 1, true) }, out: function () { g(c(this).data("intentExpressed", false), f.defaultOpacity, true) } }); j.hover(function () { i(c(this), f.defaultOpacity, false) }, function () { var k = c(this); if (!k.data("intentExpressed")) { g(k, 0, false) } }); j.find(f.menuActionSelector).css({ opacity: f.defaultOpacity }); j.find(f.menuWrapSelector).hoverIntent({ sensitivity: f.hoverSensitivity, timeout: f.hoverTimeout, interval: f.hoverInterval, over: function () { h(c(this)); j.find(f.menuSelector).fadeTo(f.fadeSpeed, 1) }, out: function () { j.find(f.menuSelector).stop().fadeTo(f.fadeSpeed, 0).hide() } }); j.find(f.menuSelector).children().css({ opacity: 1 }); j.find(f.menuWrapSelector).draggable({ containment: j.children().eq(1), start: function (k, l) { j.find(f.menuSelector).hide() }, stop: function (k, l) { h(c(this)); j.find(f.menuSelector).show() } }) } }); return e }; c.fn.dnnActionMenu.defaultOptions = { menuWrapSelector: ".dnnActionMenu", menuActionSelector: ".dnnActionMenuTag", menuSelector: "ul.dnnActionMenuBody", defaultOpacity: 0.3, fadeSpeed: "fast", borderClassName: "dnnActionMenuBorder", hoverSensitivity: 2, hoverTimeout: 200, hoverInterval: 200 }; c(document).ready(function () { c(".DnnModule").dnnActionMenu() }) })(jQuery, window, console);