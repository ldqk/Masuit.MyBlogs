jQuery(function ($) {
    $.supersized({//参数配置项
        // 功能
        slide_interval: 4000,    // 转换之间的长度
        transition: 1,    // 0 - 无，1 - 淡入淡出，2 - 滑动顶，3 - 滑动向右，4 - 滑底，5 - 滑块向左，6 - 旋转木马右键，7 - 左旋转木马
        transition_speed: 1000,    // 转型速度
        performance: 1,    // 0 - 正常，1 - 混合速度/质量，2 - 更优的图像质量，三优的转换速度//（仅适用于火狐/ IE浏览器，而不是Webkit的）

        // 大小和位置
        min_width: 0,    // 最小允许宽度（以像素为单位）
        min_height: 0,    // 最小允许高度（以像素为单位）
        vertical_center: 1,    // 垂直居中背景
        horizontal_center: 1,    // 水平中心的背景
        fit_always: 0,    // 图像绝不会超过浏览器的宽度或高度（忽略分钟。尺寸）
        fit_portrait: 1,    // 纵向图像将不超过浏览器高度
        fit_landscape: 0,    // 景观的图像将不超过宽度的浏览器

        // 组件
        slide_links: 'blank',    // 个别环节为每张幻灯片（选项：假的，'民'，'名'，'空'）
        slides: [    // 幻灯片影像
                { image: '../../Content/images/1.jpg' },
                { image: '../../Content/images/2.jpg' },
                { image: '../../Content/images/3.jpg' },
                { image: '../../Content/images/4.jpg' },
                { image: '../../Content/images/5.jpg' },
                { image: '../../Content/images/6.jpg' },
                { image: '../../Content/images/7.jpg' },
                { image: '../../Content/images/8.jpg' },
                { image: '../../Content/images/9.jpg' }
        ]
    });
});
