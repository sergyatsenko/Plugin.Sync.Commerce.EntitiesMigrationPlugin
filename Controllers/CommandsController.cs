﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogExport.Util;
using Plugin.Sync.Commerce.EntitiesMigration.Commands;
using Plugin.Sync.Commerce.EntitiesMigration.Models;
using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Framework.Conditions;

namespace Plugin.Sync.Commerce.EntitiesMigration.Controllers
{
    /// <summary>
    /// Commands Controller
    /// </summary>
    public class CommandsController : CommerceController
    {
        private readonly GetEnvironmentCommand _getEnvironmentCommand;
        private const string ENV_NAME = "HabitatAuthoring";
        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="serviceProvider">serviceProvider</param>
        /// <param name="globalEnvironment">globalEnvironment</param>
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment, GetEnvironmentCommand getEnvironmentCommand)
            : base(serviceProvider, globalEnvironment)
        {
            _getEnvironmentCommand = getEnvironmentCommand;
        }

        /// <summary>
        /// Interface function ImportComposerTemplate
        /// </summary>
        /// <param name="value">parameter</param>
        /// <returns>Action Result</returns>
        //[HttpPut]
        //[Route("ImportComposerTemplates()")]
        //public async Task<IActionResult> ImportComposerTemplates([FromBody] JArray request) //[FromBody] ODataActionParameters value)
        //{
        //    try
        //    {
        //        Condition.Requires(request).IsNotNull("ImportComposerTemplates: The argument can not be null");
        //        await InitializeEnvironment();
        //        string entityType = request.GetVa["entityType"].ToString();
        //        var command = this.Command<ImportCommerceEntitiesArgument>();
        //        var result = await command.Process(this.CurrentContext, new ImportEntitiesArgument()
        //        {
        //            InputJson = request.ToString(Formatting.None),
        //            //ImportType = ImportType.Override //(ImportType)Enum.Parse(typeof(ImportType), importType)
        //        });

        //        if (result)
        //        {
        //            return new ObjectResult("SUCCESS");
        //        }
        //        else
        //        {
        //            return new NotFoundObjectResult("Error updating entity or entity not found.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ObjectResult(ex);
        //    }
        //}

        /// <summary>
        /// Interface function ImportComposerTemplate
        /// </summary>
        /// <param name="value">parameter</param>
        /// <returns>Action Result</returns>
        //[HttpPut]
        //[Route("ExportComposerTemplates()")]
        //public async Task<IActionResult> ExportComposerTemplates([FromBody] ODataActionParameters value)
        //{
        //    try
        //    {
        //        await InitializeEnvironment();


        //        var command = this.Command<ExportCommerceEntitiesCommand>();
        //        var result = await command.Process(this.CurrentContext);
        //        return new JsonResult(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ObjectResult(ex);
        //    } 
        //}

        [HttpPut]
        [Route("ExportEntities()")]
        public async Task<IActionResult> ExportEntities([FromBody]ODataActionParameters request)
        {
            Condition.Requires<ODataActionParameters>(request, nameof(request)).IsNotNull<ODataActionParameters>();
            if (!request.ContainsKey("entityType") || request["entityType"] == null)
                return (IActionResult)new BadRequestObjectResult((object)request);
            string entityType = request["entityType"].ToString();

            try
            {
                await InitializeEnvironment();

                var exportEntitiesArgument = new ExportEntitiesArgument
                {
                    EntityType = entityType
                };
                var command = this.Command<ExportCommerceEntitiesCommand>();
                var result = await command.Process(this.CurrentContext, exportEntitiesArgument);
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                return new JsonResult(result, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }


        [HttpPut]
        [Route("ImportEntities()")]
        public async Task<IActionResult> ImportEntities([FromBody] ODataActionParameters request)
        {
            if (!this.ModelState.IsValid || request == null)
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            if (!request.ContainsKey("importFile") || request["importFile"] == null)
                return (IActionResult)new BadRequestObjectResult((object)request);

            IFormFile formFile = (IFormFile)request["importFile"];
            MemoryStream memoryStream = new MemoryStream();
            formFile.CopyTo((Stream)memoryStream);
            FormFile file = new FormFile((Stream)memoryStream, 0L, formFile.Length, formFile.Name, formFile.FileName);

            try
            {
                await InitializeEnvironment();

                var sb = new StringBuilder();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        sb.AppendLine(reader.ReadLine());
                }
                var jsonString = sb.ToString();
                var entities = JsonConvert.DeserializeObject<EntityCollectionModel>(jsonString, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All} );
                var importEntitiesArgument = new ImportEntitiesArgument(entities);

                var command = this.Command<ImportCommerceEntitiesCommand>();
                var result = await command.Process(this.CurrentContext, importEntitiesArgument);

                //return new JsonResult(result);
                return (IActionResult)new ObjectResult((object)this.ExecuteLongRunningCommand(() => command.Process(this.CurrentContext, importEntitiesArgument)));
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        private async Task<bool> InitializeEnvironment()
        {
            var commerceEnvironment = await _getEnvironmentCommand.Process(this.CurrentContext, ENV_NAME) ??
                                      this.CurrentContext.Environment;
            var pipelineContextOptions = this.CurrentContext.PipelineContextOptions;
            pipelineContextOptions.CommerceContext.Environment = commerceEnvironment;
            this.CurrentContext.PipelineContextOptions.CommerceContext.Environment = commerceEnvironment;

            return true;
        }
    }
}