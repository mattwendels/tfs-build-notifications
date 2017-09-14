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

var dashboardLib = function (options) {

    var runningBuildsCount = 0;

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

        // On disconnect.
        $.connection.hub.disconnected(function () {
            console.log('SignalR disconnected.');
            setTimeout(function () {
                alert('SignalR Reconnecting in 5 seconds...')
                $.connection.hub.start();
            }, 5000); // Restart connection after 5 seconds.
        });

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

                        if (!buildAlreadyMarkedAsRunning($(data))) {

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

                        console.log('Build completed. Local definition ID: ' + buildNotification.DefinitionLocalId);

                        // Remove running now panel and replace with updated build result.
                        var runningBuildPanel = options.buildsRunningContainer.find('[data-local-id="' + buildNotification.DefinitionLocalId + '"]');
                        var completedBuild = $(data);

                        if (runningBuildPanel.length <= 0) {
                            console.log('Could not find running build panel for local definition ID:' + buildNotification.DefinitionLocalId);
                            console.log('Build data:');
                            console.log(completedBuild);
                        }

                        completedBuild.find(options.removeBuildClass).remove();

                        runningBuildPanel.replaceWith(completedBuild);

                        // Display result for 5 seconds then remove.
                        setTimeout(function () {

                            completedBuild.fadeOut(400, function () {
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

    var buildAlreadyMarkedAsRunning = function (buildPanel) {
        return options.buildsRunningContainer.has(buildPanel).length > 0;
    };

    init();
}