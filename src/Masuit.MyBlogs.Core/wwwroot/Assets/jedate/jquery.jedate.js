/**
 @Name : jeDate v3.8.3 日期控件
 @Author: chen guojun
 @Date: 2017-06-01
 @QQ群：516754269
 @官网：http://www.jemui.com/ 或 https://github.com/singod/jeDate
 */

;(function(root, factory) {
    //amd
    if (typeof define === 'function' && define.amd) {
        define(['jquery'], factory);
    } else if (typeof exports === 'object') { //umd
        module.exports = factory();
    } else {
        root.jeDate = factory(window.jQuery || $);
    }
})(this, function($) {
    var jet = {}, doc = document, regymdzz = "YYYY|MM|DD|hh|mm|ss|zz",
        regymd = "YYYY|MM|DD|hh|mm|ss|zz".replace("|zz",""),
        parseInt = function (n) { return window.parseInt(n, 10);},
        config = {
            skinCell:"jedateblue",
            language:{
                name  : "cn",
                month : ["01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"],
                weeks : [ "日", "一", "二", "三", "四", "五", "六" ],
                times : ["小时","分钟","秒数"],
                clear : "清空",
                today : "今天",
                yes   : "确定",
                close : "关闭"
            },
            trigger:"click",
            format:"YYYY-MM-DD hh:mm:ss", //日期格式
            minDate:"1900-01-01 00:00:00", //最小日期
            maxDate:"2099-12-31 23:59:59" //最大日期
        };
    $.fn.jeDate = function(options){
        return this.each(function(){
            return new jeDate($(this),options||{});
        });
    };
    $.extend({
        jeDate:function(elem, options){
            return $(elem).each(function(){
                return new jeDate($(this),options||{});
            });
        }
    });
    jet.isObj = function (obj){
        for(var i in obj){return true;}
        return false;
    };
    jet.reMacth = function (str) {
        return str.match(/\w+|d+/g);
    };
    jet.docScroll = function(type) {
        type = type ? "scrollLeft" :"scrollTop";
        return doc.body[type] | doc.documentElement[type];
    };
    jet.winarea = function(type) {
        return doc.documentElement[type ? "clientWidth" :"clientHeight"];
    };
    //判断是否闰年
    jet.isLeap = function(y) {
        return (y % 100 !== 0 && y % 4 === 0) || (y % 400 === 0);
    };
    //获取本月的总天数
    jet.getDaysNum = function(y, m) {
        var num = 31;
        switch (parseInt(m)) {
            case 2:
                num = jet.isLeap(y) ? 29 : 28; break;
            case 4: case 6: case 9: case 11:
            num = 30; break;
        }
        return num;
    };
    //获取月与年
    jet.getYM = function(y, m, n) {
        var nd = new Date(y, m - 1);
        nd.setMonth(m - 1 + n);
        return {
            y: nd.getFullYear(),
            m: nd.getMonth() + 1
        };
    }
    //获取上个月
    jet.getPrevMonth = function(y, m, n) {
        return jet.getYM(y, m, 0 - (n || 1));
    };
    //获取下个月
    jet.getNextMonth = function(y, m, n) {
        return jet.getYM(y, m, n || 1);
    };
    //补齐数位
    jet.digit = function(num) {
        return num < 10 ? "0" + (num | 0) :num;
    };
    //判断是否为数字
    jet.IsNum = function(value){
        return /^[+-]?\d*\.?\d*$/.test(value) ? true : false;
    };
    //转换日期格式
    jet.parse = function(ymd, hms, format) {
        ymd = ymd.concat(hms);
        var ymdObj = {}, mat = regymdzz.split("|");
        $.each(ymd,function (i,val) {
            ymdObj[mat[i]] = parseInt(val);
        });
        return format.replace(new RegExp(regymdzz,"g"), function(str, index) {
            return str == "zz" ? "00":jet.digit(ymdObj[str]);
        });
    };
    jet.checkFormat = function(format) {
        var ymdhms = [];
        format.replace(new RegExp(regymdzz,"g"), function(str, index) {
            ymdhms.push(str);
        });
        return ymdhms.join("-");
    };
    jet.parseMatch = function(str) {
        var timeArr = str.split(" ");
        return jet.reMacth(timeArr[0]);
    };
    jet.testFormat = function(format,conformat) {
        var mat = ["YYYY","MM","DD"],ymdhms = [],mats = [];
        format.replace(new RegExp(regymdzz,"g"), function(str, index) {
            ymdhms.push(str);
        });
        $.each(mat,function (m,d) {
            $.each(ymdhms,function (i,f) {
                if (d == f) mats.push(f);
            })
        });
        var reformat = format.substring(0, 2) == "hh" ? ymdhms.join("-") : mats.join("-");
        var remat = reformat == conformat ? true : false;
        return remat;
    };
    //验证日期
    jet.checkDate = function (date) {
        var dateArr = jet.reMacth(date);
        if (isNaN(dateArr[0]) || isNaN(dateArr[1]) || isNaN(dateArr[2])) return false;
        if (dateArr[1] > 12 || dateArr[1] < 1) return false;
        if (dateArr[2] < 1 || dateArr[2] > 31) return false;
        if ((dateArr[1] == 4 || dateArr[1] == 6 || dateArr[1] == 9 || dateArr[1] == 11) && dateArr[2] > 30) return false;
        if (dateArr[1] == 2) {
            if (dateArr[2] > 29) return false;
            if ((dateArr[0] % 100 == 0 && dateArr[0] % 400 != 0 || dateArr[0] % 4 != 0) && dateArr[2] > 28) return false;
        }
        return true;
    };
    //返回日期
    jet.returnDate = function(obj, format, dateval) {
        format = format || 'YYYY-MM-DD hh:mm:ss';
        var undate = (dateval == undefined || dateval == "" || dateval == []), darr = undate ? [] : jet.reMacth(dateval), sparr = [],
            myDate = darr.length > 0 ? new Date(darr[0],darr[1],(darr[2]||00),(darr[3]||00),(darr[4]||00),(darr[5]||00)) : new Date(),
            myMM = myDate.getMonth(), myDD = myDate.getDate(),
            narr = [myDate.getFullYear(), myMM, myDD, myDate.getHours(), myDate.getMinutes(), myDate.getSeconds()];
        $.each(regymd.split("|"),function (i,val) {
            sparr.push(obj[val]||"");
        });
        var mday = jet.getDaysNum(narr[0], narr[1]+1),
            isDay31 = mday == 31 && jet.digit(new Date().getDate()) == 31,
            parnaVal = narr[2]+parseInt(sparr[2]||00), gday, reday;
        //判断今天是否为31号
        gday = isDay31 ? (parnaVal - 1) : parnaVal;
        //重新设置日期，必须要用new Date来设置，否则就会有问题
        var setdate = new Date(narr[0]+parseInt(sparr[0]||00), narr[1]+parseInt(sparr[1]||00), gday, narr[3]+parseInt(sparr[3]||00), narr[4]+parseInt(sparr[4]||00), narr[5]+parseInt(sparr[4]||00));
        reday = isDay31 ? jet.digit(parseInt(setdate.getDate())+1) : jet.digit(setdate.getDate());
        //获取重新设置后的日期
        var reDate = jet.parse([ setdate.getFullYear(), parseInt(setdate.getMonth())+1, reday ], [ jet.digit(setdate.getHours()), jet.digit(setdate.getMinutes()), jet.digit(setdate.getSeconds()) ], format);
        return reDate;
    };
    //判断元素类型
    jet.isValHtml = function(that) {
        return /textarea|input/.test(that[0].tagName.toLocaleLowerCase());
    };
    jet.isBool = function(obj){  return (obj == undefined || obj == true ?  true : false); };
    jet.sortDate = function (time,format) {
        var timeObj = {}, newtime = [], mats = regymd.split("|"),
            subhh = jet.checkFormat(format).substring(0, 2) == "hh" ? true :false,
            numArr = jet.IsNum(time) ? (jet.reMacth(subhh ? time.replace(/(.{2})/g,"$1,") : time.substr(0,4).replace(/^(\d{4})/g,"$1,") + time.substr(4).replace(/(.{2})/g,"$1,"))) : jet.reMacth(time),
            matArr = jet.IsNum(time) ? (jet.reMacth(subhh ? format.replace(/(.{2})/g,"$1,") : format.substr(0,4).replace(/^(\w{4})/g,"$1,") + format.substr(4).replace(/(.{2})/g,"$1,"))) : jet.reMacth(format);
        $.each(numArr,function (i,val) {
            timeObj[matArr[i]] = val;
        });
        $.each(numArr,function (i,val) {
            var mathh = subhh ? mats[3+i] : mats[i];
            newtime.push(timeObj[mathh]);
        });
        return (jet.IsNum(time)) ? (jet.reMacth(subhh ? time.replace(/(.{2})/g,"$1,") : time.substr(0,4).replace(/^(\d{4})/g,"$1,") + time.substr(4).replace(/(.{2})/g,"$1,"))) : newtime;
    };
    var searandom = function (){
        var str = "",arr = [1,2,3,4,5,6,7,8,9,0];
        for(var i=0; i<8; i++) str += arr[Math.round(Math.random() * (arr.length-1))];
        return str;
    };
    function jeDate(elem, opts){
        this.opts = opts;
        this.valCell = elem;
        this.init();
    }
    var jedfn = jeDate.prototype, jefix = "jefixed";
    jedfn.init = function(){
        var that = this, opts = that.opts, newDate = new Date(),
            jetrigger = opts.trigger != undefined ? opts.trigger : config.trigger,
            zIndex = opts.zIndex == undefined ? 2099 : opts.zIndex,
            isinitVal = (opts.isinitVal == undefined || opts.isinitVal == false) ? false : true;
        jet.fixed = jet.isBool(opts.fixed);
        var fixedCell = (opts.fixedCell == undefined || opts.fixedCell == "") ? "" : "#"+this.opts.fixedCell;
        var arrayContain = function(array, obj){
            for (var i = 0; i < array.length; i++){
                if (array[i] == obj) return true;
            }
            return false;
        };
        var initVals = function(objCell) {
            var jeformat = opts.format || config.format,jeaddDate,
                inaddVal = opts.initAddVal || {},
                hmsset = opts.hmsSetVal || {};
            //判断时间为20170430这样类型的
            //var isnosepYMD = arrayContain(["YYYYMM","YYYYMMDD","YYYYMMDDhh","YYYYMMDDhhmm","YYYYMMDDhhmmss"],jeformat);
            if (jet.isObj(hmsset)){
                jeaddDate = jet.parse([ newDate.getFullYear(), jet.digit(newDate.getMonth() + 1), jet.digit(newDate.getDate()) ], [ jet.digit(hmsset.hh), jet.digit(hmsset.mm), jet.digit(hmsset.ss) ], jeformat);
            }else{
                jeaddDate = jet.returnDate(inaddVal, jeformat);
            }

            (objCell.val() || objCell.text()) == "" ? jet.isValHtml(objCell) ? objCell.val(jeaddDate) :objCell.text(jeaddDate) :jet.isValHtml(objCell) ? objCell.val() : objCell.text();
        };
        var formatDate = function (cls,boxcell) {
            var dateDiv = $("<div/>",{"id":boxcell.replace(/\#/g,""),"class":"jedatebox "+(opts.skinCell || config.skinCell)}),
                reabsfix = $(fixedCell).length > 0 ? "relative" : (jet.fixed == true ? "absolute" :"fixed");
            dateDiv.attr("author","chen guojun").css({"z-index": boxcell != "#jedatebox" ? "" : zIndex ,"position":reabsfix,"display":"block"});
            if(boxcell != "#jedatebox") dateDiv.attr({"jeformat":opts.format || config.format,"jefixed":fixedCell});
            jet.minDate = opts.minDate || config.minDate;
            jet.maxDate = opts.maxDate || config.maxDate;
            $(cls).append(dateDiv);
            that.setHtml(opts,boxcell);
        };
        //为开启初始化的时间设置值
        if (isinitVal && jetrigger != false) {
            that.valCell.each(function() {
                initVals($(this));
            });
        }
        //判断固定元素是否存在
        if($(fixedCell).length > 0){
            var randomCell = "#jedatebox"+searandom();
            formatDate(fixedCell,randomCell);
        }else {
            //insTrigger的值为true时内部默认点击事件
            var jd = ["body","#jedatebox"];
            if (jetrigger != false) {
                that.valCell.on(jetrigger, function (ev) {
                    ev.stopPropagation();
                    if ($(jd[1]).length > 0) return;
                    formatDate(jd[0],jd[1]);
                });
            }else {
                formatDate(jd[0],jd[1]);
            }
        }
    };
    //布局控件骨架
    jedfn.setHtml = function(opts,boxcls){
        jet.boxelem = $($(boxcls).attr(jefix)).length > 0 ? boxcls : "#jedatebox";
        jet.format = $($(boxcls).attr(jefix)).length > 0 ? $(jet.boxelem).attr("jeformat") : (opts.format || config.format);
        jet.formatType = jet.checkFormat(jet.format);
        var that = this, boxCell = $(jet.boxelem),
            isYYMM = jet.testFormat(jet.format,"YYYY-MM"),isYY = jet.testFormat(jet.format,"YYYY");
        var doudStrHtml = '<div class="jedatetop"></div>' +
            '<div class="jedatetopym" style="display: none;"><ul class="ymdropul"></ul></div>' +
            '<ol class="jedaol"></ol><ul class="'+((isYYMM || isYY) ? (isYY ? "jedayy":"jedaym"):"jedaul")+'"></ul>' +
            '<div class="jedateprophms"></div>' +
            '<div class="jedatebot"></div>';
        boxCell.empty().append(doudStrHtml);
        that.generateHtml(opts,boxCell);
    };
    jedfn.isContainhh = function (format) {
        var gethms = jet.checkFormat(format),
            rearr = jet.reMacth("hh-"+gethms.split("hh")[1]);
        return rearr;
    };
    jedfn.generateHtml = function(opts,boxCell){
        var that = this, objCell = that.valCell, weekHtml = "", tmsArr,
            date = new Date(), lang = opts.language || config.language,
            isYYMM = jet.testFormat(jet.format,"YYYY-MM"),
            isYY = jet.testFormat(jet.format,"YYYY"),
            subhh = jet.format.substring(0, 2) == "hh",
            topCell = boxCell.find(".jedatetop"),
            topymCell = boxCell.find(".jedatetopym"),
            olCell  = boxCell.find(".jedaol"),
            hmsCell  = boxCell.find(".jedateprophms"),
            botCell = boxCell.find(".jedatebot");
        if ((objCell.val() || objCell.text()) == "") {
            //目标为空值则获取当前日期时间
            tmsArr = [ date.getFullYear(), date.getMonth() + 1, date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds() ];
            jet.currDate = new Date(tmsArr[0], parseInt(tmsArr[1])-1, tmsArr[2], tmsArr[3], tmsArr[4], tmsArr[5]);
            jet.ymdDate = tmsArr[0] + "-" + jet.digit(tmsArr[1]) + "-" + jet.digit(tmsArr[2]);
        } else {
            var initVal = jet.isValHtml(objCell) ? objCell.val() : objCell.text();
            var inVals = jet.sortDate(initVal,jet.format);
            if(subhh){
                tmsArr = [ date.getFullYear(), date.getMonth() + 1, date.getDate(), inVals[0], inVals[1]||date.getMinutes(), inVals[2]||date.getSeconds() ];
                jet.currDate = new Date(date.getFullYear(), date.getMonth()-1, date.getDate());
            }else{
                tmsArr = [ inVals[0], inVals[1] == undefined ? date.getMonth() + 1 : inVals[1], inVals[2] == undefined ?  date.getDate() : inVals[2], inVals[3] == undefined ? date.getHours() : inVals[3], inVals[4] == undefined ? date.getMinutes() : inVals[4], inVals[5] == undefined ? date.getSeconds() :inVals[5] ];
                jet.currDate = new Date(tmsArr[0], parseInt(tmsArr[1])-1,  tmsArr[2], tmsArr[3], tmsArr[4], tmsArr[5]);
                jet.ymdDate = tmsArr[0] + "-" + jet.digit(tmsArr[1]) + "-" + jet.digit(tmsArr[2]);
            }
        }
        //设置date的html片段
        var topStrHtml = '<div class="jedateym" style="width:50%;"><i class="prev triangle monthprev"></i><span class="jedatemm" ym="12"><em class="jedatemonth"></em><em class="pndrop"></em></span><i class="next triangle monthnext"></i></div>' +
            '<div class="jedateym" style="width:50%;"><i class="prev triangle yearprev"></i><span class="jedateyy" ym="24"><em class="jedateyear"></em><em class="pndrop"></em></span><i class="next triangle yearnext"></i></div>';

        var hmsStrHtml = '<div class="jedatepropcon"><div class="jedatehmstitle">'+(lang.name == "cn" ? "\u65F6\u95F4\u9009\u62E9":"Time to choose")+'<div class="jedatehmsclose">&times;</div></div>' +
            '<div class="jedateproptext">'+lang.times[0]+'</div><div class="jedateproptext">'+lang.times[1]+'</div><div class="jedateproptext">'+lang.times[2]+'</div>' +
            '<div class="jedatehmscon jedateprophours"></div><div class="jedatehmscon jedatepropminutes"></div><div class="jedatehmscon jedatepropseconds"></div></div>';

        var botStrHtml = '<div class="botflex jedatehmsshde"><ul class="jedatehms"><li><input type="text" maxlength="2" numval="23" hms="0"></li><i>:</i><li><input type="text" maxlength="2" numval="59" hms="1"></li><i>:</i><li><input type="text" maxlength="2" numval="59" hms="2"></li></ul></div>' +
            '<div class="botflex jedatebtn"><span class="jedateok">'+lang.yes+'</span><span class="jedatetodaymonth">'+lang.today+'</span><span class="jedateclear">'+lang.clear+'</span></div>';

        var ymBtnHtml = '<p><span class="jedateymchle">&lt;&lt;</span><span class="jedateymchri">&gt;&gt;</span><span class="jedateymchok">'+lang.close+'</span></p>';
        //设置html到对应的DOM中
        if (subhh){
            boxCell.children(".jedatetop,.jedatetopym,.jedaol,.jedaul").remove();
        }else {
            topCell.append(topStrHtml);
        }
        if (isYYMM || isYY){
            boxCell.children(".jedatetopym,.jedaol").remove();
            //设置格式为 YYYY-MM 类型的
            botCell.find(".jedatetodaymonth").hide();
            topCell.children().css({"width": "100%"}).first().remove();
            topCell.children().find(".pndrop").remove();
            boxCell.find(isYYMM ? ".jedaym" : ".jedayy").append(that.eachYM(opts,tmsArr[0], tmsArr[1],boxCell));
        }else {
            topymCell.append(ymBtnHtml);
            olCell.append(function () {
                //设置周显示
                $.each(lang.weeks, function(i, week) {
                    weekHtml += '<li class="weeks" data-week="' + week + '">' + week + "</li>";
                });
                return weekHtml;
            });
        }
        botCell.append(botStrHtml);
        //是否显示清除按钮
        jet.isBool(opts.isClear) ? "" : botCell.find(".jedateclear").hide();
        //是否显示今天按钮
        (isYYMM || isYY || subhh) ? botCell.find(".jedatetodaymonth").hide() : (jet.isBool(opts.isToday) ? "" : botCell.find(".jedatetodaymonth").hide());

        //是否显示确认按钮
        jet.isBool(opts.isok) ? "" : botCell.find(".jedateok").hide();

        //开始循环创建日期--天
        if(/\DD/.test(jet.format)){
            that.createDaysHtml(tmsArr[0], tmsArr[1], opts,boxCell);
        }
        //设置时分秒
        if(/\hh/.test(jet.format)) {
            hmsCell.append(hmsStrHtml).addClass(subhh ? "jedatepropfix" :"jedateproppos").css({"display": subhh ? "block" : "none"});
            var hmsarr = that.isContainhh(jet.format),hmsset = opts.hmsSetVal||{},
                hmsobj = ((objCell.val() || objCell.text()) == "" && jet.isObj(hmsset)) ? hmsset : {"hh":tmsArr[3], "mm":tmsArr[4], "ss":tmsArr[5]};
            $.each(["hh", "mm", "ss"], function (i, hms) {
                var undezz = (hmsarr[i] == undefined || hmsarr[i] == "zz"),
                    inphms = botCell.find('input[hms=' + i + ']');
                inphms.val(undezz ? "00":jet.digit(hmsobj[hms])).attr("readOnly",jet.isBool(opts.ishmsVal) ? false:true);
                if (hmsarr.length != 0 && undezz)  inphms.attr("disabled", true);
            });
        }else {
            botCell.find(".jedatehmsshde").hide();
            botCell.find(".jedatebtn").css({width:"100%"});
        }
        //是否开启时间选择
        if(!jet.isBool(opts.isTime)){
            botCell.find(".botflex").css({width:"100%"});
            botCell.find(".jedatehmsshde").hide();
        }
        //绑定各个事件
        that.eventsDate(opts,boxCell);
        if($($(jet.boxelem).attr(jefix)).length == 0){
            var datepos = opts.position||[];
            datepos.length > 0 ? boxCell.css({"top":datepos[0],"left":datepos[1]}) : that.orien(boxCell, objCell);
        }
        setTimeout(function () {
            opts.success && opts.success(objCell);
        }, 50);
    };
    //循环生成日历
    jedfn.createDaysHtml = function(ys, ms, opts, boxCell){
        var that = this, year = parseInt(ys), month = parseInt(ms), dateHtml = "",
            count = 0,lang = opts.language || config.language, ends = opts.isvalid||[],
            minArr = jet.reMacth(jet.minDate), minNum = minArr[0] + minArr[1] + minArr[2],
            maxArr = jet.reMacth(jet.maxDate), maxNum = maxArr[0] + maxArr[1] + maxArr[2],
            firstWeek = new Date(year, month - 1, 1).getDay() || 7,
            daysNum = jet.getDaysNum(year, month), prevM = jet.getPrevMonth(year, month),
            prevDaysNum = jet.getDaysNum(year, prevM.m), nextM = jet.getNextMonth(year, month),
            currOne = jet.currDate.getFullYear() + "-" + jet.digit(jet.currDate.getMonth() + 1) + "-" + jet.digit(1),
            thisOne = year + "-" + jet.digit(month) + "-" + jet.digit(1);
        boxCell.find(".jedateyear").attr("year", year).text(year+(lang.name == "cn" ? "\u5e74":""));
        boxCell.find(".jedatemonth").attr("month", month).text(month+(lang.name == "cn" ? "\u6708":""));
        //设置时间标注
        var mark = function (my, mm, md) {
            var Marks = opts.marks, contains = function(arr, obj) {
                var len = arr.length;
                while (len--) {
                    if (arr[len] === obj) return true;
                }
                return false;
            };
            return $.isArray(Marks) && Marks.length > 0 && contains(Marks, my + "-" + jet.digit(mm) + "-" + jet.digit(md)) ? '<i class="marks"></i>' :"";
        };
        //是否显示节日
        var isfestival = function(y, m ,d) {
            var festivalStr;
            if(opts.festival == true && lang.name == "cn"){
                var lunar = jeLunar(y, m - 1, d), feslunar = (lunar.solarFestival || lunar.lunarFestival),
                    lunartext = (feslunar && lunar.jieqi) != "" ? feslunar : (lunar.jieqi || lunar.showInLunar);
                festivalStr = '<p><span class="solar">' + d + '</span><span class="lunar">' + lunartext + '</span></p>';
            }else{
                festivalStr = '<p class="nolunar">' + d + '</p>';
            }
            return festivalStr;
        };
        //判断是否在限制的日期之中
        var dateOfLimit = function(Y, M, D, isMonth){
            var thatNum = (Y + "-" + jet.digit(M) + "-" + jet.digit(D)).replace(/\-/g, '');
            if(isMonth){
                if (parseInt(thatNum) >= parseInt(minNum) && parseInt(thatNum) <= parseInt(maxNum)) return true;
            }else {
                if (parseInt(minNum) > parseInt(thatNum) || parseInt(maxNum) < parseInt(thatNum)) return true;
            }
        };
        //判断禁用启用是长度，并设置成正则
        if(ends.length > 0){
            var dayreg = new RegExp(ends[0].replace(/,/g,"|"));
        }
        //上一月剩余天数
        for (var p = prevDaysNum - firstWeek + 1; p <= prevDaysNum; p++, count++) {
            var pmark = mark(prevM.y,prevM.m,p), pCls;
            if(ends.length > 0){
                if (dateOfLimit(prevM.y, prevM.m, p, false)){
                    pCls = "disabled";
                }else {
                    if(dayreg.test(p)){
                        pCls = ends[1] == true ? "other" : "disabled";
                    }else{
                        pCls = ends[1] == true ? "disabled" : "other";
                    }
                }
            }else {
                pCls = dateOfLimit(prevM.y, prevM.m, p, false) ? "disabled" : "other";
            }
            dateHtml += '<li data-ymd="'+prevM.y+'-'+prevM.m+'-'+p+'" class='+pCls+'>'+(isfestival(prevM.y,prevM.m,p) + pmark)+'</li>';
        }
        //本月的天数
        for(var b = 1; b <= daysNum; b++, count++){
            var bCls = "", bmark = mark(year,month,b),
                thisDate = (year + "-" + jet.digit(month) + "-" + jet.digit(b)); //本月当前日期
            //判断日期是否在限制范围中，并高亮选中的日期
            if(dateOfLimit(year, month, b, true)){
                if(ends.length > 0){
                    if (jet.ymdDate == thisDate){
                        bCls = jet.ymdDate == thisDate ? "action" : (currOne != thisOne && thisOne == thisDate ? "action" : "");
                    }else {
                        if(dayreg.test(b)){
                            bCls = ends[1] == true ? "" : "disabled";
                        }else{
                            bCls = ends[1] == true ? "disabled":"";
                        }
                    }
                }else {
                    bCls = jet.ymdDate == thisDate ? "action" : (currOne != thisOne && thisOne == thisDate ? "action" : "");
                }
            }else{
                bCls = "disabled";
            }
            if (bCls == "action") boxCell.children("ul").attr("dateval",year+'-'+month+'-'+b);
            dateHtml += '<li data-ymd="'+year+'-'+month+'-'+b+'" class='+(bCls != "" ? bCls : "")+'>'+(isfestival(year,month,b) + bmark)+'</li>';
        }
        //下一月开始天数
        for(var n = 1, nlen = 42 - count; n <= nlen; n++){
            var nmark = mark(nextM.y,nextM.m,n), nCls;
            if(ends.length > 0){
                if (dateOfLimit(nextM.y, nextM.m, n, false)){
                    nCls = "disabled";
                }else {
                    if(dayreg.test(n)){
                        nCls = ends[1] == true ? "other" : "disabled";
                    }else{
                        nCls = ends[1] == true ? "disabled":"other";
                    }
                }
            }else {
                nCls = dateOfLimit(nextM.y, nextM.m, n, false) ? "disabled" : "other";
            }
            dateHtml += '<li data-ymd="'+nextM.y+'-'+nextM.m+'-'+n+'" class='+nCls+'>'+(isfestival(nextM.y,nextM.m,n) + nmark)+'</li>';
        }
        //把日期拼接起来并插入
        boxCell.find(".jedaul").empty().html(dateHtml);
        that.chooseDays(opts,boxCell);
    };
    jedfn.eachStrhms = function(opts,boxCell) {
        var that = this, hmsArr = [],
            mins = jet.minDate.split(" ")[1] == undefined ? "00:00:00" : jet.minDate.split(" ")[1],
            maxs = jet.maxDate.split(" ")[1] == undefined ? "00:00:00" : jet.maxDate.split(" ")[1],
            minhms = jet.reMacth(mins), maxhms = jet.reMacth(maxs);
        //生成时分秒
        $.each([ 24, 60, 60 ], function(i, len) {
            var hmsStr = "", hmsCls = "", hmsarr = that.isContainhh(jet.format),
                textem = boxCell.find(".jedatehms input").eq(i).val();
            for (var h = 0; h < len; h++) {
                h = jet.digit(h);
                if (jet.isBool(opts.hmsLimit)) {
                    hmsCls = (hmsarr.length != 0 && (hmsarr[i] == undefined || hmsarr[i] == "zz")) ? "disabled" : (textem == h ? "action" : "");
                }else {
                    //判断限制时间范围的状态
                    if (h < minhms[i] || h > maxhms[i]){
                        hmsCls = h == textem ? "disabled action" : "disabled";
                    }else {
                        hmsCls = h == textem ? "action" :"";
                    };
                }
                hmsStr += '<p class="' + hmsCls + '">' + h + "</p>";
            }
            hmsArr.push(hmsStr);
        });
        return hmsArr;
    };
    //循环生成年或月
    jedfn.eachYM = function(opts,y, m,boxCell) {
        var ymStr = "",lang = opts.language || config.language,ymtext,
            objCell = this.valCell, date = new Date();
        if (jet.testFormat(jet.format,"YYYY")){
            jet.yearArr = new Array(15);
            $.each(jet.yearArr, function(i) {
                var minArr = jet.parseMatch(jet.minDate), maxArr = jet.parseMatch(jet.maxDate),
                    minY = minArr[0], maxY = maxArr[0], year = y - 7 + i,
                    objyear = jet.reMacth(jet.isValHtml(objCell) ? objCell.val() : objCell.text()),
                    getyear = ((objCell.val() || objCell.text()) == "") ? date.getFullYear() : objyear[0];
                if (year < minY || year > maxY) {
                    ymStr += "<li class='disabled' yy='" + year + "'>" + year + "</li>";
                } else {
                    ymStr += "<li class='"+(parseInt(getyear) == year ? "action" :"")+"' yy='" + year + "'>" + year + "</li>";
                }
                if(parseInt(getyear) == year) boxCell.children("ul").attr("dateval",year);
            });
            ymtext = y ;
        }else {
            $.each(lang.month, function(i, val) {
                var minArr = jet.parseMatch(jet.minDate), maxArr = jet.parseMatch(jet.maxDate),
                    thisDate = new Date(y, jet.digit(val), "01"), minTime = new Date(minArr[0], minArr[1], minArr[2]), maxTime = new Date(maxArr[0], maxArr[1], maxArr[2]);
                if (thisDate < minTime || thisDate > maxTime) {
                    ymStr += "<li class='disabled' ym='" + y + "-" + jet.digit(val) + "'>" + y + "-" + jet.digit(val) + "</li>";
                } else {
                    ymStr += "<li class='"+(m == val ? "action" :"")+"' ym='" + y + "-" + jet.digit(val) + "'>" + y + "-" + jet.digit(val) + "</li>";
                }
                if(m == val) boxCell.children("ul").attr("dateval",y + "-" + jet.digit(val));
            });
            ymtext = y + "-" + jet.digit(m);
        }
        boxCell.find(".jedatetop .jedateyear").text(ymtext);
        return ymStr;
    };
    //方位辨别
    jedfn.orien = function(obj, self, pos) {
        var tops, leris, ortop, orleri, rect = jet.fixed ? self[0].getBoundingClientRect() : obj[0].getBoundingClientRect();
        if(jet.fixed) {
            //根据目标元素计算弹层位置
            leris = rect.right + obj.outerWidth() / 1.5 >= jet.winarea(1) ? rect.right - obj.outerWidth() : rect.left + (pos ? 0 : jet.docScroll(1));
            tops = rect.bottom + obj.outerHeight() / 1 <= jet.winarea() ? rect.bottom - 1 : rect.top > obj.outerHeight() / 1.5 ? rect.top - obj.outerHeight() - 1 : jet.winarea() - obj.outerHeight();
            ortop = Math.max(tops + (pos ? 0 :jet.docScroll()) + 1, 1) + "px", orleri = leris + "px";
        }else{
            //弹层位置位于页面上下左右居中
            ortop = "50%", orleri = "50%";
            obj.css({"margin-top":-(rect.height / 2),"margin-left":-(rect.width / 2)});
        }
        obj.css({"top":ortop,"left":orleri});
    };
    //农历方位辨别
    jedfn.lunarOrien = function(obj, self, pos) {
        var tops, leris, ortop, orleri, rect =self[0].getBoundingClientRect();
        leris = rect.right + obj[0].offsetWidth / 1.5 >= jet.winarea(1) ? rect.right - obj[0].offsetWidth : rect.left + (pos ? 0 : jet.docScroll(1));
        tops = rect.bottom + obj[0].offsetHeight / 1 <= jet.winarea() ? rect.bottom - 1 : rect.top > obj[0].offsetHeight / 1.5 ? rect.top - obj[0].offsetHeight - 1 : jet.winarea() - obj[0].offsetHeight;
        ortop = Math.max(tops + (pos ? 0 :jet.docScroll()) + 1, 1) + "px", orleri = leris + "px";
        return {top: ortop, left: orleri }
    };
    //关闭层
    jedfn.dateClose = function() {
        if($($(jet.boxelem).attr(jefix)).length == 0) {
            $(jet.boxelem).remove();
        }
    };
    //为日期绑定各类事件
    jedfn.eventsDate = function(opts,boxCell) {
        var that = this, elemCell = that.valCell, lang = opts.language || config.language,
            ishhmat = jet.checkFormat(jet.format).substring(0, 2) == "hh";
        if (!ishhmat) {
            that.chooseYearMonth(opts, boxCell);
        }
        if (jet.testFormat(jet.format,"YYYY") || jet.testFormat(jet.format,"YYYY-MM")){
            that.preNextYearMonth(opts,boxCell);
            that.onlyYMevents(opts,boxCell);
        }
        //判断日期格式中是否包小时（hh）
        if(/\hh/.test(jet.format)){
            var hsCls = boxCell.find(".jedateprophours"),
                msCls = boxCell.find(".jedatepropminutes"),
                ssCls = boxCell.find(".jedatepropseconds"),
                prophms = boxCell.find(".jedateprophms"),
                screlTopNum = 155;
            var sethmsStrHtml = function () {
                var hmsStr = that.eachStrhms(opts, boxCell), hmsarr = that.isContainhh(jet.format);
                prophms.css("display","block");
                $.each([ hsCls, msCls, ssCls ], function(i, hmsCls) {
                    if (hmsCls.html() == "") hmsCls.html(hmsStr[i]);
                });
                //计算当前时分秒的位置
                $.each([ "hours", "minutes", "seconds" ], function(i, hms) {
                    var hmsCls = boxCell.find(".jedateprop" + hms),
                        achmsCls = hmsCls.find(".action"),
                        onhmsPCls = hmsCls.find("p");
                    if(hmsarr.length != 0 && (hmsarr[i] != undefined && hmsarr[i] != "zz")) {
                        hmsCls[0].scrollTop = achmsCls[0].offsetTop - screlTopNum;
                    }
                    onhmsPCls.on("click", function() {
                        var _this = $(this);
                        if (_this.hasClass("disabled")) return;
                        _this.addClass('action').siblings().removeClass('action');
                        boxCell.find(".jedatebot .jedatehms input").eq(i).val(jet.digit(_this.text()));
                        if (!ishhmat) boxCell.find(".jedateprophms").hide();
                    });
                })

            };
            //如果日期格式中前2个否包小时（hh），就直接显示，否则点击显示
            if(ishhmat){
                sethmsStrHtml();
                prophms.find(".jedatehmsclose").css("display","none");
            }else {
                boxCell.find(".jedatehms").on("click", function() {
                    sethmsStrHtml();
                    //关闭时分秒层
                    !ishhmat && prophms.find(".jedatehmsclose").on("click", function() {
                        prophms.css("display","none");
                    });
                })
            }
        }
        //检查时间输入值，并对应到相应位置
        boxCell.find(".jedatehms input").on("keyup", function() {
            var _this = $(this), thatval = _this.val(),
                hmsarr = that.isContainhh(jet.format),
                hmsVal = parseInt(_this.attr("numval")),
                thatitem = parseInt(_this.attr("hms"));
            _this.val(thatval.replace(/\D/g,""));
            //判断输入值是否大于所设值
            if(thatval > hmsVal){
                _this.val(hmsVal);
                var onval = lang.name == "cn" ? "\u8F93\u5165\u503C\u4E0D\u80FD\u5927\u4E8E " : "The input value is not greater than ";
                alert(onval+hmsVal);
            }
            if(thatval == "") _this.val("00");
            boxCell.find(".jedatehmscon").eq(thatitem).children().each(function(){
                $(this).removeClass("action");
            });
            boxCell.find(".jedatehmscon").eq(thatitem).children().eq(parseInt(_this.val().replace(/^0/g,''))).addClass("action");
            $.each([ "hours", "minutes", "seconds" ], function(i, hms) {
                var hmsCls = boxCell.find(".jedateprop" + hms),
                    achmsCls = hmsCls.find(".action");
                if(hmsarr.length != 0 && hmsarr[i] != undefined) {
                    hmsCls[0].scrollTop = achmsCls[0].offsetTop - screlTopNum;
                }
            });
        });
        //清空按钮清空日期时间
        boxCell.find(".jedatebot .jedateclear").on("click", function(ev) {
            ev.stopPropagation();
            var clearVal = jet.isValHtml(elemCell) ? elemCell.val() :elemCell.text();
            jet.isValHtml(elemCell) ? elemCell.val("") :elemCell.text("");
            that.dateClose();
            if (clearVal != "") {
                if (jet.isBool(opts.clearRestore)){
                    jet.minDate = opts.startMin || jet.minDate;
                    jet.maxDate = opts.startMax || jet.maxDate;
                }
                if ($.isFunction(opts.clearfun) || opts.clearfun != null) opts.clearfun(elemCell,clearVal);
            }
        });
        //今天按钮设置日期时间
        boxCell.find(".jedatebot .jedatetodaymonth").on("click", function() {
            var newDate = new Date(), toTime = [ newDate.getFullYear(), newDate.getMonth() + 1, newDate.getDate(), newDate.getHours(), newDate.getMinutes(), newDate.getSeconds() ],
                gettoDate = jet.parse([ toTime[0], toTime[1], toTime[2] ], [ toTime[3], toTime[4], toTime[5] ], jet.format),
                toDate = newDate.getFullYear()+"-"+jet.digit(newDate.getMonth() + 1)+"-"+jet.digit(newDate.getDate())+" "+jet.digit(newDate.getHours())+":"+jet.digit(newDate.getMinutes())+":"+jet.digit(newDate.getSeconds());
            jet.isValHtml(elemCell) ? elemCell.val(gettoDate) :jet.text(gettoDate);
            if($(boxCell.attr(jefix)).length > 0){
                var fixCell = "#"+boxCell.attr("id");
                that.setHtml(opts,fixCell);
            }
            that.dateClose();
            if ($.isFunction(opts.choosefun) || opts.choosefun != null) opts.choosefun(elemCell,gettoDate,toDate);
        });
        //确认按钮设置日期时间
        boxCell.find(".jedatebot .jedateok").on("click", function(ev) {
            ev.stopPropagation();
            var date = new Date(),
                okhms = (function() {
                    var hmsArr = [];
                    boxCell.find(".jedatehms input").each(function() {
                        var disattr = $(this).attr('disabled');
                        if(typeof(disattr) == "undefined") hmsArr.push($(this).val());
                    });
                    return hmsArr;
                })();
            var okymd = ishhmat ? [date.getFullYear(),date.getMonth() + 1,date.getDate()] : jet.reMacth(boxCell.children("ul").attr("dateval")),
                okformat = $($(jet.boxelem).attr(jefix)).length > 0 ? boxCell.attr("jeformat") : jet.format,
                okVal = jet.parse([parseInt(okymd[0]), parseInt(okymd[1]), parseInt(okymd[2])], [okhms[0]||00, okhms[1]||00, okhms[2]||00], okformat),
                okdate = (okymd[0]||date.getFullYear())+"-"+jet.digit(okymd[1]||date.getMonth() + 1)+"-"+jet.digit(okymd[2]||date.getDate())+" "+jet.digit(okhms[0]||00)+":"+jet.digit(okhms[1]||00)+":"+jet.digit(okhms[2]||00);

            jet.isValHtml(elemCell) ? elemCell.val(okVal) :elemCell.text(okVal);
            that.dateClose();
            if ($.isFunction(opts.okfun) || opts.okfun != null) opts.okfun(elemCell,okVal,okdate);
        });
        //点击空白处隐藏
        $(document).on("mouseup scroll", function(ev) {
            ev.stopPropagation();
            if (jet.boxelem == "#jedatebox"){
                var box = $(jet.boxelem);
                if (box && box.css("display") !== "none")  box.remove();
                if($("#jedatetipscon").length > 0) $("#jedatetipscon").remove();
            }
        });
        $(jet.boxelem).on("mouseup", function(ev) {
            ev.stopPropagation();
        });
    };
    //选择日期
    jedfn.chooseDays = function(opts,boxCell) {
        var that = this, objCell = that.valCell, date = new Date(), lang = opts.language || config.language;
        boxCell.find(".jedaul li").on("click", function(ev) {
            var thisformat = $(boxCell.attr(jefix)).length > 0 ? boxCell.attr("jeformat") : jet.format;
            var _that = $(this), liTms = [];
            if (_that.hasClass("disabled")) return;
            ev.stopPropagation();
            //获取时分秒的集合
            boxCell.find(".jedatehms input").each(function() {
                liTms.push($(this).val());
            });
            var dateArr = jet.reMacth(_that.attr("data-ymd")),
                getDateVal = jet.parse([ dateArr[0], dateArr[1], dateArr[2] ], [ liTms[0], liTms[1], liTms[2] ], thisformat),
                wdate = (dateArr[0]||date.getFullYear())+"-"+jet.digit(dateArr[1]||date.getMonth() + 1)+"-"+jet.digit(dateArr[2]||date.getDate())+" "+jet.digit(liTms[0])+":"+jet.digit(liTms[1])+":"+jet.digit(liTms[2]);
            jet.isValHtml(objCell) ? objCell.val(getDateVal) :objCell.text(getDateVal);
            if($(boxCell.attr(jefix)).length > 0){
                var fixCell = "#"+boxCell.attr("id");
                that.setHtml(opts,fixCell);
            }else {
                that.dateClose();
            }
            opts.festival && $("#jedatetipscon").remove();
            if ($.isFunction(opts.choosefun) || opts.choosefun != null){
                opts.choosefun && opts.choosefun(objCell,getDateVal,wdate);
            }
        });
        if(opts.festival && lang.name == "cn") {
            //鼠标进入提示框出现
            boxCell.find(".jedaul li").on("mouseover", function () {
                var _this = $(this), atlunar = jet.reMacth(_this.attr("data-ymd")),
                    tipDiv = $("<div/>",{"id":"jedatetipscon","class":"jedatetipscon"}),
                    lunar = jeLunar(parseInt(atlunar[0]), parseInt(atlunar[1]) - 1, parseInt(atlunar[2]));
                var tiphtml = '<p>' + lunar.solarYear + '\u5E74' + lunar.solarMonth + '\u6708' + lunar.solarDate + '\u65E5 ' + lunar.inWeekDays + '</p><p class="red">\u519C\u5386：' + lunar.shengxiao + '\u5E74 ' + lunar.lnongMonth + '\u6708' + lunar.lnongDate + '</p><p>' + lunar.ganzhiYear + '\u5E74 ' + lunar.ganzhiMonth + '\u6708 ' + lunar.ganzhiDate + '\u65E5</p>';
                var Fesjieri = (lunar.solarFestival || lunar.lunarFestival) != "" ? '<p class="red">' + ("\u8282\u65E5："+lunar.solarFestival + lunar.lunarFestival) + '</p>' : "";
                var Fesjieqi = lunar.jieqi != "" ? '<p class="red">'+(lunar.jieqi != "" ? "\u8282\u6C14："+lunar.jieqi : "") + '</p>': "";
                var tiptext = (lunar.solarFestival || lunar.lunarFestival || lunar.jieqi) != "" ? (Fesjieri + Fesjieqi) : "";
                //生成提示框到文档中
                $("body").append(tipDiv);
                tipDiv.html(tiphtml + tiptext);
                //获取并设置农历提示框出现的位置
                var tipPos = jedfn.lunarOrien(tipDiv, _this);
                tipDiv.css({"z-index":  (opts.zIndex == undefined ? 2099 + 5 : opts.zIndex + 5),top:tipPos.top,left:tipPos.left,position:"absolute",display:"block"});
            }).on( "mouseout", function () { //鼠标移除提示框消失
                if($("#jedatetipscon").length > 0) $("#jedatetipscon").remove();
            });
        }
    };
    //切换年月 与 下拉选择年月的事件
    jedfn.chooseYearMonth = function (opts,boxCell) {
        var that = this, yPre = boxCell.find(".yearprev"), yNext = boxCell.find(".yearnext"),
            mPre = boxCell.find(".monthprev"), mNext = boxCell.find(".monthnext"),
            jetopym = boxCell.find(".jedatetopym"), jedateyy = boxCell.find(".jedateyy"),
            jedatemm = boxCell.find(".jedatemm"), jedateyear = boxCell.find(".jedateyy .jedateyear"),
            jedatemonth = boxCell.find(".jedatemm .jedatemonth"),lang = opts.language || config.language,
            mchri = boxCell.find(".jedateymchri"), mchle = boxCell.find(".jedateymchle");
        var minArr = jet.reMacth(jet.minDate), minNum = minArr[0] + minArr[1],
            maxArr = jet.reMacth(jet.maxDate), maxNum = maxArr[0] + maxArr[1];
        //循环生成年
        var eachYears = function(YY) {
                var eachStr = "", ycls;
                $.each(new Array(15), function(i,v) {
                    if (i === 7) {
                        var getyear = jedateyear.attr("year");
                        ycls = (parseInt(YY) >= parseInt(minArr[0]) && parseInt(YY) <= parseInt(maxArr[0])) ? (getyear == YY ? 'class="action"' :"") : 'class="disabled"';
                        eachStr += "<li " + ycls + ' yy="' + YY + '">' + (YY+(lang.name == "cn" ? "\u5e74":"")) + "</li>";
                    } else {
                        ycls = (parseInt(YY - 7 + i) >= parseInt(minArr[0]) && parseInt(YY - 7 + i) <= parseInt(maxArr[0])) ? "" : 'class="disabled"';
                        eachStr += '<li ' + ycls + ' yy="' + (YY - 7 + i) + '">' + (YY - 7 + i+(lang.name == "cn" ? "\u5e74":"")) + "</li>";
                    }
                });
                return eachStr;
            },
            //循环生成月
            eachYearMonth =function (YY, ymlen) {
                var ymStr = "";
                if (ymlen == 12) {
                    $.each(lang.month, function(i, val) {
                        var getmonth = jedatemonth.attr("month"), val = jet.digit(val);
                        var mcls = (parseInt(jedateyear.attr("year") + val) >= parseInt(minNum) && parseInt(jedateyear.attr("year") + val) <= parseInt(maxNum)) ? (jet.digit(getmonth) == val ? "action" :"") : "disabled";
                        ymStr += "<li class='"+ mcls +"' mm='" + val + "'>" + (val+(lang.name == "cn" ? "\u6708":"")) + "</li>";
                    });
                    $.each([ mchri, mchle ], function(c, cls) {
                        cls.hide();
                    });
                } else {
                    ymStr = eachYears(YY);
                    $.each([ mchri, mchle ], function(c, cls) {
                        cls.show();
                    });
                }
                jetopym.removeClass( ymlen == 12 ? "jedatesety" :"jedatesetm").addClass(ymlen == 12 ? "jedatesetm" :"jedatesety");
                boxCell.find(".jedatetopym .ymdropul").html(ymStr);
                jetopym.show();
            };
        //切换年
        $.each([ yPre, yNext ], function(i, cls) {
            cls.on("click", function(ev) {
                if(boxCell.find(".jedatetopym").css("display") == "block") return;
                ev.stopPropagation();
                var year = parseInt(jedateyear.attr("year")), month = parseInt(jedatemonth.attr("month")),
                    pnYear = cls == yPre ? --year : ++year;
                that.createDaysHtml(pnYear, month, opts, boxCell);
            });
        });
        //切换月
        $.each([ mPre, mNext ], function(i, cls) {
            cls.on("click", function(ev) {
                if(boxCell.find(".jedatetopym").css("display") == "block") return;
                ev.stopPropagation();
                var year = parseInt(jedateyear.attr("year")), month = parseInt(jedatemonth.attr("month")),
                    PrevYM = jet.getPrevMonth(year, month), NextYM = jet.getNextMonth(year, month);
                cls == mPre  ? that.createDaysHtml(PrevYM.y, PrevYM.m, opts, boxCell) : that.createDaysHtml(NextYM.y, NextYM.m, opts, boxCell);
            });
        });
        //下拉选择 年或月
        $.each([ jedateyy, jedatemm ], function(i, cls) {
            cls.on("click",function () {
                var clsthat = $(this), ymVal = clsthat.attr("ym"),
                    yearAttr = parseInt(jedateyear.attr("year")),
                    dropchoose = function () {
                        boxCell.find(".ymdropul li").on("click", function(ev) {
                            var _this = $(this), Years = jedateyy == cls ? parseInt(_this.attr("yy")) : parseInt(jedateyear.attr("year")),
                                Months = jedateyy == cls ? parseInt(jedatemonth.attr("month")) : jet.digit(parseInt(_this.attr("mm")));
                            if (_this.hasClass("disabled")) return;
                            ev.stopPropagation();
                            if(jedateyy == cls){
                                jedateyear.attr("year", Years).html(Years + (lang.name == "cn" ? "\u5e74":""));
                            }else {
                                jedatemonth.attr("month", Months).html(Months + (lang.name == "cn" ? "\u6708":""));
                            }
                            jetopym.hide();
                            that.createDaysHtml(Years, Months, opts, boxCell);
                        });
                    };
                eachYearMonth(yearAttr, ymVal);
                dropchoose();
                //关闭下拉选择
                boxCell.find(".jedateymchok").on("click", function(ev) {
                    ev.stopPropagation();
                    jetopym.hide();
                });
                $.each([ mchle, mchri ], function(d, mcls) {
                    mcls.on("click", function(ev) {
                        ev.stopPropagation();
                        d == 0 ? yearAttr -= 15 :yearAttr += 15;
                        var mchStr = eachYears(yearAttr);
                        boxCell.find(".jedatetopym .ymdropul").html(mchStr);
                        dropchoose();
                    });
                });
            })
        });

    };
    //年月情况下的事件绑定
    jedfn.preNextYearMonth = function(opts,boxCell){
        var that = this, elemCell = that.valCell,
            newDate = new Date(), isYY = jet.testFormat(jet.format,"YYYY"),
            ymCls = boxCell.find(isYY ? ".jedayy li" : ".jedaym li");
        //选择年月
        ymCls.on("click", function (ev) {
            if ($(this).hasClass("disabled")) return;    //判断是否为禁选状态
            ev.stopPropagation();
            var atYM =  isYY ? jet.reMacth($(this).attr("yy")) : jet.reMacth($(this).attr("ym")),
                getYMDate = isYY ? jet.parse([atYM[0], newDate.getMonth() + 1, 1], [0, 0, 0], jet.format) : jet.parse([atYM[0], atYM[1], 1], [0, 0, 0], jet.format);
            jet.isValHtml(elemCell) ? elemCell.val(getYMDate) : elemCell.text(getYMDate);
            that.dateClose();
            if ($.isFunction(opts.choosefun) || opts.choosefun != null) opts.choosefun(elemCell, getYMDate);
        });
    };
    //仅年月情况下的点击
    jedfn.onlyYMevents = function(opts,boxCell) {
        var that = this, ymVal,newDate = new Date(),
            isYY = jet.testFormat(jet.format,"YYYY"),
            ymPre = boxCell.find(".jedateym .prev"),
            ymNext = boxCell.find(".jedateym .next"),
            onymVal = jet.reMacth(boxCell.children("ul").attr("dateval")),
            ony = parseInt(onymVal[0]), onm = parseInt(onymVal[1]);
        $.each([ ymPre, ymNext ], function(i, cls) {
            cls.on("click", function(ev) {
                ev.stopPropagation();
                if(isYY){
                    ymVal = cls == ymPre ? boxCell.find(".jedayy li").eq(0).attr("yy") : boxCell.find(".jedayy li").eq(jet.yearArr.length-1).attr("yy");
                    boxCell.find(".jedayy").html(that.eachYM(opts,ymVal, newDate.getMonth() + 1,boxCell));
                }else{
                    ymVal = cls == ymPre ? ony -= 1 : ony += 1;
                    boxCell.find(".jedaym").html(that.eachYM(opts,ymVal, onm, boxCell));
                }
                that.preNextYearMonth(opts,boxCell);
            });
        });
    };
    //日期控件版本
    $.dateVer = "3.8.3";
    //返回指定日期
    $.nowDate = function (str,format,date) {
        format = format || 'YYYY-MM-DD hh:mm:ss';
        date = date || [];
        return jet.returnDate(str, format, date);
    };
    $.timeStampDate = function (date,bool,format) {
        format = format || 'YYYY-MM-DD hh:mm:ss';
        if(bool == true){  //将时间戳转换成日期
            var setdate = new Date(parseInt(date.substring(0,10)) * 1e3);
            return jet.parse([ setdate.getFullYear(), jet.digit(setdate.getMonth()), jet.digit(setdate.getDate()) ], [ jet.digit(setdate.getHours()), jet.digit(setdate.getMinutes()), jet.digit(setdate.getSeconds()) ], format);
        }else {  //将日期转换成时间戳
            var tmsArr = jet.reMacth(date),
                newdate = new Date(tmsArr[0],tmsArr[1],tmsArr[2],tmsArr[3],tmsArr[4],tmsArr[5]),
                timeStr = newdate.getTime().toString();
            return timeStr.substr(0, 10);
        }
    };
    //分解时间
    $.splitDate = function (str) {
        var sdate = str.match(/\w+|d+/g);
        return {
            YYYY:parseInt(sdate[0]),MM:parseInt(sdate[1])||00,DD:parseInt(sdate[2])||00,
            hh:parseInt(sdate[3])||00,mm:parseInt(sdate[4])||00,ss:parseInt(sdate[5])||00
        };
    };
    //获取年月日星期
    $.getLunar = function(time){
        if(/\YYYY-MM-DD/.test(jet.formatType)){
            //如果为数字类型的日期对获取到日期的进行替换
            var nocharDate = time.substr(0,4).replace(/^(\d{4})/g,"$1,") + time.substr(4).replace(/(.{2})/g,"$1,"),
                warr = jet.IsNum(time) ? jet.reMacth(nocharDate) : jet.reMacth(time),
                lunars = jeLunar(warr[0], warr[1] - 1, warr[2]);
            return{
                nMonth: lunars.lnongMonth,             //农历月
                nDays: lunars.lnongDate,               //农历日
                yYear: parseInt(lunars.solarYear),     //阳历年
                yMonth: parseInt(lunars.solarMonth),   //阳历月
                yDays: parseInt(lunars.solarDate),     //阳历日
                cWeek: lunars.inWeekDays,              //汉字星期几
                nWeek: lunars.solarWeekDay             //数字星期几
            };
        }
    };
    return jeDate;
});

//农历数据
;(function(root, factory) {
    root.jeLunar = factory(root.jeLunar);
})(this, function(jeLunar) {
    var lunarInfo=[19416,19168,42352,21717,53856,55632,91476,22176,39632,21970,19168,42422,42192,53840,119381,46400,54944,44450,38320,84343,18800,42160,46261,27216,27968,109396,11104,38256,21234,18800,25958,54432,59984,28309,23248,11104,100067,37600,116951,51536,54432,120998,46416,22176,107956,9680,37584,53938,43344,46423,27808,46416,86869,19872,42448,83315,21200,43432,59728,27296,44710,43856,19296,43748,42352,21088,62051,55632,23383,22176,38608,19925,19152,42192,54484,53840,54616,46400,46496,103846,38320,18864,43380,42160,45690,27216,27968,44870,43872,38256,19189,18800,25776,29859,59984,27480,21952,43872,38613,37600,51552,55636,54432,55888,30034,22176,43959,9680,37584,51893,43344,46240,47780,44368,21977,19360,42416,86390,21168,43312,31060,27296,44368,23378,19296,42726,42208,53856,60005,54576,23200,30371,38608,19415,19152,42192,118966,53840,54560,56645,46496,22224,21938,18864,42359,42160,43600,111189,27936,44448],
        sTermInfo = [ 0, 21208, 43467, 63836, 85337, 107014, 128867, 150921, 173149, 195551, 218072, 240693, 263343, 285989, 308563, 331033, 353350, 375494, 397447, 419210, 440795, 462224, 483532, 504758 ];
    var Gan = "甲乙丙丁戊己庚辛壬癸", Zhi = "子丑寅卯辰巳午未申酉戌亥", Animals = "鼠牛虎兔龙蛇马羊猴鸡狗猪";
    var solarTerm = [ "小寒", "大寒", "立春", "雨水", "惊蛰", "春分", "清明", "谷雨", "立夏", "小满",
        "芒种", "夏至", "小暑", "大暑", "立秋", "处暑", "白露", "秋分", "寒露", "霜降", "立冬", "小雪", "大雪", "冬至" ];
    var nStr1 = "日一二三四五六七八九十", nStr2 = "初十廿卅", nStr3 = [ "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "腊"],
        sFtv1 = {
            "0101" : "*1元旦节",         "0202" : "湿地日",
            "0214" : "情人节",           "0308" : "妇女节",
            "0312" : "植树节",           "0315" : "消费者权益日",
            "0401" : "愚人节",           "0422" : "地球日",
            "0501" : "*1劳动节",         "0504" : "青年节",
            "0512" : "护士节",           "0518" : "博物馆日",
            "0520" : "母亲节",           "0601" : "儿童节",
            "0623" : "奥林匹克日",       "0630" : "父亲节",
            "0701" : "建党节",           "0801" : "建军节",
            "0903" : "抗战胜利日",       "0910" : "教师节",
            "1001" : "*3国庆节",         "1201" : "艾滋病日",
            "1224" : "平安夜",           "1225" : "圣诞节"
        },
        sFtv2 = {
            "0100" : "除夕",             "0101" : "*2春节",
            "0115" : "元宵节",           "0505" : "*1端午节",
            "0707" : "七夕节",           "0715" : "中元节",
            "0815" : "*1中秋节",         "0909" : "*1重阳节",
            "1015" : "下元节",           "1208" : "腊八节",
            "1223" : "小年"

        };
    function flunar(Y) {
        var sTerm = function (j, i) {
                var h = new Date((31556925974.7 * (j - 1900) + sTermInfo[i] * 60000) + Date.UTC(1900, 0, 6, 2, 5));
                return (h.getUTCDate())
            },
            d = function (k) {
                var h, j = 348;
                for (h = 32768; h > 8; h >>= 1) {
                    j += (lunarInfo[k - 1900] & h) ? 1 : 0;
                }
                return (j + b(k))
            },
            ymdCyl = function (h) {
                return (Gan.charAt(h % 10) + Zhi.charAt(h % 12))
            },
            b =function (h) {
                var islp = (g(h)) ? ((lunarInfo[h - 1900] & 65536) ? 30 : 29) : (0);
                return islp
            },
            g = function (h) {
                return (lunarInfo[h - 1900] & 15)
            },
            e = function (i, h) {
                return ((lunarInfo[i - 1900] & (65536 >> h)) ? 30 : 29)
            },
            newymd = function (m) {
                var k, j = 0, h = 0, l = new Date(1900, 0, 31), n = (m - l) / 86400000;
                this.dayCyl = n + 40;
                this.monCyl = 14;
                for (k = 1900; k<2050&&n>0; k++) {
                    h = d(k); n -= h;
                    this.monCyl += 12;
                }
                if (n < 0) {
                    n += h; k--;
                    this.monCyl -= 12;
                }
                this.year = k;
                this.yearCyl = k - 1864;
                j = g(k);
                this.isLeap = false;
                for (k = 1; k<13&&n>0; k++) {
                    if (j > 0 && k == (j + 1) && this.isLeap == false) {
                        --k;
                        this.isLeap = true;
                        h = b(this.year);
                    } else {
                        h = e(this.year, k);
                    }
                    if (this.isLeap == true && k == (j + 1)) {
                        this.isLeap = false;
                    }
                    n -= h;
                    if (this.isLeap == false) this.monCyl++;
                }
                if (n == 0 && j > 0 && k == j + 1) {
                    if (this.isLeap) {
                        this.isLeap = false;
                    } else {
                        this.isLeap = true;
                        --k;
                        --this.monCyl;
                    }
                }
                if (n < 0) {
                    n += h; --k;
                    --this.monCyl
                }
                this.month = k;
                this.day = n + 1;
            },
            digit = function (num) {
                return num < 10 ? "0" + (num | 0) :num;
            },
            reymd = function (i, j) {
                var h = i;
                return j.replace(/dd?d?d?|MM?M?M?|yy?y?y?/g, function(k) {
                    switch (k) {
                        case "yyyy":
                            var l = "000" + h.getFullYear();
                            return l.substring(l.length - 4);
                        case "dd": return digit(h.getDate());
                        case "d": return h.getDate().toString();
                        case "MM": return digit((h.getMonth() + 1));
                        case "M": return h.getMonth() + 1;
                    }
                })
            },
            lunarMD = function (i, h) {
                var j;
                switch (i, h) {
                    case 10: j = "初十"; break;
                    case 20: j = "二十"; break;
                    case 30: j = "三十"; break;
                    default:
                        j = nStr2.charAt(Math.floor(h / 10));
                        j += nStr1.charAt(h % 10);
                }
                return (j)
            };
        this.isToday = false;
        this.isRestDay = false;
        this.solarYear = reymd(Y, "yyyy");
        this.solarMonth = reymd(Y, "M");
        this.solarDate = reymd(Y, "d");
        this.solarWeekDay = Y.getDay();
        this.inWeekDays = "星期" + nStr1.charAt(this.solarWeekDay);
        var X = new newymd(Y);
        this.lunarYear = X.year;
        this.shengxiao = Animals.charAt((this.lunarYear - 4) % 12);
        this.lunarMonth = X.month;
        this.lunarIsLeapMonth = X.isLeap;
        this.lnongMonth = this.lunarIsLeapMonth ? "闰" + nStr3[X.month - 1] : nStr3[X.month - 1];
        this.lunarDate = X.day;
        this.showInLunar = this.lnongDate = lunarMD(this.lunarMonth, this.lunarDate);
        if (this.lunarDate == 1) {
            this.showInLunar = this.lnongMonth + "月";
        }
        this.ganzhiYear = ymdCyl(X.yearCyl);
        this.ganzhiMonth = ymdCyl(X.monCyl);
        this.ganzhiDate = ymdCyl(X.dayCyl++);
        this.jieqi = "";
        this.restDays = 0;
        if (sTerm(this.solarYear, (this.solarMonth - 1) * 2) == reymd(Y, "d")) {
            this.showInLunar = this.jieqi = solarTerm[(this.solarMonth - 1) * 2];
        }
        if (sTerm(this.solarYear, (this.solarMonth - 1) * 2 + 1) == reymd(Y, "d")) {
            this.showInLunar = this.jieqi = solarTerm[(this.solarMonth - 1) * 2 + 1];
        }
        if (this.showInLunar == "清明") {
            this.showInLunar = "清明节";
            this.restDays = 1;
        }
        this.solarFestival = sFtv1[reymd(Y, "MM") + reymd(Y, "dd")];
        if (typeof this.solarFestival == "undefined") {
            this.solarFestival = "";
        } else {
            if (/\*(\d)/.test(this.solarFestival)) {
                this.restDays = parseInt(RegExp.$1);
                this.solarFestival = this.solarFestival.replace(/\*\d/, "");
            }
        }
        this.showInLunar = (this.solarFestival == "") ? this.showInLunar : this.solarFestival;
        this.lunarFestival = sFtv2[this.lunarIsLeapMonth ? "00" : digit(this.lunarMonth) + digit(this.lunarDate)];
        if (typeof this.lunarFestival == "undefined") {
            this.lunarFestival = "";
        } else {
            if (/\*(\d)/.test(this.lunarFestival)) {
                this.restDays = (this.restDays > parseInt(RegExp.$1)) ? this.restDays : parseInt(RegExp.$1);
                this.lunarFestival = this.lunarFestival.replace(/\*\d/, "");
            }
        }
        if (this.lunarMonth == 12  && this.lunarDate == e(this.lunarYear, 12)) {
            this.lunarFestival = sFtv2["0100"];
            this.restDays = 1;
        }
        this.showInLunar = (this.lunarFestival == "") ? this.showInLunar : this.lunarFestival;
    }
    var jeLunar = function(y,m,d) {
        return new flunar(new Date(y,m,d));
    };
    return jeLunar;
});