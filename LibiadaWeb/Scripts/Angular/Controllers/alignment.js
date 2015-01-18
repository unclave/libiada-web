﻿"use strict";

var app = angular.module("Alignment", []);

function Alignment ($scope) {
    MapModelFromJson($scope, data);

    $scope.matterSelectionLimit = 2;
    $scope.selectedMatters = 0;

    $scope.matterCheckChanged = function(matter){
        if (matter.Selected) {
            $scope.selectedMatters++;
        } else {
            $scope.selectedMatters--;
        }
    }

    $scope.disableMattersSelect = function(matter) {
        return $scope.selectedMatters == $scope.matterSelectionLimit && !matter.Selected;
    }

    $scope.characteristic = {
        characteristicType: $scope.characteristicTypes[0],
        link: $scope.links[0],
        notation: $scope.notationsFiltered[0]
    };

    $scope.isLinkable = function(characteristic) {
        return characteristic.characteristicType.Linkable;
    };
}

angular.module("Alignment", []).controller("AlignmentCtrl", ["$scope", Alignment]);
