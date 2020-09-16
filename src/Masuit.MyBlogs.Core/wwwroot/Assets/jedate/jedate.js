/**
 @Name : jeDate v6.5.0 日期控件
 @Author: chen guojun
 @Date: 2018-04-30
 @QQ群：516754269
 @官网：http://www.jemui.com/ 或 https://github.com/singod/jeDate
 */
;(function(window, factory) {
    //amd
    if (typeof define === 'function' && define.amd) {
        define(factory);
    } else if (typeof exports === 'object') { //umd
        module.exports = factory();
    } else {
        window.jeDate = factory();
    }
})(this, function() {    
    var doc = document, win = window;
    var jet = {}, doc = document, regymdzz = "YYYY|MM|DD|hh|mm|ss|zz", gr = /\-/g,
        regymd = "YYYY|MM|DD|hh|mm|ss|zz".replace("|zz",""), parseInt = function (n) { return window.parseInt(n, 10);};
    var $Q = function (selector,content) {
        content = content || document;
        return selector.nodeType ? selector : content.querySelector(selector);
    };
    var jeDate = function(elem,options){
        var opts = typeof (options) === "function" ? options() : options;
        return new jeDatePick(elem,opts);
    };
    //日期控件版本
    jeDate.dateVer = "V6.5.0";
    //用一个或多个其他对象来扩展一个对象，返回被扩展的对象
    jeDate.extend = jet.extend = function () {
        var options, name, src, copy,deep = false, target = arguments[0], i = 1, length = arguments.length;
        if (typeof (target) === "boolean") deep = target, target = arguments[1] || {}, i = 2;
        if (typeof (target) !== "object" && typeof (target) !== "function") target = {};
        if (length === i) target = this, --i;
        for (; i < length; i++) {
            if ((options = arguments[i]) != null) {
                for (name in options) {
                    src = target[name], copy = options[name];
                    if (target === copy) continue;
                    if (copy !== undefined) target[name] = copy;
                }
            }
        }
        return target;
    };
    //返回指定日期
    jeDate.nowDate = function (val,format) {
        format = format || 'YYYY-MM-DD hh:mm:ss';
        if (!isNaN(val)) val = {DD: val};
        return jet.parse(jet.getDateTime(val),format);
    };
    //日期转换
    jeDate.convert = function (obj) {
        obj.format = obj.format || 'YYYY-MM-DD hh:mm:ss';
        obj.addval = obj.addval || [];
        var mats = jet.reMatch(obj.format),objVal = {};
        jet.each(jet.reMatch(obj.val),function (i,cval) {
            objVal[mats[i]] = parseInt(cval);
        });
        var result = new DateTime(obj.addval,objVal), redate = { 
            YYYY:result.GetYear(), MM:result.GetMonth(), DD:result.GetDate(),
            hh:result.GetHours(), mm:result.GetMinutes(), ss:result.GetSeconds()
        };
        return redate;
    };
    jeDate.valText = function (elem,value) {
        return jet.valText(elem,value);     
    }
    //日期时间戳相互转换
    jeDate.timeStampDate = function (date,format) {
        format = format || 'YYYY-MM-DD hh:mm:ss';
        var dateTest = (/^(-)?\d{1,10}$/.test(date) || /^(-)?\d{1,13}$/.test(date));
        if(/^[1-9]*[1-9][0-9]*$/.test(date) && dateTest){
            var vdate = parseInt(date);
            if (/^(-)?\d{1,10}$/.test(vdate)) {
                vdate = vdate * 1000;
            } else if (/^(-)?\d{1,13}$/.test(vdate)) {
                vdate = vdate * 1000;
            } else if (/^(-)?\d{1,14}$/.test(vdate)) {
                vdate = vdate * 100;
            } else {
                alert("时间戳格式不正确");
                return;
            }
            var setdate = new Date(vdate);
            return jet.parse({
                YYYY:setdate.getFullYear(), MM:jet.digit(setdate.getMonth()+1), DD:jet.digit(setdate.getDate()) , 
                hh:jet.digit(setdate.getHours()), mm:jet.digit(setdate.getMinutes()), ss:jet.digit(setdate.getSeconds()) 
            }, format);
        }else {
            //将日期转换成时间戳
            var arrs = jet.reMatch(date),
                newdate = new Date(arrs[0],arrs[1]-1,arrs[2],arrs[3]||0,arrs[4]||0,arrs[5]||0),
                timeStr = Math.round(newdate.getTime() / 1000);
            return timeStr;
        }
    };
    //获取年月日星期
    jeDate.getLunar = function(obj){
        //如果为数字类型的日期对获取到日期的进行替换
        var lunars = jeLunar(obj.YYYY, parseInt(obj.MM) - 1, obj.DD);
        return{
            nM: lunars.lnongMonth,              //农历月
            nD: lunars.lnongDate,               //农历日
            cY: parseInt(lunars.solarYear),     //阳历年
            cM: parseInt(lunars.solarMonth),    //阳历月
            cD: parseInt(lunars.solarDate),     //阳历日
            cW: lunars.inWeekDays,              //汉字星期几
            nW: lunars.solarWeekDay             //数字星期几
        };
    };
    //转换日期格式
    jeDate.parse = jet.parse = function(ymdhms, format) {
        return format.replace(new RegExp(regymdzz,"g"), function(str, index) {
            return str == "zz" ? "00":jet.digit(ymdhms[str]);
        });
    }
    //返回日期
    function DateTime(arr,valObj) {
        var that = this,newdate = new Date(), narr = ["FullYear","Month","Date","Hours","Minutes","Seconds"]; 
        var vb = jet.extend({YYYY:null,MM:null,DD:null,hh:newdate.getHours(),mm:newdate.getMinutes(),ss:newdate.getSeconds()},valObj);
        var ND = valObj == undefined ? newdate : new Date(vb.YYYY,vb.MM,vb.DD,vb.hh,vb.mm,vb.ss);
        if((arr||[]).length>0) jet.each(arr,function (i,par) { 
            ND["set"+narr[i]](narr[i] == "Month" ? parseInt(par)-1:parseInt(par)); 
        });
        //返回一个数值相同的新DateTime对象 
        that.reDate = function () {
            return new DateTime();
        };
        //返回此实例的Date值 
        that.GetValue = function () {
            return ND;
        };
        //获取此实例所表示日期的年份部分。 
        that.GetYear = function () {
            return ND.getFullYear();
        };
        //获取此实例所表示日期的月份部分。 
        that.GetMonth = function () {
            return ND.getMonth() + 1;
        };
        //获取此实例所表示的日期为该月中的第几天。 
        that.GetDate = function () {
            return ND.getDate();
        };
        //获取此实例所表示日期的小时部分。 
        that.GetHours = function () {
            return ND.getHours();
        };
        //获取此实例所表示日期的分钟部分。 
        that.GetMinutes = function () {
            return ND.getMinutes();
        };
        //获取此实例所表示日期的秒部分。 
        that.GetSeconds = function () {
            return ND.getSeconds();
        };
    };

    jet.extend(jet,{
        isType : function (obj,type) {
            var firstUper = function (str) {
                str = str.toLowerCase();
                return str.replace(/\b(\w)|\s(\w)/g, function (m) {
                    return m.toUpperCase();
                });
            }
            return Object.prototype.toString.call(obj) == "[object " + firstUper(type) + "]";
        },
        each : function (obj, callback, args) {
            var name, i = 0, length = obj.length, iselem = (length === undefined || obj === "function");
            if (iselem) {
                for (name in obj) { if (callback.call(obj[name], name, obj[name]) === false) { break } }
            } else {
                for (; i < length;) { if (callback.call(obj[i], i, obj[i++]) === false) { break } }
            }            
            return obj;
        },
        on : function (elm, type, fn) {
            if (elm.addEventListener) {
                elm.addEventListener(type, fn, false);//DOM2.0
                return true;
            }else if (elm.attachEvent) {
                return elm.attachEvent("on" + type, fn);//IE5+
            }else {
                elm["on" + type] = fn;//DOM 0
            }
        },
        isObj : function (obj){
            for(var i in obj){return true;}
            return false;
        },
        trim : function (str){ return str.replace(/(^\s*)|(\s*$)/g, ""); }, 
        reMatch : function (str) {
            var smarr = [],maStr = "", parti = /(^\w{4}|\w{2}\B)/g;
            if(jet.isNum(str)){
                maStr =  str.replace(parti,"$1-");
            }else{
                maStr = /^[A-Za-z]+$/.test(str) ? str.replace(parti,"$1-") : str;   
            }
            jet.each(maStr.match(/\w+|d+/g),function (i,val) {
                smarr.push(jet.isNum(val) ? parseInt(val):val);
            });
            return smarr;
        },
        equals : function (arrA,arrB) {
            if (!arrB) return false;
            if (arrA.length != arrB.length) return false;
            for (var i = 0, l = arrA.length; i < l; i++) {
                if (arrA[i] instanceof Array && arrB[i] instanceof Array) {
                    if (!arrA[i].equals(arrB[i])) return false;
                } else if (arrA[i] != arrB[i]) {
                    return false;
                }
            }
            return true;
        },
        docScroll : function(type) {
            type = type ? "scrollLeft" :"scrollTop";
            return document.body[type] | document.documentElement[type];
        },
        docArea : function(type) {
            return document.documentElement[type ? "clientWidth" :"clientHeight"];
        },
        //补齐数位
        digit : function(num) {
            return num < 10 ? "0" + (num | 0) :num;
        },
        //判断是否为数字
        isNum : function(value){
            return /^[+-]?\d*\.?\d*$/.test(value) ? true : false;
        },
        //获取本月的总天数
        getDaysNum : function(y, m) {
            var num = 31,isLeap = (y % 100 !== 0 && y % 4 === 0) || (y % 400 === 0);
            switch (parseInt(m)) {
                case 2: num = isLeap ? 29 : 28; break;
                case 4: case 6: case 9: case 11: num = 30; break;
            }
            return num;
        },
        //获取月与年
        getYM : function(y, m, n) {
            var nd = new Date(y, m - 1);
            nd.setMonth(m - 1 + n);
            return {
                y: nd.getFullYear(),
                m: nd.getMonth() + 1
            };
        },
        //获取上个月
        prevMonth : function(y, m, n) {
            return jet.getYM(y, m, 0 - (n || 1));
        },
        //获取下个月
        nextMonth : function(y, m, n) {
            return jet.getYM(y, m, n || 1);
        },
        setCss:function(elem,obj) {
            for (var x in obj) elem.style[x] = obj[x];
        },
        html : function (elem,html) {
            return typeof html === "undefined" ? elem && elem.nodeType === 1 ? elem.innerHTML :undefined :typeof html !== "undefined" && html == true ? elem && elem.nodeType === 1 ? elem.outerHTML :undefined : elem.innerHTML = html;
                
        },
        // 读取设置节点文本内容
        text : function(elem,value) {
            var innText = document.all ? "innerText" :"textContent";
            return typeof value === "undefined" ? elem && elem.nodeType === 1 ? elem[innText] :undefined : elem[innText] = value;
        },
        //设置值
        val : function (elem,value) {
            if (typeof value === "undefined") {
                return elem && elem.nodeType === 1 && typeof elem.value !== "undefined" ? elem.value :undefined;
            }
            // 将value转化为string
            value = value == null ? "" :value + "";
            elem.value = value;
        },
        attr : function(elem,value){
            return elem.getAttribute(value);
        },
        hasClass : function (obj, cls) {
            return obj.className.match(new RegExp('(\\s|^)' + cls + '(\\s|$)'));
        },
        stopPropagation : function (ev) { 
            (ev && ev.stopPropagation) ? ev.stopPropagation() : window.event.cancelBubble = true;  
        },
        template : function (str, data) {
            var strCell = !/[^\w\-\.:]/.test(str) ? document.getElementById(str).innerHTML : str;
            var keys = function (obj){
                var arr = [];
                for(arr[arr.length] in obj);
                return arr ;
            }, dataVar = function (obj) {
                var vars = ''; 
                for (var key in obj) {
                    vars += 'var ' + key + '= $D["' + key + '"];';
                }
                return vars;
            }, compile = function (source,data) {
                var code = "var $out='" + source.replace(/[\r\n]/g, '').replace(/^(.+?)\{\%|\%\}(.+?)\{\%|\%\}(.+?)$/g, function (val) {
                        return val.replace(/(['"])/g, '\\\$1');
                    }).replace(/\{\%\s*=\s*(.+?)\%\}/g, "';$out+=$1;$out+='").replace(/\{\%(.+?)\%\}/g, "';$1;$out+='") + "';return new String($out);";
                var vars = dataVar(data), Render = new Function('$D',vars + code);
                return new Render(data) + '';
            };
            return compile(strCell,data);
        },
        
        //判断元素类型
        isValDiv : function(elem) {
            return /textarea|input/.test(elem.tagName.toLocaleLowerCase());
        },
        valText : function (elem,value) {
            var cell = $Q(elem) ,type = jet.isValDiv(cell) ? "val" : "text";
            if(value != undefined){
                jet[type](cell,value);  
            }else{
                return jet[type](cell);
            }       
        },
        isBool : function(obj){  return (obj == undefined || obj == true ?  true : false); },
        //获取返回的日期
        getDateTime : function (obj) {
            var result = new DateTime(), objVal = jet.extend({YYYY:null,MM:null,DD:null,hh:0,mm:0,ss:0},obj),
                matArr = {YYYY:"FullYear",MM:"Month",DD:"Date",hh:"Hours",mm:"Minutes",ss:"Seconds"};
            jet.each(["ss","mm","hh","DD","MM","YYYY"],function (i,mat) {
                if (!jet.isNum(parseInt(objVal[mat]))) return null;
                var reVal = result.GetValue();
                if (parseInt(objVal[mat]) || parseInt(objVal[mat]) == 0){
                    reVal["set"+matArr[mat]](result["Get"+matArr[mat]]() + (mat == "MM" ? -1 : 0) + parseInt(objVal[mat]));
                }
            });
            //获取格式化后的日期
            var redate = { 
                YYYY:result.GetYear(), MM:result.GetMonth(), DD:result.GetDate(),
                hh:result.GetHours(), mm:result.GetMinutes(), ss:result.GetSeconds()
            };
            return redate;
        }
    });

    function jeDatePick(elem, options){
        var config = {
            language:{
                name   : "cn",
                month  : ["01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"],
                weeks  : [ "日", "一", "二", "三", "四", "五", "六" ],
                times  : ["小时","分钟","秒数"],
                timetxt: ["时间选择","开始时间","结束时间"],
                backtxt:"返回日期",
                clear  : "清空",
                today  : "现在",
                yes    : "确定"
            },
            format:"YYYY-MM-DD hh:mm:ss",               //日期格式
            minDate:"1900-01-01 00:00:00",              //最小日期
            maxDate:"2099-12-31 23:59:59",              //最大日期
            isShow:true,                                //是否显示为固定日历，为false的时候固定显示
            multiPane:true,                             //是否为双面板，为false是展示双面板
            onClose:true,                               //是否为选中日期后关闭弹层，为false时选中日期后关闭弹层
            range:false,                                //如果不为空且不为false，则会进行区域选择，例如 " 至 "，" ~ "，" To "
            trigger:"click",                            //是否为内部触发事件，默认为内部触发事件
            position:[],                                //自定义日期弹层的偏移位置，长度为0，弹层自动查找位置
            valiDate:[],                                //有效日期与非有效日期，例如 ["0[4-7]$,1[1-5]$,2[58]$",true]
            isinitVal:false,                            //是否初始化时间，默认不初始化时间
            initDate:{},                                //初始化时间，加减 天 时 分
            isTime:true,                                //是否开启时间选择
            isClear:true,                               //是否显示清空按钮
            isToday:true,                               //是否显示今天或本月按钮
            isYes:true,                                 //是否显示确定按钮
            festival:false,                             //是否显示农历节日
            fixed:true,                                 //是否静止定位，为true时定位在输入框，为false时居中定位
            zIndex:2099,                                //弹出层的层级高度
            method:{},                                 //自定义方法                
            theme:{},                                   //自定义主题色
            shortcut:[],                                //日期选择的快捷方式
            donefun:null,                                //选中日期完成的回调
            before:null,                                //在界面加载之前执行
            succeed:null                                //在界面加载之后执行  
        };
        this.$opts = jet.extend(config,options||{});
        this.valCell = $Q(elem); 
        this.format = this.$opts.format;
        this.valCell != null ? this.init() : alert(elem+"  ID\u6216\u7C7B\u540D\u4E0D\u5B58\u5728!");
        jet.extend(this,this.$opts.method);
        delete this.$opts.method;
    }
    var searandom = function (){
        var str = "",arr = [1,2,3,4,5,6,7,8,9,0];
        for(var i=0; i<8; i++) str += arr[Math.round(Math.random() * (arr.length-1))];
        return str;
    };

    var jefix = "jefixed",ymdzArr = jet.reMatch(regymdzz),elx = "#jedate";
    jet.extend(jeDatePick.prototype,{
        init : function () {  
            var that = this, opts = that.$opts, newDate = new Date(), shortArr = [],
                trigges = opts.trigger,ndate = opts.initDate || [], inVal, range = opts.range,
                zIndex = opts.zIndex == undefined ? 10000 : opts.zIndex,isShow = jet.isBool(opts.isShow),
                isinitVal = (opts.isinitVal == undefined || opts.isinitVal == false) ? false : true;
            that.setDatas();
            opts.before && opts.before(that.valCell);
            //为开启初始化的时间设置值
            if (isinitVal && trigges && isShow) {   
                if (ndate[1]){
                    var addval = jet.getDateTime(ndate[0]);
                    inVal = [{
                        YYYY:addval.YYYY, MM:jet.digit(addval.MM), DD:jet.digit(addval.DD) , 
                        hh:jet.digit(addval.hh), mm:jet.digit(addval.mm), ss:jet.digit(addval.ss) 
                    }];
                }else {
                    inVal = that.getValue(jet.isObj(ndate[0]) ? ndate[0] : {});
                }
                if(!range) that.setValue([inVal[0]],opts.format,true);
            }

            var getCurrValue = function () {
                var mats = jet.reMatch(that.format), isEmpty = that.getValue() != "",curVal = [],
                    parmat = that.dlen == 7 ? "hh:mm:ss" : "YYYY-MM"+ (that.dlen <= 2 ? "":"-DD");
                that.selectValue = [jet.parse(jet.getDateTime({}), parmat)];
                if(isEmpty && isShow){
                    var getVal = that.getValue().split(range);
                    jet.each(new Array(range ? 2 : 1),function (a) {
                        curVal[a] = {};
                        jet.each(jet.reMatch(getVal[a]),function (i,val) {
                            curVal[a][mats[i]] = parseInt(val);
                        });
                    });
                    if(range) that.selectValue = getVal;
                }else{
                    var parr = that.getValue({})[0], nmVal = jet.nextMonth(parr.YYYY,parr.MM||jet.getDateTime({}).MM),
                        narr = (that.dlen>2 && that.dlen <=6) ? {YYYY:nmVal.y,MM:nmVal.m} : {};
                    curVal = [parr];
                }
                that.selectDate = curVal;
                return curVal;
            },ymarr = [];
            that.minDate = ""; that.maxDate = "";
            if(!isShow || !trigges) ymarr = getCurrValue();
            if(!isShow || !trigges){  
                that.minDate = jet.isType(opts.minDate,"function") ? opts.minDate(that) : opts.minDate;
                that.maxDate = jet.isType(opts.maxDate,"function") ? opts.maxDate(that) : opts.maxDate;
                that.storeData(ymarr[0],ymarr[1]);
                that.renderDate();
                opts.succeed && opts.succeed(that.dateCell);
            }else{
                if (trigges) {
                    jet.on(that.valCell,trigges,function(){
                        if (document.querySelectorAll(elx).length > 0) return;
                        var gvarr = getCurrValue();
                        that.minDate = jet.isType(opts.minDate,"function") ? opts.minDate(that) : opts.minDate;
                        that.maxDate = jet.isType(opts.maxDate,"function") ? opts.maxDate(that) : opts.maxDate;
                        that.storeData(gvarr[0],gvarr[1]);
                        that.renderDate();
                    });
                }
            }        
        },
        setDatas : function(){
            var that = this, opts = that.$opts,range = opts.range,shortArr = [],isShow = jet.isBool(opts.isShow),multi = opts.multiPane;
            that.$data = jet.extend({year:false,month:false,day:true,time:false,timebtn:false},{
                shortcut:[],lang:opts.language,yaerlist:[],monthlist:[[],[]],ymlist:[[],[]], daylist:[[],[]],
                clear:opts.isClear,today:range ? false:opts.isToday,yes:opts.isYes,pane:multi ? 1:2
            });
            if(opts.shortcut.length>0){
                jet.each(opts.shortcut,function (i,short) {  
                    var tarr = [], shval = jet.isType(short.val,"function") ? short.val() : short.val;
                    if(jet.isType(shval,"object")){ 
                        for (var s in shval) tarr.push(s + ':' + shval[s]);
                        shortArr.push(jet.extend({},{name:short.name,val:"{" + tarr.join('#') + "}"}));
                    } 
                });
                that.$data.shortcut = shortArr;
            }
            that.dlen = (function () {
                var mats = jet.reMatch(that.format),marr = [];
                jet.each(ymdzArr,function(i,val){
                    jet.each(mats,function(m,mval){
                        if(val == mval) marr.push(mval);
                    });
                });
                var matlen = marr.length, lens = (marr[0] == "hh")&&matlen<=3 ? 7 : matlen;
                return lens;
            })();
            that.$data.dlen = that.dlen;
            that.timeInspect = false;
            if(that.dlen == 1){
                jet.extend(that.$data,{year:true,day:false});
            }else if(that.dlen == 2){
                jet.extend(that.$data,{month:true,day:false});
            }else if(that.dlen>3 && that.dlen <=6){
                that.$data.timebtn = true;
            }else if(that.dlen == 7){
                jet.extend(that.$data,{day:false,time:true});
            }
            if(!isShow){
                that.$data.clear = false;
                that.$data.yes = false;
            }
        },
        renderDate : function () {
            var that= this, opts = that.$opts,isShow = jet.isBool(opts.isShow),
                elxID = !isShow ? elx+searandom() : elx, setzin = {"zIndex":  (opts.zIndex == undefined ? 10000 : opts.zIndex)};  
            if(that.dateCell == undefined){   
                that.dateCell = document.createElement("div");  
                that.dateCell.id = elxID.replace(/\#/g,"");
                that.dateCell.className = elx.replace(/\#/g,"")+" "+(opts.shortcut.length>0?" leftmenu":"");
                that.dateCell.setAttribute("author","chen guojun"); 
            }
            jet.html(that.dateCell,jet.template(that.dateTemplate(),that.$data));
            //自定义主题色
            if(jet.isObj(opts.theme)){   
                var styleDiv = document.createElement("style"),stCell = ".jedate"+searandom(), t = opts.theme, 
                    BG = "background-color:"+t.bgcolor, WC = "color:"+(t.color == undefined ? "#FFFFFF":t.color),
                    OTH = (t.pnColor == undefined ? "":"color:"+t.pnColor+";");
                that.dateCell.className = that.dateCell.className+" "+stCell.replace(/^./g,""); styleDiv.setAttribute("type","text/css");
                styleDiv.innerHTML = stCell+" .jedate-menu p:hover{"+BG+";"+WC+";}"+stCell+" .jedate-header em{"+WC+";}"+
                stCell+" .jedate-content .yeartable td.action span,"+stCell+" .jedate-content .monthtable td.action span,"+
                stCell+" .jedate-content .yeartable td.action span:hover,"+stCell+" .jedate-content .monthtable td.action span:hover{"+BG+";border:1px "+t.bgcolor+" solid;"+WC+";}"+stCell+" .jedate-content .daystable td.action,"+stCell+" .jedate-content .daystable td.action:hover,"+
                stCell+" .jedate-content .daystable td.action .lunar,"+stCell+" .jedate-header,"+stCell+" .jedate-time .timeheader,"+
                stCell+" .jedate-time .hmslist ul li.action,"+stCell+" .jedate-time .hmslist ul li.action:hover,"+
                stCell+" .jedate-time .hmslist ul li.disabled.action,"+stCell+" .jedate-footbtn .timecon,"+stCell+" .jedate-footbtn .btnscon span{"+BG+";"+WC+";}"+
                stCell+" .jedate-content .daystable td.other,"+stCell+" .jedate-content .daystable td.other .nolunar,"+stCell+" .jedate-content .daystable td.other .lunar{"+OTH+"}"+stCell+" .jedate-content .daystable td.contain,"+stCell+" .jedate-content .daystable td.contain:hover{background-"+OTH+"}";
                that.dateCell.appendChild(styleDiv);
            }
            
            that.compileBindNode(that.dateCell); 
            if (document.querySelectorAll(elxID).length > 0) document.body.removeChild($Q(elxID));
            !isShow ? that.valCell.appendChild(that.dateCell) : document.body.appendChild(that.dateCell);
            jet.setCss(that.dateCell,jet.extend({position:(!isShow ? "relative" : (opts.fixed == true ? "absolute" :"fixed"))},isShow ? setzin:{}));
            that.methodEventBind();
            if(that.dlen == 7 || (that.dlen>3 && that.dlen <=6)) that.locateScroll();
            if(opts.festival && opts.language.name == "cn") that.showFestival();
            if(isShow){ 
                that.dateOrien(that.dateCell,that.valCell);  
                that.blankArea(); 
            }
            
        },
        //设置日期值
        setValue : function (fnStr,matStr,bool) {
            var that = this, valCell = that.valCell,strVal;
            matStr = matStr || that.format;
            if((typeof fnStr=='string')&&fnStr!=''){
                var sprange = fnStr.split(that.$opts.range), inArr=[];
                jet.each(sprange,function (i,sval) {
                    var reVal = jet.reMatch(sval), inObj={};
                    jet.each(jet.reMatch(matStr),function (r,val) {
                        inObj[val] = reVal[r];
                    });
                    inArr.push(inObj);
                });
                strVal = inArr;
            }else {
                strVal = fnStr;
            }
            var vals = that.parseValue(strVal,matStr);
            if (bool != false) jet.valText(valCell,vals);  
            return vals;
        },
        //获取日期值
        getValue : function (valobj) {
            var that = this, valCell = that.valCell,
                opts = that.$opts, reObj, result = new DateTime().reDate(),
                dateY = result.GetYear(),dateM = result.GetMonth(),dateD = result.GetDate(),
                timeh = result.GetHours(),timem = result.GetMinutes(),times = result.GetSeconds();
            if (valobj == undefined && jet.isBool(opts.isShow)){         
                reObj = jet.valText(valCell); 
            }else {
                var isValShow = jet.isBool(opts.isShow) ? (jet.valText(valCell) == "") : !jet.isBool(opts.isShow),
                    objarr = jet.extend({YYYY:null,MM:null,DD:null},valobj||{}),
                    ranMat = [],newArr = new Array(2),unObj = function (obj) {
                        return [(objarr[obj] == undefined || objarr[obj] == null),objarr[obj]];
                    }, defObj = [{ YYYY:dateY,MM:dateM,DD:dateD, hh:timeh,mm:timem,ss:times,zz:00},
                        { YYYY:dateY,MM:dateM,DD:dateD, hh:timeh,mm:timem,ss:times,zz:00}];
                if (isValShow) {
                    //目标为空值则获取当前日期时间
                    jet.each(newArr,function (i) {
                        var inObj = {};
                        jet.each(ymdzArr, function (r, val) {
                            inObj[val] = parseInt(unObj(val)[0] ? defObj[i][val] : unObj(val)[1]);
                        });
                        ranMat.push(jet.extend(defObj[i], inObj));
                    });
                } else {
                    var isunRange = opts.range != false, initVal = that.getValue(),
                        spVal = initVal.split(opts.range), reMat = jet.reMatch(that.format);
                    jet.each(newArr,function (i) {
                        var inObj = {}, reVal = isunRange ? jet.reMatch(spVal[i]) : jet.reMatch(initVal);
                        jet.each(reMat,function (r,val) {
                            inObj[val] = reVal[r];
                        });
                        var exVal = jet.extend(inObj,valobj||{});
                        ranMat.push(jet.extend(defObj[i],exVal));
                    });
                }
                reObj = ranMat;
            }
            return reObj;
        },
        storeData:function (curr,next) {
            next = next || {};
            var that = this, opts = that.$opts,multi = opts.multiPane,valCell = that.valCell,
                days = new Date().getDate(), DTS = that.$data,isnext = jet.isObj(next),
                RES = {yearlist:[],monthlist:[[],[]],daylist:[],daytit:[],timelist:[]},seltime,         
                cday = curr.DD == null ? days : curr.DD, nday = next.DD == null ? days : next.DD,
                timeA = {hh:curr.hh,mm:curr.mm,ss:curr.ss}, timeB = {hh:next.hh||0,mm:next.mm||0,ss:next.ss||0};  
            //设置年的数据
            RES.yearlist.push(that.eachYear(parseInt(curr.YYYY),1));
            if(multi == false){
                var yearNext = isnext ? next.YYYY : curr.YYYY;
                RES.yearlist.push(that.eachYear(parseInt(yearNext),2));
            } 
            //设置月的数据
            RES.monthlist[0] = that.eachMonth(curr.YYYY,0);
            if(multi == false){
                var monthNext = isnext ? next.YYYY : curr.YYYY+1;
                RES.monthlist[1] = that.eachMonth(curr.YYYY+1,1);
            } 
            //设置天的数据
            RES.daylist.push(that.eachDays(curr.YYYY,curr.MM,cday,0));
            RES.daytit.push({YYYY:curr.YYYY,MM:curr.MM});
            if(multi == false){
                var dayNext = jet.nextMonth(curr.YYYY,curr.MM);
                RES.daylist.push(that.eachDays(dayNext.y,dayNext.m,nday,1));
                RES.daytit.push({YYYY:dayNext.y,MM:dayNext.m});
            } 
            //设置时间数据
            that.selectTime = [timeA,timeB];  
            RES.timelist.push(that.eachTime(timeA,1));
            if(multi == false){
                seltime = that.dlen == 7 && opts.range && !isnext ? timeA : timeB;
                if(that.dlen == 7 && opts.range && jet.valText(valCell) == ""){
                    that.selectTime[1] = jet.extend(timeB,timeA);
                } 
                RES.timelist.push(that.eachTime(seltime,2)); 
            } 
            //最后将数据合并于总数据中
            jet.extend(that.$data,RES);
        },
        dateTemplate : function() {
            var that = this, opts = that.$opts, multi = opts.multiPane,YMDStr = "",hmsStr = "",lang = opts.language,
                ytxt = lang.name == "cn" ? "年":"", mtxt = lang.name == "cn" ? "月":"";
            var ymvals = multi ? '{%=ymlist[0].YYYY%}-{%=ymlist[0].MM%}':'{%=ymlist[0].YYYY%}-{%=ymlist[0].MM%}#{%=ymlist[ynidx].YYYY%}-{%=ymlist[ynidx].MM%}';
            var aowArr = (function () {
                var butArr = [], ismu = multi ? "11":"23";
                if(that.dlen == 1){
                    butArr = ['{%=yearlist[i][0].y-'+ismu+'%}','{%=yearlist[i][yearlist[i].length-1].y%}'];
                }else if(that.dlen == 2){
                    butArr = multi ? ['{%=yearlist[0][0].y-1%}','{%=yearlist[0][0].y+1%}']:['{%=yearlist[i][0].y-'+ismu+'%}','{%=yearlist[i][yearlist[i].length-1].y%}'];
                }else if(that.dlen>2 && that.dlen <=6){
                    butArr = ['{%=yearlist[0][0].y-1%}','{%=yearlist[0][0].y+1%}'];
                }
                return butArr;
            })();
            var lyPrev = '<em class="yearprev yprev jedatefont" @on="yearBtn(lprev,'+aowArr[0]+')">&#xed6c2;</em>',
                lyNext = '<em class="yearnext ynext jedatefont" on="yearBtn(lnext,'+aowArr[2]+')">&#xed6c5;</em>',
                ryPrev = '<em class="yearprev yprev jedatefont" on="yearBtn(rprev,'+aowArr[3]+')">&#xed6c2;</em>',
                ryNext = '<em class="yearnext ynext jedatefont" @on="yearBtn(rnext,'+aowArr[1]+')">&#xed6c5;</em>',
                mPrev = '{% if(dlen>2){ %}<em class="monthprev mprev jedatefont" @on="monthBtn(mprev,{%=daytit[i].YYYY%}-{%=daytit[i].MM%})">&#xed602;</em>{% } %}',
                mNext = '{% if(dlen>2){ %}<em class="monthnext mnext jedatefont" @on="monthBtn(mnext,{%=daytit[i].YYYY%}-{%=daytit[i].MM%})">&#xed605;</em>{% } %}';
            //循环年的模板
            var yaerHtml = '<table class="yeartable year{%= i==0 ? "left":"right"%}" style="display:{%=year ? "block":"none"%};"><tbody><tr>'+
            '{% for(var y=0;y<=11;y++){ %}<td class="{%=yearlist[i][y].style%}" @on="yearClick({%=yearlist[i][y].y%})"><span>{%=yearlist[i][y].y%}'+ytxt+'</span></td>{% if((y+1)%3==0){ %} </tr>{% } %} {% } %} </tbody></table>';
            //循环月的模板
            var monthHtml = '<table class="monthtable month{%= i==0 ? "left":"right"%}" style="display:{%=month ? "block":"none"%};"><tbody><tr>'+
            '{% for(var m=0;m<=11;m++){ %}<td class="{%=monthlist[i][m].style%}" ym="{%=monthlist[i][m].y%}-{%=monthlist[i][m].m%}" @on="monthClick({%=monthlist[i][m].y%}-{%=monthlist[i][m].m%})"><span>{%=monthlist[i][m].m%}'+mtxt+'</span></td>{% if((m+1)%3==0){ %} </tr>{% } %} {% } %} </tbody></table>';
            //循环天的模板
            var daysHtml = '<table class="daystable days{%= i==0 ? "left":"right"%}" style="display:{%=day ? "block":"none"%};"><thead><tr>'+
            '{% for(var w=0;w<lang.weeks.length;w++){ %} <th>{%=lang.weeks[w]%}</th> {% } %}</tr></thead><tbody>'+
            '<tr>{% for(var d=0;d<=41;d++){ %}<td class="{%=daylist[i][d].style%}" ymd="{%=daylist[i][d].ymd%}" @on="daysClick({%=daylist[i][d].ymd%})">{%=daylist[i][d].day%}</td>{% if((d+1)%7==0){ %} </tr>{% } %} {% } %} </tbody></table>';
            //循环时间模板
            var hmsHtml = '<div class="jedate-time">{% for(var h=0;h<timelist.length;h++){ %}<div class="timepane"><div class="timeheader">{%= timelist.length == 1 ? lang.timetxt[0]:lang.timetxt[h+1]%}</div><div class="timecontent">'+
            '<div class="hmstitle"><p>{%=lang.times[0]%}</p><p>{%=lang.times[1]%}</p><p>{%=lang.times[2]%}</p></div>'+
            '<div class="hmslist">{% for(var t=0;t<3;t++){ %}<div class="hmsauto"><ul>{% for(var s=0;s<timelist[h][t].length;s++){ %}<li class="{%=timelist[h][t][s].style%}" @on="hmsClick({%= h %},{%= h>0?3+t:t %})">{%= timelist[h][t][s].hms < 10 ? "0" + timelist[h][t][s].hms :timelist[h][t][s].hms %}</li>{% } %}</ul></div>{% } %}</div></div>'+'</div>{% } %}</div>'; 
            //左边选择模板
            var shortHtml = opts.shortcut.length > 0 ? "{% for(var s=0;s<shortcut.length;s++){ %}<p @on=shortClick({%= shortcut[s].val %})>{%=shortcut[s].name%}</p>{% } %}":'';
            
            var ymtitHtml = (function () {
                var ymtitStr = "";
                if(that.dlen == 1){
                    ymtitStr = '<span class="ymbtn">{%=yearlist[i][0].y%}'+ytxt+' ~ {%=yearlist[i][yearlist[i].length-1].y%}'+ytxt+'</span>';
                }else if(that.dlen == 2){
                    ymtitStr = '<span class="ymbtn" @on="yearShow({%=yearlist[0][i].y%})">{%=yearlist[0][i].y%}'+ytxt+'</span>';
                }else if(that.dlen>2 && that.dlen <=6){
                    ymtitStr = '<span class="ymbtn" @on="monthShow({%=daytit[i].MM%})">{%=daytit[i].MM%}'+mtxt+'</span>'+
                    '<span class="ymbtn" @on="yearShow({%=daytit[i].YYYY%})">{%=daytit[i].YYYY%}'+ytxt+'</span>';
                }
                return ymtitStr;
            })();

            var ymButton = (function () {
                var titStrBut = "";
                if(that.dlen==1){
                    titStrBut = multi ? [lyPrev+ryNext]:[lyPrev,ryNext];
                }else if(that.dlen==2){  
                    titStrBut = multi ? [lyPrev+ryNext] : [lyPrev,ryNext];  
                }else if(that.dlen>2 && that.dlen <=6){
                    titStrBut = multi ? [lyPrev+mPrev+mNext+ryNext] : [lyPrev+mPrev,mNext+ryNext];
                }else if(that.dlen==7){
                    titStrBut = "";
                }
                return titStrBut;
            })();
            
            if(that.dlen == 1){
                YMDStr = yaerHtml;
            }else if(that.dlen == 2){
                YMDStr = yaerHtml + monthHtml;
            }else if(that.dlen == 3){
                YMDStr = yaerHtml + monthHtml + daysHtml;
            }else if(that.dlen > 3 && that.dlen <= 6){
                YMDStr = yaerHtml + monthHtml + daysHtml;
                hmsStr = hmsHtml;
            }else if(that.dlen == 7){
                hmsStr = hmsHtml;
            }
            var paneHtml = '{% for(var i=0;i<pane;i++){ %}<div class="jedate-pane">'+
                '<div class="jedate-header">{% if(i==0){ %}'+ymButton[0]+'{% }else{ %}'+ymButton[1]+'{% } %}'+ymtitHtml+'</div>'+
                '<div class="jedate-content{%= i==1?" bordge":"" %}">'+YMDStr+'</div>'+
                '</div>{% } %}';   
            var btnStr = '{% if(timebtn){%}<div class="timecon" style="cursor: pointer;" @on="timeBtn">{%=lang.timetxt[0]%}</div>{% } %}<div class="btnscon">{% if(clear){ %}<span class="clear" @on="clearBtn">{%=lang.clear%}</span>{% } %}{% if(today){ %}<span class="today" @on="nowBtn">{%=lang.today%}</span>{% } %}{% if(yes){ %}<span class="setok" @on="sureBtn">{%=lang.yes%}</span>{% } %}</div>';

            return '<div class="jedate-menu" style="display:{%=shortcut.length>0 ? "block":"none"%};">'+shortHtml+'</div><div class="jedate-wrap">'+paneHtml+'</div>'+hmsStr+'<div class="jedate-footbtn">'+btnStr+'</div><div class="jedate-tips"></div>';        
        },
        //递归绑定事件
        compileBindNode : function (dom) {
            var self = this, aton = "@on";
            var acquireAttr = function (atVal){
                var args=/\(.*\)/.exec(atVal);
                if(args) { //如果函数带参数,将参数字符串转换为参数数组
                    args = args[0];
                    atVal = atVal.replace(args,"");
                    args = args.replace(/[\(\)\'\"]/g,'').split(",");
                }else args = [];
                return [atVal,args];
            }; 
            jet.each(dom.childNodes,function (i,node) {
                if (node.nodeType === 1) {
                    if(!self.$opts.festival) node.removeAttribute("ymd");
                    self.compileBindNode(node);
                    var geton = node.getAttribute(aton);
                    if(geton != null){
                        var onarr = acquireAttr(geton); 
                        jet.on(node,"click",function () {
                            self[onarr[0]] && self[onarr[0]].apply(node,onarr[1]);
                        });
                        node.removeAttribute(aton);
                    }    
                }
            });  
        },
        methodEventBind : function() {
            var that = this, opts = that.$opts, multi = opts.multiPane, DTS = that.$data,
                result = new DateTime().reDate(),dateY = result.GetYear(),dateM = result.GetMonth(),dateD = result.GetDate(),
                range = opts.range,elCell = that.dateCell;
            jet.extend(that,{
                yearBtn:function (type,val) {
                    var yarr = val.split("#"), pval = jet.reMatch(yarr[0]), tmval = that.selectTime;
                    exarr = [jet.extend({YYYY:parseInt(val),MM:dateM,DD:dateD},tmval[0]),{}];
                    var dateVal = that.parseValue([exarr[0]],that.format);
                    that.storeData(exarr[0],exarr[1]);
                    that.renderDate();
                    opts.toggle && opts.toggle({elem:that.valCell,val:dateVal,date:exarr[0]});
                },
                yearShow:function (val) {
                    DTS.year = DTS.year ? false : true;
                    DTS.month = that.dlen < 3 ? true : false;   
                    if(that.dlen > 2 && that.dlen <= 6){
                        var dayCell = $Q(".daystable",elCell);
                        DTS.day = dayCell.style.display == "none" ? true : false;
                    } 
                    that.renderDate();
                },
                monthBtn:function (type,val) { 
                    var ymarr = jet.reMatch(val),tmval = that.selectTime, exarr=[], PrevYM , NextYM,   
                        year = parseInt(ymarr[0]),month = parseInt(ymarr[1]); 
                    if(range){
                        if(type == "mprev"){
                            PrevYM = jet.prevMonth(year, month);
                            NextYM = jet.nextMonth(PrevYM.y, PrevYM.m);
                        }else{
                            NextYM = jet.nextMonth(year, month);
                            PrevYM = jet.prevMonth(NextYM.y, NextYM.m);
                        }
                        exarr = [jet.extend({YYYY:PrevYM.y,MM:PrevYM.m,DD:dateD},tmval[0]),{YYYY:NextYM.y,MM:NextYM.m,DD:dateD}];
                    }else{
                        var PNYM = (type == "mprev") ? jet.prevMonth(year, month) : jet.nextMonth(year, month);
                        exarr = [jet.extend({YYYY:PNYM.y,MM:PNYM.m,DD:dateD},tmval[0]),{}];
                    }
                    var dateVal = that.parseValue([exarr[0]],that.format);
                    that.storeData(exarr[0],exarr[1]);
                    that.renderDate();
                    opts.toggle && opts.toggle({elem:that.valCell,val:dateVal,date:exarr[0]});
                },
                monthShow:function (val) {
                    DTS.year = false;
                    DTS.month = DTS.month ? false : true; 
                    if(that.dlen > 2 && that.dlen <= 6){
                        var dayCell = $Q(".daystable",elCell);
                        DTS.day = dayCell.style.display == "none" ? true : false;
                    }   
                    that.renderDate();
                },
                shortClick:function (val) {  
                    var reval = val.replace(/\#/g,','),evobj = eval("("+reval+")"), 
                        gval = jet.getDateTime(evobj),tmval = that.selectTime;
                    that.selectValue = [jet.parse(gval,"YYYY-MM-DD")];
                    that.selectDate = [{YYYY:gval.YYYY,MM:gval.MM,DD:gval.DD}];
                    if(opts.onClose){
                        var nYM = jet.nextMonth(gval.YYYY,gval.MM),
                            ymarr = [{YYYY:gval.YYYY,MM:gval.MM,DD:gval.DD},{YYYY:nYM.y,MM:nYM.m,DD:null}];
                        that.storeData(jet.extend(ymarr[0],tmval[0]),jet.extend(ymarr[1],tmval[1]));
                        that.renderDate();
                    }else{
                        that.setValue(gval,that.format);
                        that.closeDate(); 
                    }
                },
                yearClick:function (val) {
                    if(jet.hasClass(this,"disabled")) return;
                    var yearVal = "",lens = that.dlen;
                    if(range && lens == 1){
                        var ylen = that.selectValue.length;
                        that.selectDate = (ylen == 2) ? [{YYYY:parseInt(val),MM:dateM}] : 
                            [{YYYY:that.selectDate[0].YYYY,MM:that.selectDate[0].MM},{YYYY:parseInt(val),MM:dateM}];
                        that.selectValue = (ylen == 2) ? [val+"-"+jet.digit(dateM)] : [that.selectValue[0],val+"-"+jet.digit(dateM)];
                        
                        if(that.selectValue.length == 2){
                            var svalarr = [that.selectValue[0],that.selectValue[1]],newArr = [{},{}];
                            svalarr.sort(function(a, b){ return a > b ? 1 : -1; });
                            that.selectValue = svalarr;
                            jet.each(svalarr,function(i,strval) {
                                jet.each(jet.reMatch(strval),function(s,dval) {
                                    newArr[i][ymdzArr[s]] = dval;
                                });
                            });
                            that.selectDate = newArr;
                        } 
                    }else if(lens>1 && lens <=6){   
                        yearVal = parseInt(val);
                    }else{
                        that.selectValue = [val+"-"+jet.digit(dateM)];
                        that.selectDate = [{YYYY:parseInt(val),MM:dateM}];
                    }
                    DTS.year = (lens == 1) ? true : false;
                    DTS.month = (lens < 3) ? true : false;
                    DTS.day = (lens > 2 && lens <= 6) ? true : false;
                    var electVal = (lens>1 && lens <=6) ? yearVal : parseInt(that.selectDate[0].YYYY);
                    that.storeData(jet.extend({YYYY:electVal,MM:dateM,DD:dateD},that.selectTime[0]),{});
                    that.renderDate();    
                },
                monthClick:function (val) {
                    if(jet.hasClass(this,"disabled")) return;
                    var ymval = jet.reMatch(val),newArr = [{},{}],mlen = that.selectValue.length ;
                    if(range){
                        that.selectDate = (mlen == 2) ? [{YYYY:ymval[0],MM:ymval[1]}] : 
                            [{YYYY:that.selectDate[0].YYYY,MM:that.selectDate[0].MM},{YYYY:parseInt(val),MM:ymval[1]}];
                        that.selectValue = (mlen == 2) ? [val] : [that.selectValue[0],val];    

                        if(that.selectValue.length == 2){
                            var svalarr = [that.selectValue[0],that.selectValue[1]];
                            svalarr.sort(function(a, b){ return a > b ? 1 : -1; });
                            that.selectValue = svalarr;
                            jet.each(svalarr,function(i,strval) {
                                jet.each(jet.reMatch(strval),function(s,dval) {
                                    newArr[i][ymdzArr[s]] = dval;
                                });
                            });
                            that.selectDate = newArr;
                        }
                    }else{
                        that.selectValue = [val];
                        that.selectDate = [{YYYY:ymval[0],MM:ymval[1]}];
                    }
                    if(that.dlen > 2){
                        DTS.year = false;
                        DTS.month = false;
                    }
                    DTS.day = (that.dlen > 2 && that.dlen <= 6) ? true : false;
                    that.storeData(jet.extend({
                        YYYY:parseInt(that.selectDate[0].YYYY),
                        MM:parseInt(that.selectDate[0].MM),
                        DD:dateD
                    },that.selectTime[0]),{});
                    that.renderDate();   
                },
                daysClick:function (val) {
                    if(jet.hasClass(this,"disabled")) return;
                    var tmval = that.selectTime, matVal = jet.reMatch(val),
                        slen = that.selectValue.length,dateVal = "",
                        newArr = [{},{}],sday,nYM,ymarr;
                    if(range){
                        
                        if(slen == 1){
                            var svalarr = [that.selectValue[0],val];
                            svalarr.sort(function(a, b){ return a > b ? 1 : -1; });
                            that.selectValue = svalarr;
                            jet.each(svalarr,function(i,strval) {
                                jet.each(jet.reMatch(strval),function(s,dval) {
                                    newArr[i][ymdzArr[s]] = dval;
                                });
                            });
                            that.selectDate = newArr;
                        }else{
                            that.selectValue = [val];  
                            newArr = [{YYYY:matVal[0],MM:matVal[1],DD:matVal[2]}];
                            that.selectDate = [{YYYY:matVal[0],MM:matVal[1],DD:matVal[2]},{}];
                        }
                        nYM = jet.nextMonth(newArr[0].YYYY,newArr[0].MM);
                        ymarr = [{YYYY:newArr[0].YYYY,MM:newArr[0].MM,DD:newArr[0].DD},{YYYY:nYM.y,MM:nYM.m,DD:null}];
                        that.storeData(jet.extend(ymarr[0],tmval[0]),jet.extend(ymarr[1],tmval[1]));
                        that.renderDate();
                    }else{
                        that.selectValue = [val];
                        that.selectDate = [{YYYY:matVal[0],MM:matVal[1],DD:matVal[2]},{YYYY:matVal[0],MM:matVal[1],DD:matVal[2]}];
                        jet.each(new Array(range == false ? 1 : 2),function (a) {
                            jet.each(matVal,function (i,val) {
                                newArr[a][ymdzArr[i]] = val;
                            });
                            jet.extend(newArr[a],tmval[a]);
                        });
                        if(opts.onClose){  
                            that.storeData(jet.extend(newArr[0],tmval[0]),jet.extend(newArr[1],tmval[1]));
                            that.renderDate();
                        }else{
                            dateVal = that.setValue(newArr,that.format);
                            that.closeDate();
                            opts.donefun && opts.donefun.call(that,{elem:that.valCell,val:dateVal,date:newArr}); 
                        }
                    }    
                },
                hmsClick:function(idx,num) {
                    var pidx = parseInt(num), vals = parseInt(jet.text(this)), 
                        paridx = parseInt(idx), act = "action",mhms = ["hh","mm","ss"], 
                        ulCell = $Q(".jedate-time",that.dateCell).querySelectorAll("ul")[pidx], 
                        tlen = that.$data.timelist[0].length;
                    if(jet.hasClass(this,"disabled")) return;
                    jet.each(ulCell.childNodes,function (i,node) {
                        var reg = new RegExp("(^|\\s+)" + act + "(\\s+|$)", "g");
                        node.className = reg.test(node.className) ? node.className.replace(reg, '') : node.className;
                    });
                    that.selectTime[paridx][paridx == 1 ? mhms[pidx-tlen]:mhms[pidx]] = vals;
                    this.className = this.className + act;
                    var hmsCls = ulCell.querySelector("."+act);
                    ulCell.scrollTop = hmsCls ? (hmsCls.offsetTop-145):0; 
                    if(that.dlen == 7 && idx == 0 && range && !multi){
                        var nVal = that.getValue({}), nYM = jet.nextMonth(nVal[0].YYYY,nVal[0].MM),st = that.selectTime;
                        that.storeData(
                            {YYYY:nVal[0].YYYY,MM:nVal[0].MM,DD:null,hh:st[0].hh,mm:st[0].mm,ss:st[0].ss},
                            {YYYY:nYM.y,MM:nYM.m,DD:null,hh:st[1].hh,mm:st[1].mm,ss:st[1].ss}
                        );
                        that.renderDate();
                    }
                },
                timeBtn:function() {
                    var timeCell = $Q(".jedate-time",elCell), disNo = timeCell.style.display == "none";
                    jet.text(this,disNo ? opts.language.backtxt:opts.language.timetxt[0]);
                    jet.setCss(timeCell,{display: disNo ? "block":"none"});
                },
                //清空按钮函数
                clearBtn:function () {
                    jet.valText(that.valCell,"");
                    that.selectDate = [jet.parse(jet.getDateTime({}),"YYYY-MM-DD hh:mm:ss")];
                    that.closeDate();
                    opts.clearfun && opts.clearfun.call(that); 
                },
                //现在按钮函数
                nowBtn:function () {
                    var newArr = jet.getDateTime({}), nYM = jet.nextMonth(newArr.YYYY,newArr.MM), dateVal;   
                    that.selectDate = [newArr];
                    dateVal = opts.isShow ? that.setValue([newArr],that.format,true) : jet.parse(newArr,that.format);
                    if(opts.onClose && range || !opts.isShow){
                        that.storeData(newArr,{YYYY:nYM.y,MM:nYM.m,DD:null,hh:0,mm:0,ss:0});
                        that.renderDate();
                    }else{
                        that.closeDate();
                    } 
                    opts.donefun && opts.donefun.call(that,{elem:that.valCell,val:dateVal,date:newArr}); 
                },
                //确认按钮函数
                sureBtn:function () {
                    var newArr = that.selectValue.length > 1 ? [{},{}]: [{}], dateVal = "", tmval = that.selectTime;
                    var equal = function (o) {
                        var h = o.hh == undefined ? 0:o.hh, m = o.mm == undefined ? 0:o.mm, s = o.ss == undefined ? 0:o.ss;
                        return parseInt(jet.digit(h)+""+jet.digit(m)+""+jet.digit(s));
                    };
                    if(range){
                        if(that.selectValue.length > 1){
                            var sortarr = that.selectValue;
                            sortarr.sort(function(a, b){ return a > b ? 1 : -1; });
                            jet.each(sortarr,function (i,arr) {
                                jet.each(jet.reMatch(arr),function (a,val) {
                                    newArr[i][ymdzArr[a]] = val; 
                                });
                                jet.extend(newArr[i],tmval[i]);
                            }); 
                        }else if(that.dlen == 7 && tmval.length>1){
                            newArr = tmval;
                        } 
                        var sameTime = equal(tmval[0]) >= equal(tmval[1]),selVal = that.selectValue, sameDate = "";
                        if(selVal[1] != undefined) sameDate = selVal[0].replace(/\-/g,"") == selVal[1].replace(/\-/g,"");
                        if(selVal.length == 1 && that.dlen < 7){
                            that.tips(opts.language.name == "cn" ? "未选结束日期" : "Please select the end date"); return;
                        }else if((that.dlen == 7 && sameTime) || (sameDate && sameTime)){
                            that.tips(opts.language.name == "cn" ? "结束时间必须大于开始时间" : "The end time must be greater than the start time"); return;
                        } 
                    }else{
                        jet.each(new Array(range == false ? 1 : 2),function (i) {
                            if(that.dlen != 7) jet.each(jet.reMatch(that.selectValue[0]),function (a,val) {
                                newArr[i][ymdzArr[a]] = val;
                            });
                            jet.extend(newArr[i],tmval[i]);
                        });
                    } 
                    dateVal = that.setValue(newArr,that.format,opts.isShow ? true:false);
                    opts.isShow && that.closeDate();
                    opts.donefun && opts.donefun.call(that,{elem:that.valCell,val:dateVal,date:newArr}); 
                },
                blankArea:function () {
                    jet.on(document,"mouseup",function (ev) {
                        jet.stopPropagation(ev);  
                        that.closeDate();                      
                    });
                    jet.on($Q(elx),"mouseup", function(ev) {
                        jet.stopPropagation(ev); 
                    });
                }   
            });
        },
        //循环生成年数据
        eachYear  : function (val,type) {
            var that = this,opts = that.$opts, yNum = parseInt(val),yarr = [],seCls='', selYear = that.selectDate,i,
                mins = jet.reMatch(that.minDate), maxs = jet.reMatch(that.maxDate);   
            i = type == 1 ? yNum : that.yindex;  
            that.yindex = type == 1 ? 12+yNum : 12+that.yindex;
            var endDate = selYear[1] == undefined ? "":selYear[1].YYYY;
            for(; i < that.yindex; i++){
                if(i == selYear[0].YYYY || i == endDate){
                    seCls = "action";
                }else if(i>selYear[0].YYYY && i<endDate){
                    seCls = "contain";
                }else if(i < mins[0] || i > maxs[0]){
                    seCls = "disabled";
                }else{
                    seCls =  "";
                }
                yarr.push({style:seCls,y:i});    
            }
            return yarr;
        },
        //循环生成月数据
        eachMonth : function (val,type) {
            var that = this,opts = that.$opts, range = opts.range, marr = [],
                selMonth = that.selectDate, seCls='',monthArr = opts.language.month ,
                mins = jet.reMatch(that.minDate), maxs = jet.reMatch(that.maxDate),
                minym = parseInt(mins[0]+""+jet.digit(mins[1])), 
                maxym =  parseInt(maxs[0]+""+jet.digit(maxs[1])),
                currStart = parseInt(selMonth[0].YYYY+""+jet.digit(selMonth[0].MM)),
                currEnd = selMonth[1] ? parseInt(selMonth[1].YYYY+""+jet.digit(selMonth[1].MM)) : 0;
            jet.each(monthArr,function (i,months) {
                var ival = parseInt(val+""+jet.digit(months));
                if(ival == currStart || ival == currEnd){
                    seCls = "action";
                }else if(ival > currStart && ival < currEnd){ 
                    seCls = "contain";  
                }else if(ival < minym || ival > maxym){
                    seCls = "disabled";
                }else{
                    seCls = "";
                }
                marr.push({style:seCls ,y:val ,m:months}); 
            }); 
            return marr;
        },
        //循环生成天数据
        eachDays  : function (yd,md,ds,idx) {
            var that = this, count = 0, daysArr = [],opts = that.$opts, multiPane = jet.isBool(opts.multiPane),
                firstWeek = new Date(yd, md - 1, 1).getDay() || 7,valrange = opts.range != false,
                daysNum = jet.getDaysNum(yd, md), didx = 0, sDate = that.selectDate,
                prevM = jet.prevMonth(yd, md),isShow = jet.isBool(opts.isShow),
                prevDaysNum = jet.getDaysNum(yd, prevM.m),nextM = jet.nextMonth(yd, md), objCell = that.valCell,
                lang = opts.language, endval = opts.valiDate||[],
                minArr = jet.reMatch(that.minDate), minNum = parseInt(minArr[0]+""+jet.digit(minArr[1])+""+jet.digit(minArr[2])),
                maxArr = jet.reMatch(that.maxDate), maxNum = parseInt(maxArr[0]+""+jet.digit(maxArr[1])+""+jet.digit(maxArr[2]));
            var startDate = sDate[0] ? parseInt(sDate[0].YYYY+""+jet.digit(sDate[0].MM)+""+jet.digit(sDate[0].DD)) : "";
            var endDate = sDate[1] ? parseInt(sDate[1].YYYY+""+jet.digit(sDate[1].MM)+""+jet.digit(sDate[1].DD)) : "";
            //设置时间标注   
            var setMark = function (my, mm, md) {
                var Marks = opts.marks, contains = function(arr, obj) {
                    var clen = arr.length;
                    while (clen--) {  if (arr[clen] === obj) return true; }
                    return false;
                },isArr = jet.isType(Marks,"array");  
                return isArr && Marks.length > 0 && contains(Marks, my + "-" + jet.digit(mm) + "-" + jet.digit(md)) ? '<i class="marks"></i>' :"";
            };
            //是否显示节日
            var isfestival = function(y, m ,d) {
                var festivalStr = '';
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
            var dateLimit = function(Y, M, D, isMonth){
                var thatNum = parseInt(Y + "" + jet.digit(M) + "" + jet.digit(D));
                if(isMonth){
                    if (thatNum >= minNum && thatNum <= maxNum) return true;
                }else {
                    if (minNum > thatNum || maxNum < thatNum) return true;
                }
            };
            
            var regExpDate = function (date,cls) {
                var inArray = function (search,array){
                    for(var i in array) if(array[i]==search) return true;
                    return false;
                };
                if(endval.length > 0 && endval[0]!=""){
                    if(/\%/g.test(endval[0])){
                        var reval = endval[0].replace(/\%/g,"").split(","), enArr = [];
                        jet.each(reval,function (r,rel) {
                            enArr.push(jet.digit(parseInt(rel)));
                        });
                        var isfind = inArray(jet.digit(date), enArr) == false;
                        cls = jet.isBool(endval[1]) ? (isfind ? " disabled" :cls) : (isfind ? cls :" disabled");
                    }else {
                        var valreg = that.dateRegExp(endval[0]), regday = valreg.test(jet.digit(date));
                        cls = jet.isBool(endval[1]) ? (regday ? " disabled" : cls) : (regday ? cls : " disabled");
                    }
                }
                return cls;
            };

            //var ymds = ymdarr[1]
            //上一月剩余天数
            for (var p = prevDaysNum - firstWeek + 1; p <= prevDaysNum; p++, count++) {
                var pmark = setMark(prevM.y,prevM.m,p), pcls = dateLimit(prevM.y, prevM.m, p, false) ? "disabled" : "other";
                pcls = regExpDate(p,pcls);
                daysArr.push({style:pcls,ymd:prevM.y+'-'+jet.digit(prevM.m)+'-'+jet.digit(p),day:(isfestival(prevM.y,prevM.m,p) + pmark)});
            }
            //本月的天数
            for(var b = 1; b <= daysNum; b++, count++){
                var bmark = setMark(yd,md,b), bcls = "";
                var dateval = parseInt(yd+""+jet.digit(md)+""+jet.digit(b)),
                    parsdate = dateval > startDate, rangdate = dateval < endDate;
                if(dateLimit(yd, md, b, true)){
                    if(dateval == startDate || dateval == endDate){
                        bcls = " action";
                    }else if(parsdate&&rangdate){
                        bcls = " contain";
                    }else {
                        bcls = "";
                    }
                }else {
                    bcls = " disabled";
                }
                bcls = regExpDate(b,bcls);
                daysArr.push({style:"normal"+bcls,ymd:yd+'-'+jet.digit(md)+'-'+jet.digit(b),day:(isfestival(yd,md,b) + bmark)});
            }
            //下一月开始天数
            for(var n = 1, nlen = 42 - count; n <= nlen; n++){
                var nmark = setMark(nextM.y,nextM.m,n);
                var ncls = dateLimit(nextM.y, nextM.m, n, false) ? "disabled" : "other";
                ncls = regExpDate(n,ncls);
                daysArr.push({style:ncls,ymd:nextM.y+'-'+jet.digit(nextM.m)+'-'+jet.digit(n),day:(isfestival(nextM.y,nextM.m,n) + nmark)});
            }
            //将星期与日期拼接起来
            return daysArr;  
        },
        eachTime  : function (hmsArr,type) {  
            var that = this,opts = that.$opts, range = opts.range,multi = opts.multiPane, minVal = [], maxVal = [],
                mhms = ["hh","mm","ss"], timeArr = [],hmsCls = '',format = that.format,    
                ntVal = jet.trim(that.minDate).replace(/\s+/g," "), 
                xtVal = jet.trim(that.maxDate).replace(/\s+/g," "), 
                nVal = ntVal.split(" "), xVal = xtVal.split(" ");
            if(that.dlen>3 && /\:/.test(nVal) && /\:/.test(xVal)){
                minVal = jet.reMatch(/\s/.test(ntVal)&&that.dlen>3 ? nVal[1] : ntVal);
                maxVal = jet.reMatch(/\s/.test(xtVal)&&that.dlen>3 ? xVal[1] : xtVal);
            }
            jet.each([24,60,60],function (s,lens) {
                timeArr[s] = [];
                var unhmsVal = minVal[s] == undefined || minVal[s] == 0 ? hmsArr[mhms[s]] : minVal[s],
                currVal = that.getValue() == "" ? unhmsVal : hmsArr[mhms[s]];
                if(that.dlen>3 && /\:/.test(nVal) && type==1){
                    that.selectTime[0][mhms[s]] = currVal;
                }
                for (var h = 0; h < lens; h++) {
                    var exists = new RegExp(mhms[s],"g").test(format);
                    if(h == currVal){
                        hmsCls = exists ? "action" : "disabled"; 
                    }else if(!exists || !range && multi &&(h<minVal[s] ||h>maxVal[s])){    
                        hmsCls = "disabled";
                    }else if(!multi){
                        hmsCls = type == 1&&h<minVal[s] || type == 2&&h>maxVal[s] ? "disabled" : "";
                    }else{
                        hmsCls = ""; 
                    }
                    timeArr[s].push({style:hmsCls,hms:h});
                }
            }); 
            return timeArr;
        },
        //关闭日期控件
        closeDate : function () {
            var elem = $Q(elx), tipelem = $Q("#jedatetipscon");
            elem && document.body.removeChild(elem);
            tipelem && document.body.removeChild(tipelem);
            //再次初始化值
            this.setDatas();
        },
        //转换日期值
        parseValue : function (fnObj,matStr) {
            var that = this, valArr=[],opts = that.$opts, range = opts.range;
            jet.each(fnObj,function (i,val) {
                valArr.push(jet.parse(val, matStr));
            });
            return range == false ? valArr[0] : valArr.join(range);
        },
        //初始验证正则
        dateRegExp : function(valArr) {
            var enval = valArr.split(",")||[], regs = "";
            var doExp = function (val) {
                var arr, tmpEval, regs = /#?\{(.*?)\}/;
                val = val + "";
                while ((arr = regs.exec(val)) != null) {
                    arr.lastIndex = arr.index + arr[1].length + arr[0].length - arr[1].length - 1;
                    tmpEval = parseInt(eval(arr[1]));
                    if (tmpEval < 0) tmpEval = "9700" + -tmpEval;
                    val = val.substring(0, arr.index) + tmpEval + val.substring(arr.lastIndex + 1);
                }
                return val;
            };
            if (enval && enval.length > 0) {
                for (var i = 0; i < enval.length; i++) {
                    regs += doExp(enval[i]);
                    if (i != enval.length - 1) regs += "|";
                }
                regs = regs ? new RegExp("(?:" + regs + ")") : null;
            } else {
                regs = null;
            }
            //re = new RegExp((re + "").replace(/^\/\(\?:(.*)\)\/.*/, "$1"));
            return regs;
        },
        //显示农历节日
        showFestival:function () {
            var that = this, opts = that.$opts;
            jet.each(that.dateCell.querySelectorAll(".daystable td"),function (i,node) {
                var tval = jet.reMatch(jet.attr(node,"ymd")),tipDiv = document.createElement("div");
                node.removeAttribute("ymd");
                //鼠标进入提示框出现
                jet.on(node,"mouseover",function () {
                    var lunar = new jeLunar(tval[0], tval[1] - 1, tval[2]);
                    if($Q("#jedatetipscon")) return;
                    tipDiv.id = tipDiv.className = "jedatetipscon";
                    var tiphtml = '<p>' + lunar.solarYear + '\u5E74' + lunar.solarMonth + '\u6708' + lunar.solarDate + '\u65E5 ' + lunar.inWeekDays + '</p><p class="red">\u519C\u5386：' + lunar.shengxiao + '\u5E74 ' + lunar.lnongMonth + '\u6708' + lunar.lnongDate + '</p><p>' + lunar.ganzhiYear + '\u5E74 ' + lunar.ganzhiMonth + '\u6708 ' + lunar.ganzhiDate + '\u65E5</p>';
                    var Fesjieri = (lunar.solarFestival || lunar.lunarFestival) != "" ? '<p class="red">' + ("\u8282\u65E5："+lunar.solarFestival + lunar.lunarFestival) + '</p>' : "";
                    var Fesjieqi = lunar.jieqi != "" ? '<p class="red">'+(lunar.jieqi != "" ? "\u8282\u6C14："+lunar.jieqi : "") + '</p>': "";
                    var tiptext = (lunar.solarFestival || lunar.lunarFestival || lunar.jieqi) != "" ? (Fesjieri + Fesjieqi) : "";
                    jet.html(tipDiv,tiphtml + tiptext);
                    document.body.appendChild(tipDiv);
                    //获取并设置农历提示框出现的位置
                    var tipPos = that.lunarOrien(tipDiv, this);
                    jet.setCss(tipDiv,{"zIndex":  (opts.zIndex == undefined ? 10000 + 5 : opts.zIndex + 5),top:tipPos.top,left:tipPos.left,position:"absolute",display:"block"});
                });
                //鼠标移除提示框消失
                jet.on(node,"mouseout",function () {
                    document.body.removeChild($Q("#jedatetipscon"));
                });
            });
            if (that.dateCell.nodeType === 1 && !jet.hasClass(that.dateCell,"grid")) that.dateCell.className = that.dateCell.className + " grid";
        },
        //农历方位辨别
        lunarOrien : function(obj, self, pos) {
            var tops, leris, ortop, orleri, rect =self.getBoundingClientRect(), boxW = obj.offsetWidth, boxH = obj.offsetHeight;
            leris = rect.right + boxW / 1.5 >= jet.docArea(true) ? rect.right - boxW : rect.left + (pos ? 0 : jet.docScroll(true));
            tops = rect.bottom + boxH / 1 <= jet.docArea() ? rect.bottom - 1 : rect.top > boxH / 1.5 ? rect.top - boxH - 1 : jet.docArea() - boxH;
            if(leris + boxW > jet.docArea(true)) leris = rect.left - (boxW - rect.width);
            ortop = Math.max(tops + (pos ? 0 :jet.docScroll()) + 1, 1) + "px", orleri = leris + "px";
            return {top: ortop, left: orleri }
        },
        //辨别控件的方位
        dateOrien : function(elbox, valCls, pos) {
            var that = this, tops, leris, ortop, orleri,
                rect = that.$opts.fixed ? valCls.getBoundingClientRect() : elbox.getBoundingClientRect(),
                leris = rect.left, tops = rect.bottom;
            if(that.$opts.fixed) {
                var boxW = elbox.offsetWidth, boxH = elbox.offsetHeight;
                //如果右侧超出边界
                if(leris + boxW > jet.docArea(true)){
                    leris = leris - (boxW - rect.width);
                }
                //如果底部超出边界
                if(tops + boxH > jet.docArea()){
                    tops = rect.top > boxH ? rect.top - boxH -2 : jet.docArea() - boxH -1;
                }
                //根据目标元素计算弹层位置
                ortop = Math.max(tops + (pos ? 0 :jet.docScroll())+1, 1) + "px"; orleri = leris + "px";
            }else{
                //弹层位置位于页面上下左右居中
                ortop = "50%"; orleri = "50%";
                elbox.style.cssText = "marginTop:"+-(rect.height / 2)+";marginLeft:"+-(rect.width / 2);
            }
            jet.setCss(elbox,{top:ortop,left:orleri});
        },
        tips : function (text, time) {
            var that = this, tipCls = $Q(".jedate-tips",that.dateCell),tipTime;
            jet.html(tipCls,text||""); jet.setCss(tipCls,{display:"block"});
            clearTimeout(tipTime);
            tipTime = setTimeout(function(){
                jet.html(tipCls,""); jet.setCss(tipCls,{display:"none"});
            }, (time||2.5)*1000);
        },
        locateScroll : function () {
            var that = this, ulCell = $Q(".jedate-time",that.dateCell).querySelectorAll("ul");
            jet.each(ulCell, function(i,cell) {
                var hmsCls = cell.querySelector(".action");
                cell.scrollTop = hmsCls ? (hmsCls.offsetTop-145):0;
            });
            if(that.dlen != 7) jet.setCss($Q(".jedate-time",that.dateCell),{display:'none'});
        }
    });
    
    //农历数据
    function jeLunar(ly,lm,ld) {
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
                return (h.getUTCDate());
            },
            d = function (k) {
                var h, j = 348;
                for (h = 32768; h > 8; h >>= 1) j += (lunarInfo[k - 1900] & h) ? 1 : 0;
                return (j + b(k));
            },
            ymdCyl = function (h) {
                return (Gan.charAt(h % 10) + Zhi.charAt(h % 12));
            },
            b = function (h) {
                var islp = (g(h)) ? ((lunarInfo[h - 1900] & 65536) ? 30 : 29) : (0);
                return islp;
            },
            g = function (h) {
                return (lunarInfo[h - 1900] & 15)
            },
            e = function (i, h) {
                return ((lunarInfo[i - 1900] & (65536 >> h)) ? 30 : 29);
            },
            newymd = function (m) {
                var k, j = 0, h = 0, l = new Date(1900, 0, 31), n = (m - l) / 86400000;
                this.dayCyl = n + 40;
                this.monCyl = 14;
                for (k = 1900; k < 2050 && n > 0; k++) {
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
                for (k = 1; k < 13 && n > 0; k++) {
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
                    --this.monCyl;
                }
                this.month = k;
                this.day = n + 1;
            },
            digit = function (num) {
                return num < 10 ? "0" + (num | 0) : num;
            },
            reymd = function (i, j) {
                var h = i;
                return j.replace(/dd?d?d?|MM?M?M?|yy?y?y?/g, function (k) {
                    switch (k) {
                        case "yyyy":
                            var l = "000" + h.getFullYear();
                            return l.substring(l.length - 4);
                        case "dd": return digit(h.getDate());
                        case "d": return h.getDate().toString();
                        case "MM": return digit((h.getMonth() + 1));
                        case "M": return h.getMonth() + 1;
                    }
                });
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
                return j;
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
            if (this.lunarMonth == 12 && this.lunarDate == e(this.lunarYear, 12)) {
                this.lunarFestival = sFtv2["0100"];
                this.restDays = 1;
            }
            this.showInLunar = (this.lunarFestival == "") ? this.showInLunar : this.lunarFestival;
        }
        return new flunar(new Date(ly, lm, ld));
    }
    return jeDate;
});
