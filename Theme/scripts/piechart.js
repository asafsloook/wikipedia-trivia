/* 
 Created on : 06.04.2017, 09:56:13
 Author     : Ramon Hirsinger
 */
(function ($) {

    $.fn.piechart = function (options) {
        $defaults = {
            allCategoriesToggled: false,
            categoryToggled: 4,
            categoryToggledRandom: false,
            containerWidth: '100%',
            canvasHeight: '280px',
            canvasWidth: '280px',
            toggleBtnClass: 'piechart-category-title-row',
            toggleContainerClass: 'piechart-category-content-row',
            toggleOpenClass: 'toggle-open',
            pieStyle: {
                fillStyle: "#00B0AF",
                overlayStyle: "#CA0097",
                stroke: false,
                lineWidth: 0,
                strokeStyle: '#000000',
                radius: 100
            },
            pieAnimation: {
                nextTime: 0,
                pct: 0
            }
        };

        var settings = $.extend({}, $defaults, options);
        var data = settings.data;

        var containerOpt = {
            parentAttr: {
                class: "piechart-container container"
            },
            wrapperAttr: {
                class: "piechart-wrapper col-xs-12 col-sm-6 col-md-4"
            },
            wrapperCSS: {
                textAlign: 'center'
            },
            catWrapperAttr: {
                class: 'piechart-catagory-wrapper'
            },
            catWrapperCSS: {

            },
            catRowTitleAttr: {
                class: settings.toggleBtnClass + ' row'
            },
            catRowTitleCSS: {

            },
            catRowTitleColAttr: {
                class: 'piechart-category-title-col col-xs-12'
            },
            catRowTitleColCSS: {

            },
            catRowContentAttr: {
                class: settings.toggleContainerClass + ' row'
            },
            catRowContentCSS: {

            },
            catRowBtnAttr: {
                class: 'piechart-category-akkordeon-headline'
            },
            catRowBtnCSS: {

            },
            elementWrapperAttr: {
                class: 'piechart-element-wrapper'
            },
            elementWrapperCSS: {
                width: settings.containerWidth,
                display: 'inline-block'
            },
            elementInnerWapperAttr: {
                class: 'piechart-element-inner-wrapper'
            },
            elementInnerWrapperCSS: {
                overflow: 'hidden',
                position: 'relative',
                zIndex: '990',
                paddingBottom: '100%'
            },
            elementCanvasAttr: {
                class: 'piechart-canvas'
            },
            elementCanvasCSS: {
                position: 'absolute',
                top: '0',
                left: '0',
                right: '0',
                bottom: '0',
                margin: 'auto auto',
                zIndex: '999',
            },
            elementTitleAttr: {
                class: 'piechart-element-title'
            },
            elementTitleCSS: {
                position: 'absolute',
                top: '10px',
                left: '0',
                width: '100%',
                textAlign: 'center',
                fontFamily: 'Roboto, Arial, sans-serif',
                fontSize: '20px',
                fontWeight: '300',
                padding: '0',
                margin: '0',
                zIndex: '1000'
            }
        }

        init(this, containerOpt, settings, data);
        toggle(settings);
//        resize(settings);
    };

    function toggle(settings) {
        $clickRows = $('.' + settings.toggleBtnClass);

        $('.' + settings.toggleBtnClass).on('click', function () {
            $this = $(this);
            if ($this.hasClass(settings.toggleOpenClass)) {
                $this.removeClass(settings.toggleOpenClass).next().removeClass(settings.toggleOpenClass);
            } else {
                $this.addClass(settings.toggleOpenClass).next().addClass(settings.toggleOpenClass);
                paintSingleTab($clickRows.index($this));
            }
        });
    }

    function resize(settings) {

        $data = settings.data;
        $openContentRow = $('.' + settings.toggleContainerClass + '.' + settings.toggleOpenClass);

        $(window).resize(function () {
            $openContentRow.each(function () {
                $this = $(this);
                $index = $openContentRow.index($this);
                resizePieChart($index);
            });
        });
    }

    function init(parent, containerOpt, settings, data) {
        $settings = settings;
        $parent = parent;
        $opt = containerOpt;
        $data = data;
        $values = new Array();

        $piechartset_title = "";
        $piechartset_val = 0;
        $piecharset_cat = "";

        $parent.attr($opt.parentAttr);
        $catwrapper = $('<div />').attr($opt.catWrapperAttr)
                .css($opt.catWrapperCSS);

        if ($settings.categoryToggledRandom) {
            $randomToggled = getRandom(1, $data.length) - 1;
        }

        for (i = 0; i < $data.length; i++) {
            for (v = 0; v < $data[i].length; v++) {
                $.each($data[i][v], function (key, value) {
                    if (key === "val") {
                        $values.push(value);
                    }
                    if (key === "label") {
                        $piechartset_title = value;
                    }

                    if (key === "cat") {
                        $piecharset_cat = value;
                    }
                });

                if (v === 0) {
                    $catwrapper.appendTo($parent);
                    $catrowtitle = $('<div />').attr($opt.catRowTitleAttr)
                            .addClass('cat-index-' + i)
                            .css($opt.catRowTitleCSS)
                            .appendTo($catwrapper);
                    $catrowtitlecol = $('<div />').attr($opt.catRowTitleColAttr)
                            .appendTo($catrowtitle);
                    $catrowbody = $('<div />').attr($opt.catRowContentAttr)
                            .addClass('cat-index-' + i)
                            .css($opt.catRowContentCSS)
                            .appendTo($catwrapper);

                    //CHECK WHETER ALL CATEGORIES ARE INITIAL TOGGLED OR NOT        
                    if ($settings.allCategoriesToggled) {
                        $catrowtitle.addClass($settings.toggleOpenClass);
                        $catrowbody.addClass($settings.toggleOpenClass);
                    } else {
                        // IF NOT RANDOM ROW TOGGLE
                        if (!$settings.categoryToggledRandom) {
                            $categoryToggled = $settings.categoryToggled;
                            if (i === ($categoryToggled - 1)
                                    && ($categoryToggled > 0 && $categoryToggled <= $data.length)
                                    ) {
                                $catrowtitle.addClass($settings.toggleOpenClass);
                                $catrowbody.addClass($settings.toggleOpenClass);
                            }
                            //IF RANDOM ROW TOGGLE
                        } else {
                            if (i === $randomToggled) {
                                $catrowtitle.addClass($settings.toggleOpenClass);
                                $catrowbody.addClass($settings.toggleOpenClass);
                            }
                        }
                    }

                    $cattitle = $('<span>' + $piecharset_cat + '</span>').attr($opt.catRowBtnAttr)
                            .appendTo($catrowtitlecol);
                }

                $wrapper = $('<div />').attr($opt.wrapperAttr)
                        .css($opt.wrapperCSS)
                        .appendTo($catrowbody);
                $elementWrapper = $('<div />').attr($opt.elementWrapperAttr)
                        .css($opt.elementWrapperCSS)
                        .appendTo($wrapper);
                $elementInnerWrapper = $('<div />').attr($opt.elementInnerWapperAttr)
                        .css($opt.elementInnerWrapperCSS)
                        .appendTo($elementWrapper);
                $piechartCanvas = $('<canvas width="' + $settings.canvasWidth + '" height="' + $settings.canvasHeight + '">').attr($opt.elementCanvasAttr)
                        .attr("id", "pieChartCanvas_" + $piecharset_cat + "_" + v)
                        .css($opt.elementCanvasCSS)
                        .appendTo($elementInnerWrapper);
                $title = $('<h5>' + $piechartset_title + '</h5>')
                        .attr($opt.elementTitleAttr)
                        .css($opt.elementTitleCSS)
                        .prependTo($elementInnerWrapper);
            }
        }

        if ($settings.allCategoriesToggled) {
            paintAllTabs();
        } else {
            if ($settings.categoryToggledRandom) {
                paintSingleTab($randomToggled);
            } else {
                paintSingleTab($settings.categoryToggled - 1);
            }
        }
    }

    function getValueArrayByIndex(index) {
        $values = new Array();
        for (v = 0; v < $data[index].length; v++) {
            $.each($data[index][v], function (key, value) {
                if (key === "val") {
                    $values.push(value);
                }
            });
        }
        return $values;
    }

    function getRandom(min, max) {
        return min + Math.floor(Math.random() * (max - min + 1));
    }

    function paintSingleTab(index) {
        $canvasArray = $('.piechart-category-content-row.cat-index-' + index).find('canvas');
        paintPieCharts(getValueArrayByIndex(index), $canvasArray, $settings);
    }

    function paintAllTabs() {
        $canvasArray = $('canvas');
        paintPieCharts($values, $canvasArray, $settings);
    }

    function resizePieChart(index) {
        $canvasArray = $('.piechart-category-content-row.cat-index-' + index).find('canvas');
        repaintPieChart($values, $canvasArray);
    }

    function repaintPieChart(values, canvasarray) {

        $values = values;
        canvasarray.each(function (index, val) {
            $endingPct = $values[index] * 10;
            $canvas = document.getElementById($(this).attr("id"));
            $context = $canvas.getContext('2d');
            $centerX = $canvas.width / 2;
            $centerY = $canvas.height / 2;
            draw($endingPct, $context, $centerX, $centerY, $radius);
        });
    }

    function paintPieCharts(values, canvasarray, settings) {

        $settings = settings;
        $values = values;
        $radius = $settings.pieStyle.radius;
        $pct = $settings.pieAnimation.pct;
        $fps = 80;
        $tfilled = 0;
        $drawSteps = 2;
        $duration = 1000;
        $rate = 10;
        
        $interval = window.setInterval(function () {
            
            canvasarray.each(function (index, val) {
                $endingPct = $values[index] * 10;
                $canvas = document.getElementById($(this).attr("id"));
                $context = $canvas.getContext('2d');
                $centerX = $canvas.width / 2;
                $centerY = $canvas.height / 2;
                
                if ($pct <= $endingPct) {
                    draw($pct, $context, $centerX, $centerY, $radius);
                }
            });
            
            $pct += $drawSteps;
            
            $tfilled += $rate;

            if ($tfilled >= $duration) {
                window.clearInterval($interval);
            }

        }, $rate);

    }

    function draw(pct, context, centerX, centerY, radius) {
        $context = context;
        $pct = pct;
        $centerX = centerX;
        $centerY = centerY;

        $context.clearRect(0, 0, $centerX * 2, $centerY * 2);

        $radius = radius;
        $endRadians = -Math.PI / 2 + Math.PI * 2 * $pct / 100;
        $context.beginPath();
        $context.arc($centerX, $centerY, $radius, 0, 2 * Math.PI);
        $context.fillStyle = $settings.pieStyle.fillStyle;
        $context.fill();
        $context.closePath();
        
        $context.beginPath();
        $context.arc($centerX, $centerY, $radius, -Math.PI / 2, $endRadians);
        $context.lineTo($centerX, $centerY);
        $context.fillStyle = $settings.pieStyle.overlayStyle;
        $context.fill();

        $context.font = 'lighter 18px Roboto, Arial, sans-serif';
        $context.fillStyle = '#000000';
        $context.fillText($pct + '%',$centerX - 20,($centerY * 2));

        $context.closePath();
    }

}(jQuery));
