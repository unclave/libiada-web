﻿function AccordanceController(data) {
    "use strict";

    function accordance($scope, filterFilter) {
        MapModelFromJson($scope, data);

        $scope.selectedMatters = 0;

        $scope.matterCheckChanged = function (matter) {
            if (matter.Selected) {
                $scope.selectedMatters++;
            } else {
                $scope.selectedMatters--;
            }
        };

        $scope.disableMattersSelect = function (matter) {
            return ($scope.selectedMatters === $scope.maximumSelectedMatters) && !matter.Selected;
        };

        $scope.disableSubmit = function () {
            return $scope.selectedMatters < $scope.minimumSelectedMatters;
        };

        $scope.notationsFiltered = [];

        $scope.natureId = $scope.natures[0].Value;

        var filterByNature = function () {
            FilterOptionsByNature($scope, filterFilter, "notations");
            var notation = $scope.notationsFiltered[0];
            $scope.characteristic.notation = notation;
        };

        $scope.isLinkable = function (characteristic) {
            return characteristic.characteristicType.CharacteristicTypeLink.length > 1;
        };

        $scope.getLinks = function (characteristicType) {
            var characteristicTypeLinks = characteristicType.CharacteristicTypeLink;
            var links = [];
            for (var i = 0; i < characteristicTypeLinks.length; i++) {
                for (var j = 0; j < $scope.links.length; j++) {
                    if ($scope.links[j].CharacteristicTypeLink.indexOf(characteristicTypeLinks[i]) !== -1) {
                        links.push($scope.links[j]);
                    }
                }
            }

            return links;
        };

        $scope.characteristic = {
            characteristicType: $scope.characteristicTypes[0],
            link: $scope.getLinks($scope.characteristicTypes[0])[0],
            notation: $scope.notationsFiltered[0]
        };

        $scope.$watch("natureId", filterByNature, true);
    }

    angular.module("Accordance", []).controller("AccordanceCtrl", ["$scope", "filterFilter", accordance]);
}
