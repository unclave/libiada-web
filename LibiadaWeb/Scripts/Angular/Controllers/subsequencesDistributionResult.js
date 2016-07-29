﻿function SubsequencesDistributionResultController(data) {
    "use strict";

    function subsequencesDistributionResult($scope) {
        MapModelFromJson($scope, data);

        function addCharacteristicComparer() {
            $scope.characteristicComparers.push({ characteristic: $scope.subsequencesCharacteristicsList[0], precision: 0 });
        }

        function deleteCharacteristicComparer(characteristicComparer) {
            $scope.characteristicComparers.splice($scope.characteristicComparers.indexOf(characteristicComparer), 1);
        }

        // initializes data for genes map 
        function fillPoints() {
            var id = 0;
            for (var i = 0; i < $scope.result.length; i++) {
                var sequenceData = $scope.result[i];
                $scope.matters.push({ id: sequenceData.MatterId, name: sequenceData.MatterName, visible: true });

                for (var j = 0; j < sequenceData.SubsequencesData.length; j++) {
                    var subsequenceData = sequenceData.SubsequencesData[j];
                    var point = {
                        id: id,
                        matterId: sequenceData.MatterId,
                        name: sequenceData.MatterName,
                        sequenceRemoteId: sequenceData.RemoteId,
                        attributes: subsequenceData.Attributes,
                        partial: subsequenceData.partial,
                        featureId: subsequenceData.FeatureId,
                        positions: subsequenceData.Starts,
                        lengths: subsequenceData.Lengths,
                        subsequenceRemoteId: subsequenceData.RemoteId,
                        numericX: i + 1,
                        x: sequenceData.Characteristic,
                        subsequenceCharacteristics: subsequenceData.CharacteristicsValues,
                        featureVisible: true,
                        matterVisible: true
                    };
                    $scope.points.push(point);
                    $scope.visiblePoints.push(point);
                    id++;
                }
            }
        }

        // filters dots by subsequences feature
        function filterByFeature(feature) {
            d3.selectAll(".dot")
                .filter(function (dot) { return dot.featureId === feature.Value; })
                .attr("visibility", function (d) {
                    d.featureVisible = feature.Selected;
                    return $scope.dotVisible(d) ? "visible" : "hidden";
                });

            if (feature.Selected) { // adding new visible poins
                for (var i = 0; i < $scope.points.length; i++) {
                    if ($scope.points[i].featureId === feature.Value) {
                        $scope.visiblePoints.push($scope.points[i]);
                    }
                }
            } else { // removing not visible points
                for (var j = 0; j < $scope.visiblePoints.length; j++) {
                    if ($scope.visiblePoints[j].featureId === feature.Value) {
                        $scope.visiblePoints.splice($scope.visiblePoints.indexOf($scope.visiblePoints[j]), 1);
                        j--;
                    }
                }
            }
        }

        // checks if dot is visible
        function dotVisible(dot) {
            return dot.featureVisible && dot.matterVisible;
        }

        function dotsSimilar(d, dot) {
            if (d.featureId !== dot.featureId) {
                return false;
            }

            switch(d.featureId) {
                case 4: // CDS
                case 5: // RRNA
                case 6: // TRNA
                    if (d.attributes["product"] !== dot.attributes["product"]) {
                        return false;
                    }
                    break;
            }

            return true;
            
        }

        // shows tooltip for dot or group of dots
        function showTooltip(d, tooltip, svg) {
            $scope.clearTooltip(tooltip);

            tooltip.style("opacity", 0.9);

            var tooltipHtml = [];

            tooltip.selectedPoint = d;
            tooltip.selectedDots = svg.selectAll(".dot")
                .filter(function (dot) {
                    if (dot.matterId === d.matterId && yValue(dot) === yValue(d)) { // if dots are in the same position
                        tooltipHtml.push($scope.fillPointTooltip(dot));
                        return true;
                    } else if ($scope.highlight) { // if similar dot are highlighted
                        for (var i = 0; i < $scope.characteristicComparers.length; i++) {
                            var dotValue = dot.subsequenceCharacteristics[$scope.characteristicComparers[i].characteristic.Value];
                            var dValue = d.subsequenceCharacteristics[$scope.characteristicComparers[i].characteristic.Value];
                            if (Math.abs(dotValue - dValue) > $scope.characteristicComparers[i].precision) { // if dValue is out of range for any comparer
                                return false;
                            }
                        }

                        var tooltipColor = $scope.dotsSimilar(d, dot) ? "text-success" : "text-danger";

                        tooltipHtml.push("<span class='" + tooltipColor + "'>" + $scope.fillPointTooltip(dot) + "</span>");

                        return true;
                    }

                    return false;
                })
                .attr("rx", $scope.selectedDotRadius);

            tooltip.html(tooltipHtml.join("</br></br>"));

            tooltip.style("background", "#000")
                .style("color", "#fff")
                .style("border-radius", "5px")
                .style("font-family", "monospace")
                .style("padding", "5px")
                .style("left", (d3.event.pageX + 18) + "px")
                .style("top", (d3.event.pageY + 18) + "px");
        }

        // constructs string representing tooltip text (inner html)
        function fillPointTooltip(d) {
            var tooltipContent = [];
            var genBankLink = "<a target='_blank' href='https://www.ncbi.nlm.nih.gov/nuccore/";

            var header = d.sequenceRemoteId ? genBankLink + d.sequenceRemoteId + "'>" + d.name + "</a>" : d.name;
            tooltipContent.push(header);

            if (d.subsequenceRemoteId) {
                var peptideGenbankLink = genBankLink + d.subsequenceRemoteId + "'>Peptide ncbi page</a>";
                tooltipContent.push(peptideGenbankLink);
            }

            tooltipContent.push($scope.featuresNames[d.featureId]);

            var attributes = [];

            for (var key in d.attributes) {
                attributes.push(key + (d.attributes[key] === "" ? "" : " = " + d.attributes[key]));
            }

            tooltipContent.push(attributes.join("<br/>"));

            if (d.partial) {
                tooltipContent.push("partial");
            }

            var start = d.positions[0] + 1;
            var end = d.positions[0] + d.lengths[0];
            var positionGenbankLink = d.sequenceRemoteId ?
                                      genBankLink + d.sequenceRemoteId + "?from=" + start + "&to=" + end + "'>" + d.positions.join(", ") + "</a>" :
                                      d.positions.join(", ");
            tooltipContent.push("Position: " + positionGenbankLink);
            tooltipContent.push("Length: " + d.lengths.join(", "));
            tooltipContent.push("(" + d.x + ", " + yValue(d) + ")");

            return tooltipContent.join("</br>");
        }

        function clearTooltip(tooltip) {
            if (tooltip) {
                tooltip.html("").style("opacity", 0);

                if (tooltip.selectedDots) {
                    tooltip.selectedDots.attr("rx", $scope.dotRadius);
                }
            }
        }

        function isKeyUpOrDown(keyCode) {
            return keyCode === 40 || keyCode === 38;
        }

        function xValue(d) {
            return $scope.numericXAxis ? d.numericX : d.x;
        }

        function yValue(d) {
            return d.subsequenceCharacteristics[$scope.subsequenceCharacteristic.Value];
        }

        // main drawing method
        function drawGenesMap() {
            // removing previous chart and tooltip if any
            d3.select(".tooltip").remove();
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
            var xScale = d3.scale.linear().range([0, width]); // value -> display
            var xMap = function (d) { return xScale($scope.xValue(d)); }; // data -> display
            var xAxis = d3.svg.axis().scale(xScale).orient("bottom");
            xAxis.innerTickSize(-height).outerTickSize(0).tickPadding(10);

            // setup y
            var yScale = d3.scale.linear().range([height, 0]); // value -> display
            var yMap = function (d) { return yScale($scope.yValue(d)); }; // data -> display
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


            // calculating margins for dots
            var xMin = d3.min($scope.points, $scope.xValue);
            var xMax = d3.max($scope.points, $scope.xValue);
            var xMargin = (xMax - xMin) * 0.05;
            var yMax = d3.max($scope.points, $scope.yValue);
            var yMin = d3.min($scope.points, $scope.yValue);
            var yMargin = (yMax - yMin) * 0.05;

            // don't want dots overlapping axis, so adding buffer to data domain
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
                .text($scope.subsequenceCharacteristic.Text)
                .style("font-size", "12pt");

            // draw dots
            svg.selectAll(".dot")
                .data($scope.points)
                .enter()
                .append("ellipse")
                .attr("class", "dot")
                .attr("rx", $scope.dotRadius)
                .attr("ry", $scope.dotRadius)
                .attr("cx", xMap)
                .attr("cy", yMap)
                .style("fill-opacity", 0.6)
                .style("fill", function (d) { return color(cValue(d)); })
                .style("stroke", function (d) { return color(cValue(d)); })
                .attr("visibility", function (dot) {
                    return $scope.dotVisible(dot) ? "visible" : "hidden";
                });

            // draw legend
            var legend = svg.selectAll(".legend")
                .data($scope.matters)
                .enter().append("g")
                .attr("class", "legend")
                .attr("transform", function (d, i) { return "translate(0," + i * 20 + ")"; })
                .on("click", function (d) {
                    d.visible = !d.visible;
                    var legendEntry = d3.select(this);
                    legendEntry.select("text")
                        .style("opacity", function () { return d.visible ? 1 : 0.5; });
                    legendEntry.select("rect")
                        .style("fill-opacity", function () { return d.visible ? 1 : 0; });

                    svg.selectAll(".dot")
                        .filter(function (dot) { return dot.matterId === d.id; })
                        .attr("visibility", function (dot) {
                            dot.matterVisible = d.visible;
                            return $scope.dotVisible(dot) ? "visible" : "hidden";
                        });
                });

            // draw legend colored rectangles
            legend.append("rect")
                .attr("width", 15)
                .attr("height", 15)
                .style("fill", function (d) { return color(d.id); })
                .style("stroke", function (d) { return color(d.id); })
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

            // tooltip event bind
            d3.select("body").on("click", function () {
                var clickedDots = svg.selectAll(".dot").filter(function () {
                    return this === d3.event.target;
                });

                if (clickedDots.empty()) {
                    $scope.clearTooltip(tooltip);
                } else {
                    var points = clickedDots.data();
                    $scope.showTooltip(points[0], tooltip, svg);
                }
            });

            // tooltip show on key up or key down
            d3.select("body")
                .on("keydown", function () {
                    if (tooltip.selectedPoint) {
                        var keyCode = d3.event.keyCode;
                        if (isKeyUpOrDown(keyCode)) {
                            var nextPoint;
                            var indexOfPoint = $scope.visiblePoints.indexOf(tooltip.selectedPoint);
                            $scope.clearTooltip(tooltip);

                            switch (keyCode) {
                                case 40: // down
                                    for (var i = indexOfPoint + 1; i < $scope.visiblePoints.length; i++) {
                                        if ($scope.visiblePoints[i].matterId === tooltip.selectedPoint.matterId) {
                                            nextPoint = $scope.visiblePoints[i];
                                            break;
                                        }
                                    }
                                    break;
                                case 38: // up
                                    for (var j = indexOfPoint - 1; j >= 0; j--) {
                                        if ($scope.visiblePoints[j].matterId === tooltip.selectedPoint.matterId) {
                                            nextPoint = $scope.visiblePoints[j];
                                            break;
                                        }
                                    }
                                    break;
                            }

                            if (nextPoint) {
                                return $scope.showTooltip(nextPoint, tooltip, svg);
                            }
                        }
                    }
                });

            // preventing scroll in key up and key down
            window.addEventListener("keydown", function (e) {
                if (isKeyUpOrDown(e.keyCode)) {
                    e.preventDefault();
                }
            }, false);
        }

        $scope.setCheckBoxesState = SetCheckBoxesState;

        $scope.drawGenesMap = drawGenesMap;
        $scope.dotVisible = dotVisible;
        $scope.dotsSimilar = dotsSimilar;
        $scope.filterByFeature = filterByFeature;
        $scope.fillPoints = fillPoints;
        $scope.fillPointTooltip = fillPointTooltip;
        $scope.showTooltip = showTooltip;
        $scope.clearTooltip = clearTooltip;
        $scope.isKeyUpOrDown = isKeyUpOrDown;
        $scope.xValue = xValue;
        $scope.yValue = yValue;
        $scope.addCharacteristicComparer = addCharacteristicComparer;
        $scope.deleteCharacteristicComparer = deleteCharacteristicComparer;

        $scope.legendHeight = $scope.result.length * 20;
        $scope.hight = 800 + $scope.legendHeight;
        $scope.width = 800;
        $scope.dotRadius = 4;
        $scope.selectedDotRadius = $scope.dotRadius * 3;
        $scope.points = [];
        $scope.visiblePoints = [];
        $scope.matters = [];
        $scope.subsequenceCharacteristic = $scope.subsequencesCharacteristicsList[0];
        $scope.characteristicComparers = [];
        $scope.fillPoints();
    }

    angular.module("SubsequencesDistributionResult", []).controller("SubsequencesDistributionResultCtrl", ["$scope", subsequencesDistributionResult]);
}