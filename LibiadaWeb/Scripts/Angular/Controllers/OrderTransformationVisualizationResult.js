﻿function OrderTransformationVisualizationResultController() {
    "use strict";

    function orderTransformationVisualizationResult($scope, $http) {

        // initializes data for chart
        function fillLegend() {
            $scope.legend = [];
            for (var i = 0; i < $scope.transformationsName.length; i++) {
                $scope.legend.push({ name: $scope.transformationsName[i], visible: true });
            }
        }

        // initializes data for chart
        function fillPointsAndLines() {
            $scope.points = [];
            $scope.lines = [];
            var initialOrder = $scope.initialOrder.OrderId;
            var typeOfTransformation = $scope.typeOfTransformation.Text;
            var addedOrders = [initialOrder];
            var ordersForChecking = [initialOrder];
            var visibilityTranform = [];
            for (var l = 0; l < $scope.legend.length; l++) {
                visibilityTranform.push({ name: $scope.legend[l].name, visible: $scope.legend[l].visible });
            }
            $scope.points.push({
                id: 0,
                Value: initialOrder,
                x: 0,
                y: initialOrder,
                visibilityTranformation: visibilityTranform
            });
            var counterIdPoints = 1;
            var counterIdLines = 0;
            $scope.counterIteration = 1;
            while (ordersForChecking.length > 0) {
                var newOrdersForChecking = [];
                for (var i = 0; i < ordersForChecking.length; i++) {
                    for (var j = 0;
                        j < $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation.length;
                        j++) {
                        if ((typeOfTransformation === "All" ||
                            $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j].Transformation ===
                            typeOfTransformation)) {
                            var pointExist = false;
                            for (var k = 0; k < $scope.points.length; k++) {
                                if ($scope.points[k].x === $scope.counterIteration &&
                                    $scope.points[k].y ===
                                    $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .OrderId) {
                                    pointExist = true;
                                    break;
                                }
                            }
                            if (!pointExist) {
                                $scope.points.push({
                                    id: counterIdPoints,
                                    Value: $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .OrderId,
                                    x: $scope.counterIteration,
                                    y: $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .OrderId,
                                    visibilityTranformation: visibilityTranform
                                });
                                counterIdPoints++;
                            }
                            if (addedOrders.indexOf(
                                $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                    .OrderId) === -1) {
                                addedOrders.push(
                                    $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j].OrderId);
                                newOrdersForChecking.push(
                                    $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j].OrderId);
                            }
                            var lineExist = false;
                            var lineIterator = 0;
                            for (var k = 0; k < $scope.lines.length; k++) {
                                if ($scope.lines[k].x1 === $scope.counterIteration - 1 &&
                                    $scope.lines[k].y1 === ordersForChecking[i] &&
                                    $scope.lines[k].x2 === $scope.counterIteration &&
                                    $scope.lines[k].y2 === $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j].OrderId) {
                                    lineExist = true;
                                    lineIterator = ++$scope.lines[k].iterator;
                                    break;
                                }
                            }
                            if (lineExist) {
                                var cyline =
                                    ($scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .OrderId +
                                        ordersForChecking[i]) /
                                    2.0;
                                var a = $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                    .OrderId -
                                    ordersForChecking[i];
                                var shifty = 0.1 + 0.05 * Math.abs(a);
                                var shiftx = 0.1 + 0.005 * $scope.countOfUniqueFinalOrders.length;
                                var shift = shifty > shiftx ? shifty : shiftx;
                                $scope.lines.push({
                                    id: counterIdLines,
                                    Value: $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .Transformation,
                                    arrowType: $scope.legend.length,
                                    iterator: 0,
                                    x1: $scope.counterIteration - 1,
                                    y1: ordersForChecking[i],
                                    x2: $scope.counterIteration - 0.5,
                                    y2: cyline + shift * lineIterator,
                                    startOrderId: ordersForChecking[i]
                                });
                                counterIdLines++;
                                $scope.lines.push({
                                    id: counterIdLines,
                                    Value: $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .Transformation,
                                    arrowType: j,
                                    iterator: 0,
                                    x1: $scope.counterIteration - 0.5,
                                    y1: cyline + shift * lineIterator,
                                    x2: $scope.counterIteration,
                                    y2: $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .OrderId,
                                    startOrderId: ordersForChecking[i]
                                });
                                counterIdLines++;
                            } else {
                                $scope.lines.push({
                                    id: counterIdLines,
                                    Value: $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .Transformation,
                                    arrowType: j,
                                    iterator: 0,
                                    x1: $scope.counterIteration - 1,
                                    y1: ordersForChecking[i],
                                    x2: $scope.counterIteration,
                                    y2: $scope.transformationsData[ordersForChecking[i] - 1].ResultTransformation[j]
                                        .OrderId,
                                    startOrderId: ordersForChecking[i]
                                });
                                counterIdLines++;
                            }
                        }
                    }
                }
                ordersForChecking = newOrdersForChecking;
                $scope.counterIteration++;
            }
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            tooltipContent.push("Order ID: " + d.Value);
            for (var i = 0; i < $scope.orders.length; i++) {
                if (d.Value === $scope.orders[i].OrderId) {
                    tooltipContent.push("Order: " + $scope.orders[i].Order);
                    break;
                }
            }
            return tooltipContent.join("</br>");
        }

        // shows tooltip for dot or group of dots
        function showTooltip(d, tooltip, newSelectedDot, svg) {
            $scope.clearTooltip(tooltip);
            var color = d3.scaleOrdinal(d3.schemeCategory10);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(function (dot) {
                    if (dot.x === d.x && dot.y === d.y) {
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    } else {
                        return false;
                    }
                })
                .attr("rx", $scope.selectedDotRadius)
                .attr("ry", $scope.selectedDotRadius);

            tooltip.html(tooltipHtml.join("</br></br>"));

            tooltip.append("br");
            tooltip.append("div")
                .append("svg")
                .attr("height", $scope.legend.length * 20)
                .attr("width", 20)
                .selectAll(".dotlegend")
                .data(d.visibilityTranformation)
                .enter()
                .append("g")
                .attr("class", "dotlegend")
                .attr("transform", function (vt, i) { return "translate(0," + i * 20 + ")"; })
                .append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function (vt) { return color(vt.name); })
                .style("stroke", function (vt) { return color(vt.name); })
                .style("stroke-width", 4)
                .style("fill-opacity", function (vt) { return vt.visible ? 1 : 0; })
                .on("click", function (vt) {
                    vt.visible = !vt.visible;
                    d3.select(this).style("fill-opacity", function () { return vt.visible ? 1 : 0; });
                    svg.selectAll(".line")
                        .filter(function (line) {
                            return line.startOrderId === d.Value && line.Value === vt.name;
                        })
                        .attr("visibility", function (line) {
                            return vt.visible ? "visible" : "hidden";
                        });
                });
            

            tooltip.style("background", "#eee")
                .style("color", "#000")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (d3.event.pageX + 10) + "px")
                .style("top", (d3.event.pageY - 8) + "px");

            tooltip.hideTooltip = false;
        }

        // clears tooltip and unselects dots
        function clearTooltip(tooltip) {
            if (tooltip) {
                if (tooltip.hideTooltip) {
                    tooltip.html("").style("opacity", 0);

                    if (tooltip.selectedDots) {
                        tooltip.selectedDots.attr("rx", $scope.dotRadius)
                            .attr("ry", $scope.dotRadius);
                    }
                }

                tooltip.hideTooltip = true;
            }
        }

        function xValue(d) {
            return d.x;
        }

        function yValue(d) {
            return d.y;
        }

        function draw() {
            $scope.fillPointsAndLines();

            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
            d3.select(".chart-svg").remove();

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.height - margin.top - margin.bottom;

            // setup x
            // calculating margins for dots
            var xMin = d3.min($scope.points, $scope.xValue);
            var xMax = d3.max($scope.points, $scope.xValue);
            var xMargin = (xMax - xMin) * 0.05;

            var xScale = d3.scaleLinear()
                .domain([xMin - xMargin, xMax + xMargin])
                .range([0, width]);
            var xAxis = d3.axisBottom(xScale)
                .ticks($scope.counterIteration)
                .tickSizeInner(-height)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.xMap = function (d) { return xScale($scope.xValue(d)); };

            // setup y
            // calculating margins for dots
            var yMax = d3.max($scope.points, $scope.yValue);
            var yMin = d3.min($scope.points, $scope.yValue);
            var yMargin = (yMax - yMin) * 0.05;

            var yScale = d3.scaleLinear()
                .domain([yMin - yMargin, yMax + yMargin])
                .range([height, 0]);
            var yAxis = d3.axisLeft(yScale)
                .ticks($scope.orders.length > 16 ? ($scope.orders.length + 1) / 10 : ($scope.orders.length + 1))
                .tickSizeInner(-width)
                .tickSizeOuter(0)
                .tickPadding(10);

            $scope.yMap = function (d) { return yScale($scope.yValue(d)); };

            // setup fill color
            var cValue = function (d) { return d.Value; };
            var color = d3.scaleOrdinal(d3.schemeCategory10);

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.height)
                .attr("class", "chart-svg");

            var g = svg.append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add defs for lines ends
            var defs = svg.append("defs");
            for (var i = 0; i < $scope.legend.length; i++) {
                defs.append("marker")
                    .attr("id", "arrow" + i)
                    .attr("viewBox", "0 -5 10 10")
                    .attr("refX", 6)
                    .attr("refY", 0)
                    .attr("markerWidth", 6)
                    .attr("markerHeight", 6)
                    .attr("orient", "auto")
                    .append("path")
                    .attr("d", "M0,-5L10,0L0,5")
                    .attr("stroke", color($scope.legend[i].name))
                    .attr("fill", color($scope.legend[i].name));
            }

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
                .style("opacity", 0);

            // preventing tooltip hiding if dot clicked
            tooltip.on("click", function () { tooltip.hideTooltip = false; });

            // hiding tooltip
            d3.select("#chart").on("click", function () { $scope.clearTooltip(tooltip); });

            // x-axis
            g.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis);

            g.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + (width / 2) + " ," + (height + margin.top - $scope.legendHeight) + ")")
                .style("text-anchor", "middle")
                .text("Iteration")
                .style("font-size", "12pt");

            // y-axis
            g.append("g")
                .attr("class", "y axis")
                .call(yAxis);

            g.append("text")
                .attr("class", "label")
                .attr("transform", "rotate(-90)")
                .attr("y", 0 - margin.left)
                .attr("x", 0 - (height / 2))
                .attr("dy", ".71em")
                .style("text-anchor", "middle")
                .text("Order Id")
                .style("font-size", "12pt");

            // draw lines
            g.selectAll(".line")
                .data($scope.lines)
                .enter()
                .append("line")
                .attr("class", "line")
                .attr("x1", function (d) { return xScale(d.x1); })
                .attr("y1", function (d) { return yScale(d.y1); })
                .attr("x2", function (d) { return xScale(d.x2); })
                .attr("y2", function (d) { return yScale(d.y2); })
                .attr("marker-end", function (d) { return "url(#arrow" + d.arrowType + ")"; })
                .style("stroke", function (d) { return color(d.Value); })
                .style("stroke-width", "2")
                .attr("visibility", "visible");

            // draw legend
            var legend = g.selectAll(".legend")
                .data($scope.legend)
                .enter()
                .append("g")
                .attr("class", "legend")
                .attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; })
                .on("click", function (d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", function () { return d.visible ? 1 : 0.5; });
                    legendEntry.select("rect")
                        .style("fill-opacity", function () { return d.visible ? 1 : 0; });

                    svg.selectAll(".line")
                        .filter(function (line) { return line.Value === d.name; })
                        .attr("visibility", function (line) {
                            return d.visible ? "visible" : "hidden";
                        });
                    for (var k = 0; k < $scope.points.length; k++) {
                        for (var j = 0; j < $scope.legend.length; j++) {
                            if ($scope.points[k].visibilityTranformation[j].name === d.name) {
                                $scope.points[k].visibilityTranformation[j].visible = d.visible;
                            }
                        }
                    }
                });

            // draw dots
            g.selectAll(".dot")
                .data($scope.points)
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", $scope.xMap)
                .attr("cy", $scope.yMap)
                .style("fill-opacity", 0.6)
                .style("fill", "black")
                .style("stroke", "black")
                .on("click", function (d) { return $scope.showTooltip(d, tooltip, d3.select(this), g); });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function (d) { return color(d.name); })
                .style("stroke", function (d) { return color(d.name); })
                .style("stroke-width", 4)
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .text(function (d) { return d.name; })
                .style("font-size", "9pt");
        }

        $scope.draw = draw;
        $scope.fillPointsAndLines = fillPointsAndLines;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.yValue = yValue;
        $scope.xValue = xValue;

        $scope.width = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 2;

        $scope.fillLegend = fillLegend;

        $scope.loadingScreenHeader = "Loading data";

        var location = window.location.href.split("/");
        $scope.taskId = location[location.length - 1];

        $scope.loading = true;

        $http.get("/api/TaskManagerWebApi/" + $scope.taskId)
            .then(function (data) {
                MapModelFromJson($scope, JSON.parse(data.data));

                $scope.countOfUniqueFinalOrders = [];
                for (var i = 0; i < $scope.transformationsData.length; i++) {
                    $scope.countOfUniqueFinalOrders.push($scope.transformationsData[i].UniqueFinalOrders.length);
                }

                $scope.fillLegend();

                $scope.legendHeight = $scope.legend.length * 20;
                $scope.height = 800 + $scope.legendHeight;

                console.log($scope);

                $scope.loading = false;
            }, function () {
                alert("Failed loading characteristic data");
                $scope.loading = false;
            });
    }

    angular.module("libiada").controller("OrderTransformationVisualizationResultCtrl", ["$scope", "$http", orderTransformationVisualizationResult]);

}