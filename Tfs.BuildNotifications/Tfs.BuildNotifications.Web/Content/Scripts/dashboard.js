// Quick highlight and fade...
jQuery.fn.highlight = function () {

    $(this).each(function () {
        var el = $(this);
        $("<div/>")
            .width(el.outerWidth())
            .height(el.outerHeight())
            .css({
                "position": "absolute",
                "left": el.offset().left,
                "top": el.offset().top,
                "background-color": "#ffff99",
                "opacity": ".7",
                "z-index": "9999999"
            }).appendTo('body').fadeOut(2000).queue(function () { $(this).remove(); });
    });
}

var dashboardHubProxy = $.connection.dashboardHub;
var runningBuildsCount = 0;

var dashboardLib = function (options) {

    var init = function () {

        // Bind server calls.
        dashboardHubProxy.client.buildNotification = function (data) {
            updateUiForBuildChange(data);
        };

        $.connection.hub.start()
            .done(function () {
                console.log('SignalR connected. Connection ID=' + $.connection.hub.id);

                // Get a list of running builds and update UI (if builds are running during dashboard load).
                dashboardHubProxy.server.getRunningBuilds($.connection.hub.id);
            })
            .fail(function () { console.log('SignalR failed to connect.'); });

        function updateUiForBuildChange(data) {

            var buildNotification = $.parseJSON(data);
            var dashboardBuild = $('*[data-local-id="' + buildNotification.DefinitionLocalId + '"]').not('*[data-build-running]', 'true');
            var connectionId = dashboardBuild.attr('data-connection-id');
            var projectId = dashboardBuild.attr('data-project-id');

            // Update dashboard definition (to set as now running etc.)
            $.ajax({
                type: 'get',
                url: '/buildsummary',
                data: {
                    connectionId: connectionId,
                    projectId: projectId,
                    localBuildId: buildNotification.DefinitionLocalId
                },
                success: function (data) {

                    var updatedBuild = ($(data));

                    dashboardBuild.replaceWith(updatedBuild);

                    // Add to 'Running Now' section.
                    if (buildNotification.InProgress) {

                        if (!buildAlreadyMarkedAsRunning(buildNotification)) {

                            runningBuildsCount++;

                            var runningNow = $(data);

                            runningNow.hide();
                            runningNow.attr('data-build-running', 'true');
                            runningNow.find(options.removeBuildClass).remove();

                            options.buildsRunningContainer.append(runningNow);

                            runningNow.fadeIn(400, function () {
                                checkAnyBuildsRunning();
                                setTimeout(runningNow.highlight(), 1000);

                            });
                        }
                    }
                    else {

                        runningBuildsCount--;

                        // Remove running now panel and replace with updated build result.
                        var runningBuildPanel = options.buildsRunningContainer
                            .find('[data-local-id="' + buildNotification.DefinitionLocalId + '"][data-last-run-id="' + buildNotification.BuildRunId + '"]');

                        var completedBuild = $(data);

                        completedBuild.find(options.removeBuildClass).remove();

                        runningBuildPanel.replaceWith(completedBuild);

                        var delayPanelRemove = options.buildsRunningContainer
                            .find('[data-local-id="' + buildNotification.DefinitionLocalId + '"][data-last-run-id="' + buildNotification.BuildRunId + '"]');

                        // Display result for 5 seconds then remove.
                        setTimeout(function () {

                            delayPanelRemove.fadeOut(400, function () {
                                checkAnyBuildsRunning();
                            });
                        }, 5000);
                    }
                }
            });
        }
    };

    var checkAnyBuildsRunning = function () {

        if (runningBuildsCount > 0) {
            options.noBuildsRunningMessage.hide();
        }
        else {
            options.noBuildsRunningMessage.show();
        }
    };

    var buildAlreadyMarkedAsRunning = function (buildNotification) {

        return result = options.buildsRunningContainer
            .find('*[data-local-id="' + buildNotification.DefinitionLocalId + '"][data-last-run-id="' + buildNotification.BuildRunId + '"]')
            .length > 0;
    };

    init();
}