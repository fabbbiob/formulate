﻿namespace formulate.app.Controllers
{

    // Namespaces.
    using DataValues;
    using Helpers;
    using Models.Requests;
    using Persistence;
    using Resolvers;
    using System;
    using System.Linq;
    using System.Web.Http;
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Web;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.WebApi.Filters;
    using CoreConstants = Umbraco.Core.Constants;
    using DataValuesConstants = formulate.app.Constants.Trees.DataValues;


    /// <summary>
    /// Controller for Formulate data values.
    /// </summary>
    [PluginController("formulate")]
    [UmbracoApplicationAuthorize(CoreConstants.Applications.Users)]
    public class DataValuesController : UmbracoAuthorizedJsonController
    {

        #region Constants

        private const string UnhandledError = @"An unhandled error occurred. Refer to the error log.";
        private const string PersistDataValueError = @"An error occurred while attempting to persist a Formulate data value.";
        private const string GetDataValueInfoError = @"An error occurred while attempting to get the data value info for a Formulate data value.";
        private const string DeleteDataValueError = @"An error occurred while attempting to delete the Formulate data value.";

        #endregion


        #region Properties

        private IDataValuePersistence Persistence { get; set; }
        private IEntityPersistence Entities { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DataValuesController()
            : this(UmbracoContext.Current)
        {
        }


        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="context">Umbraco context.</param>
        public DataValuesController(UmbracoContext context)
            : base(context)
        {
            Persistence = DataValuePersistence.Current.Manager;
            Entities = EntityPersistence.Current.Manager;
        }

        #endregion


        #region Web Methods

        /// <summary>
        /// Creates a data value.
        /// </summary>
        /// <param name="request">
        /// The request to create the data value.
        /// </param>
        /// <returns>
        /// An object indicating success or failure, along with some
        /// accompanying data.
        /// </returns>
        [HttpPost]
        public object PersistDataValue(PersistDataValueRequest request)
        {

            // Variables.
            var result = default(object);
            var rootId = CoreConstants.System.Root.ToInvariantString();
            var dataValuesRootId = GuidHelper.GetGuid(DataValuesConstants.Id);
            var parentId = GuidHelper.GetGuid(request.ParentId);
            var kindId = GuidHelper.GetGuid(request.KindId);


            // Catch all errors.
            try
            {

                // Parse or create the data value ID.
                var dataValueId = string.IsNullOrWhiteSpace(request.DataValueId)
                    ? Guid.NewGuid()
                    : GuidHelper.GetGuid(request.DataValueId);


                // Get the ID path.
                var path = parentId == Guid.Empty
                    ? new[] { dataValuesRootId, dataValueId }
                    : Entities.Retrieve(parentId).Path
                        .Concat(new[] { dataValueId }).ToArray();


                // Create data value.
                var dataValue = new DataValue()
                {
                    KindId = kindId,
                    Id = dataValueId,
                    Path = path,
                    Name = request.DataValueName,
                    Alias = request.DataValueAlias
                };


                // Persist data value.
                Persistence.Persist(dataValue);


                // Variables.
                var fullPath = new[] { rootId }
                    .Concat(path.Select(x => GuidHelper.GetString(x)))
                    .ToArray();


                // Success.
                result = new
                {
                    Success = true,
                    Id = GuidHelper.GetString(dataValueId),
                    Path = fullPath
                };

            }
            catch (Exception ex)
            {

                // Error.
                LogHelper.Error<DataValuesController>(PersistDataValueError, ex);
                result = new
                {
                    Success = false,
                    Reason = UnhandledError
                };

            }


            // Return result.
            return result;

        }


        /// <summary>
        /// Returns info about the data value with the specified ID.
        /// </summary>
        /// <param name="request">
        /// The request to get the data value info.
        /// </param>
        /// <returns>
        /// An object indicating success or failure, along with some
        /// accompanying data.
        /// </returns>
        [HttpGet]
        public object GetDataValueInfo(
            [FromUri] GetDataValueInfoRequest request)
        {

            // Variables.
            var result = default(object);
            var rootId = CoreConstants.System.Root.ToInvariantString();


            // Catch all errors.
            try
            {

                // Variables.
                var id = GuidHelper.GetGuid(request.DataValueId);
                var dataValue = Persistence.Retrieve(id);
                var fullPath = new[] { rootId }
                    .Concat(dataValue.Path.Select(x => GuidHelper.GetString(x)))
                    .ToArray();


                // Set result.
                result = new
                {
                    Success = true,
                    DataValueId = GuidHelper.GetString(dataValue.Id),
                    KindId = GuidHelper.GetString(dataValue.KindId),
                    Path = fullPath,
                    Alias = dataValue.Alias,
                    Name = dataValue.Name
                };

            }
            catch (Exception ex)
            {

                // Error.
                LogHelper.Error<DataValuesController>(GetDataValueInfoError, ex);
                result = new
                {
                    Success = false,
                    Reason = UnhandledError
                };

            }


            // Return result.
            return result;

        }


        /// <summary>
        /// Deletes the data value with the specified ID.
        /// </summary>
        /// <param name="request">
        /// The request to delete the data value.
        /// </param>
        /// <returns>
        /// An object indicating success or failure, along with some
        /// accompanying data.
        /// </returns>
        [HttpPost()]
        public object DeleteDataValue(DeleteDataValueRequest request)
        {

            // Variables.
            var result = default(object);


            // Catch all errors.
            try
            {

                // Variables.
                var dataValueId = GuidHelper.GetGuid(request.DataValueId);


                // Delete the data value.
                Persistence.Delete(dataValueId);


                // Success.
                result = new
                {
                    Success = true
                };

            }
            catch (Exception ex)
            {

                // Error.
                LogHelper.Error<DataValuesController>(DeleteDataValueError, ex);
                result = new
                {
                    Success = false,
                    Reason = UnhandledError
                };

            }


            // Return the result.
            return result;

        }

        #endregion

    }

}