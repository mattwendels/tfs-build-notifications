using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using Tfs.BuildNotifications.Core.Exceptions;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Model;
using Tfs.BuildNotifications.Web.ViewModels;

namespace Tfs.BuildNotifications.Web.Nancy.Modules
{
    public class ConnectionModule : NancyModule
    {
        private readonly IBuildConfigurationService _buildConfigurationService;

        public ConnectionModule(IBuildConfigurationService buildConfigurationService)
        {
            _buildConfigurationService = buildConfigurationService;

            #region /addconnection

            Get["/addconnection"] = _ =>
            {
                return View["Views/AddConnection.cshtml", new AddEditConnectionViewModel
                {
                    NewConfiguration = !_buildConfigurationService.HasConfiguration()
                }];
            };

            Post["/addconnection"] = _ =>
            {
                var model = this.Bind<AddEditConnectionViewModel>();
                var validationResult = this.Validate(model);

                if (!validationResult.IsValid)
                {
                    model.Errors.AddRange(validationResult.Errors.SelectMany(e => e.Value).Select(y => y.ErrorMessage));

                    return View["Views/AddConnection.cshtml", model];
                }

                try
                {
                    _buildConfigurationService.AddConnection(model.TfsServerUrl, model.TfsServerLocation, model.UserName, 
                        model.Password, model.PersonalAccessToken);
                }
                catch (ServiceValidationException s)
                {
                    model.Errors.AddRange(s.ServiceErrors);

                    return View["Views/AddConnection.cshtml", model];
                }

                return Response.AsRedirect("/");
            };

            #endregion

            #region /editconnection

            Get["/editconnection/{id}"] = x =>
            {
                var connection = _buildConfigurationService.GetBuildConfig().Connections.FirstOrDefault(c => c.Id.ToString() == x.id);

                if (connection != null)
                {
                    var viewModel = new AddEditConnectionViewModel
                    {
                        NewConfiguration = false,
                        UserName = connection.UserName,
                        TfsServerUrl = connection.TfsServerUrl,
                        TfsServerLocation = connection.TfsServerDeployment.ToString()
                    };

                    return View["Views/EditConnection.cshtml", viewModel];
                }

                return Response.AsRedirect("/");
            };

            Post["/editconnection/{id}"] = x =>
            {
                var model = this.Bind<AddEditConnectionViewModel>();
                var validationResult = this.Validate(model);

                if (!validationResult.IsValid)
                {
                    model.Errors.AddRange(validationResult.Errors.SelectMany(e => e.Value).Select(y => y.ErrorMessage));

                    return View["Views/EditConnection.cshtml", model];
                }

                try
                {
                    _buildConfigurationService.EditConnection(x.id, model.TfsServerUrl, model.TfsServerLocation, 
                        model.UserName, model.Password, model.PersonalAccessToken);
                }
                catch (ServiceValidationException s)
                {
                    model.Errors.AddRange(s.ServiceErrors);

                    return View["Views/EditConnection.cshtml", model];
                }

                return Response.AsRedirect("/");
            };

            #endregion

            #region /deleteconnection

            Post["/deleteconnection"] = _ =>
            {
                _buildConfigurationService.DeleteConnection((string)Request.Form["ConnectionId"]);

                return Response.AsRedirect("/");
            };

            #endregion

            #region /addproject

            Get["/addproject/{id}"] = x =>
            {
                string id = x.id;

                try
                {
                    var projects = _buildConfigurationService.GetProjects(id, out var connection);

                    var model = new AddProjectViewModel
                    {
                        Connection = connection,
                        ProjectSelections = projects.Select(p => new ProjectSelectionModel
                        {
                            Id = p.Id, Name = p.Name,
                            Disabled = _buildConfigurationService.ProjectExistsInConfig(connection.Id, p.Id)

                        }).ToList()
                    };

                    return View["Views/AddProject.cshtml", model];
                }
                catch (InvalidOperationException)
                {
                    return new NotFoundResponse();
                }
            };

            Post["/addproject"] = x =>
            {
                var connectionId = (string)Request.Form["ConnectionId"];
                var selections = this.Bind<List<ProjectSelectionModel>>();

                _buildConfigurationService.AddProjects(connectionId, 
                    selections.Where(s => s.Selected).Select(s => new Project { Id = s.Id, Name = s.Name }).ToList());

                return Response.AsRedirect("/");
            };

            #endregion

            #region /deleteproject

            Post["/deleteproject"] = _ =>
            {
                _buildConfigurationService.DeleteProject((string)Request.Form["ConnectionId"], (string)Request.Form["ProjectId"]);

                return Response.AsRedirect("/");
            };

            #endregion

            #region /addbuilds

            Get["/addbuilds"] = x =>
            {
                var connectionId = (string)Request.Query["connectionId"];
                var projectId = (string)Request.Query["projectId"];

                if (string.IsNullOrEmpty(connectionId) || string.IsNullOrEmpty(projectId))
                {
                    return new NotFoundResponse();
                }

                try
                {
                    var buildDefinitions =_buildConfigurationService.GetBuildDefinitions(connectionId, projectId, 
                        out var connection, out var project);

                    var model = new AddBuildsViewModel
                    {
                        Connection = connection,
                        Project = project,
                        BuildSelections = buildDefinitions.Select(b => new BuildSelectionModel
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Disabled = _buildConfigurationService.BuildExistsInConfig(connection.Id, project.Id, b.Id)

                        }).ToList(),
                    };

                    return View["Views/AddBuilds.cshtml", model];
                }
                catch (InvalidOperationException)
                {
                    return new NotFoundResponse();
                }
            };

            Post["/addbuilds"] = x =>
            {
                var connectionId = (string)Request.Form["ConnectionId"];
                var projectId = (string)Request.Form["ProjectId"];

                var selections = this.Bind<List<BuildSelectionModel>>();

                _buildConfigurationService.AddBuildDefinitions(connectionId, projectId,
                    selections.Where(s => s.Selected).Select(s => new BuildDefinition { Id = s.Id, Name = s.Name }).ToList());

                return Response.AsRedirect("/");
            };

            #endregion

            #region /deletebuild

            Post["/deletebuild"] = _ =>
            {
                _buildConfigurationService.DeleteBuild((string)Request.Form["LocalDefId"]);

                return Response.AsRedirect("/");
            };

            #endregion
        }
    }

}
