﻿function SubsequencesDistributionResultController(data) {
    "use strict";

    function subsequencesDistributionResult($scope) {
        MapModelFromJson($scope, data);

        function fillPoints() {
            var id = 0;
            for (var i = 0; i < $scope.result.length; i++) {
                var sequenceData = $scope.result[i];
                $scope.matters.push({ id: sequenceData.MatterId, name: sequenceData.MatterName });

                for (var j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    var subsequenceData = sequenceData.SubsequencesData[j];
                    $scope.points.push({
                        id: id,
                        matterId: sequenceData.MatterId,
                        name: sequenceData.MatterName,
                        sequenceWebApiId: sequenceData.WebApiId,
                        attributes: subsequenceData.Attributes,
                        featureId:subsequenceData.FeatureId,
                        featureName: subsequenceData.FeatureName,
                        positions: subsequenceData.Starts,
                        lengths: subsequenceData.Lengths,
                        subsequenceWebApiId: subsequenceData.WebApiId,
                        numericX: i + 1,
                        x: sequenceData.Characteristic,
                        y: subsequenceData.Characteristic,
                        featureVisible: true,
                        matterVisible: true
                    });

                    id++;
                }
            }
        }

        function filterByFeature(feature) {
            d3.selectAll(".dot")
                .filter(function (dot) { return dot.featureId === feature.Value })
                .attr("r", function (d) {
                    d.featureVisible = feature.Selected;
                    return $scope.pointVisible(d) ? $scope.pointRadius : 0;
                });
        }

        function pointVisible(point) {
            return point.featureVisible && point.matterVisible;
        }

        function drawGenesMap() {
            //removing previous chart is any
            d3.select("svg").remove();

            // all organisms are visible after redrawing
            $scope.points.forEach(function (point) {
                point.matterVisible = true;
                for (var i = 0; i < $scope.features.length; i++) {
                    if ($scope.features[i].Value === point.featureId) {
                        point.featureVisible = $scope.features[i].Selected;
                        break;
                    }
                }
            });

            // chart size and margin settings
            var margin = { top: 30 + $scope.legendHeight, right: 30, bottom: 30, left: 60 };
            var width = $scope.width - margin.left - margin.right;
            var height = $scope.hight - margin.top - margin.bottom;

            // setup x 
            var xValue = function (d) { return $scope.numericXAxis ? d.numericX : d.x; }; // data -> value
            var xScale = d3.scale.linear().range([0, width]); // value -> display
            var xMap = function (d) { return xScale(xValue(d)); }; // data -> display
            var xAxis = d3.svg.axis().scale(xScale).orient("bottom");
            xAxis.innerTickSize(-height).outerTickSize(0).tickPadding(10);

            // setup y
            var yValue = function (d) { return d.y; }; // data -> value
            var yScale = d3.scale.linear().range([height, 0]); // value -> display
            var yMap = function (d) { return yScale(yValue(d)); }; // data -> display
            var yAxis = d3.svg.axis().scale(yScale).orient("left");
            yAxis.innerTickSize(-width).outerTickSize(0).tickPadding(10);

            // setup fill color
            var cValue = function (d) { return d.matterId; };
            var color = d3.scale.category20();

            // add the graph canvas to the body of the webpage
            var svg = d3.select("#chart").append("svg")
                .attr("width", $scope.width)
                .attr("height", $scope.hight)
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // add the tooltip area to the webpage
            var tooltip = d3.select("#chart").append("div")
                .attr("class", "tooltip")
                .style("opacity", 0);

            // tooltip drawing method
            var showTooltip = function (d) {
                tooltip.style("opacity", .9);

                var genBank = "http://www.ncbi.nlm.nih.gov/nuccore/";

                var tooltipHeader = d.sequenceWebApiId ?
                                    "<a target='_blank' href='" + genBank + d.sequenceWebApiId + "'>" + d.name + "</a>" :
                                    d.name;

                var peptideGenbankLink = d.subsequenceWebApiId ?
                                         "<a target='_blank' href='" + genBank + d.subsequenceWebApiId + "'>Peptide ncbi page</a><br/>" : "";

                var start = d.positions[0] + 1;
                var end = d.positions[0] + d.lengths[0];
                var positionGenbankLink = d.sequenceWebApiId ?
                                          "<a target='_blank' href='" + genBank + d.sequenceWebApiId + "?from=" + start + "&to=" + end + "'>" + d.positions.join(", ") + "</a>" :
                                          d.positions.join(", ");

                tooltip.html(tooltipHeader + "<br/>"
                           + peptideGenbankLink
                           + d.featureName + "<br/>"
                           + d.attributes.join("<br/>") + "<br/>"
                           + "Position: " + positionGenbankLink + "<br/>"
                           + " (" + d.x + ", " + d.y + ")")
                    .style("background", "#000")
                    .style("color", "#fff")
                    .style("border-radius", "5px")
                    .style("font-family", "monospace")
                    .style("padding", "5px")
                    .style("left", (d3.event.pageX + 5) + "px")
                    .style("top", (d3.event.pageY - 28) + "px");

                tooltip.hideTooltip = false;
            }

            // preventing tooltip hiding
            tooltip.on("click", function (d) {
                tooltip.hideTooltip = false;
            });

            // hiding tooltip
            d3.select("#chart").on("click", function (d) {
                if (tooltip.hideTooltip) {
                    tooltip.html("").style("opacity", 0);
                }

                tooltip.hideTooltip = true;
            });

            // calculating margins for dots
            var xMin = d3.min($scope.points, xValue);
            var xMax = d3.max($scope.points, xValue);
            var xMargin = (xMax - xMin) * 0.05;
            var yMax = d3.max($scope.points, yValue);
            var yMin = d3.min($scope.points, yValue);
            var yMargin = (yMax - yMin) * 0.05;

            // don't want dots overlapping axis, so add in buffer to data domain
            xScale.domain([xMin - xMargin, xMax + xMargin]);
            yScale.domain([yMin - yMargin, yMax + yMargin]);

            // x-axis
            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + height + ")")
                .call(xAxis)
                .append("text")
                .attr("class", "label")
                .attr("x", width)
                .attr("y", -6)
                .style("text-anchor", "end")
                .text($scope.sequenceCharacteristicName)
                .style("font-size", "12pt");

            // y-axis
            svg.append("g")
                .attr("class", "y axis")
                .call(yAxis)
                .append("text")
                .attr("class", "label")
                .attr("transform", "rotate(-90)")
                .attr("y", 6)
                .attr("dy", ".71em")
                .style("text-anchor", "end")
                .text($scope.subsequencesCharacteristicName)
                .style("font-size", "12pt");

            // draw dots
            svg.selectAll(".dot")
                .data($scope.points)
                .enter()
                .append("circle")
                .attr("class", "dot")
                .attr("r", function (d) { return pointVisible(d) ? $scope.pointRadius : 0; })
                .attr("cx", xMap)
                .attr("cy", yMap)
                .style("fill", function (d) { return color(cValue(d)); })
                .on("click", showTooltip);

            // draw legend
            var legend = svg.selectAll(".legend")
                .data($scope.matters)
                .enter().append("g")
                .attr("class", "legend")
                .attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; })
                .on("click", function (d) {
                    var legendEntry = d3.select(this);
                    legendEntry.style("opacity", function (d) { return legendEntry.style("opacity") == 1 ? .5 : 1; });

                    svg.selectAll(".dot")
                        .filter(function (dot) { return dot.matterId === d.id })
                        .attr("r", function (d) {
                            d.matterVisible = legendEntry.style("opacity") == 1;
                            return $scope.pointVisible(d) ? $scope.pointRadius : 0;
                        });
                });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("x", width - 18)
                .attr("width", 18)
                .attr("height", 18)
                .style("fill", function (d) { return color(d.id); })
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")");

            // draw legend text
            legend.append("text")
                .attr("x", width - 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .attr("transform", "translate(0, -" + $scope.legendHeight + ")")
                .style("text-anchor", "end")
                .text(function (d) { return d.name; })
                .style("font-size", "9pt");
        }

        $scope.drawGenesMap = drawGenesMap;
        $scope.pointVisible = pointVisible;
        $scope.filterByFeature = filterByFeature;
        $scope.fillPoints = fillPoints;

        $scope.legendHeight = $scope.result.length * 20;
        $scope.hight = 800 + $scope.legendHeight;
        $scope.width = 800;

        $scope.pointRadius = 3.5;
        $scope.points = [];
        $scope.matters = [];
        $scope.fillPoints();

    }

    angular.module("SubsequencesDistributionResult", []).controller("SubsequencesDistributionResultCtrl", ["$scope", subsequencesDistributionResult]);
}