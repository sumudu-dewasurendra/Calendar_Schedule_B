using acpCommon.Authorization;
using acpCommon.Context;
using acpCommon.Creators;
using acpCommon.Services;
using acpCommon.Validators;
using acpDtoModel.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.OData.Query;

namespace acpMetaDataWebApi.Controllers
{
    /// <summary>
    /// REST Endpoints for UserScheduleController.
    /// </summary>
    [RoutePrefix("api/userSchedules")]
    public class UserScheduleController : ApiController
    {
        private IService<UserScheduleDto> userScheduleService = null;

        /// <summary>
        /// Constructs AcpDataTypeController.
        /// </summary>
        /// <param name="userScheduleService">Instance of IService.</param>
        public UserScheduleController(IService<UserScheduleDto> userScheduleService)
        {
            this.userScheduleService = userScheduleService;
        }

        /// <summary>
        /// Gets all standard userSchedules or specific items.
        /// URL - GET:  api/userSchedules
        /// Search - URL : api/userSchedules?$filter=date eq 'varchar(10)'
        /// </summary>
        /// <param name="queryOptionDto">OData Query Options.</param> 
        /// <returns>Enumerates a UserScheduleDto.</returns>
        [HttpGet]
        [Route()]
        [AcpResourceAuthorizeAttribute(AcpVariableContext.ACCESS_READ, AuthorizationPermissions.SUPER_ADMIN_RESOURCES, AuthorizationPermissions.ADMIN_RESOURCES, AuthorizationPermissions.GROUP_ADMIN_RESOURCES, AuthorizationPermissions.GENERAL_DATA_ADMIN_RESOURCES)]
        public IEnumerable<UserScheduleDto> Get(ODataQueryOptions<UserScheduleDto> queryOptions)
        {
            return userScheduleService.search(queryOptions);
        }

        /// <summary>
        /// Gets a acpDataType by userSchedule ID.
        /// URL - GET: api/userSchedules/5
        /// </summary>
        /// <param name="userScheduleId">Unique ID of the userSchedule.</param>
        /// <returns>Returns a userSchedule.</returns>
        [HttpGet]
        [Route("{userId:int}")]
        [AcpResourceAuthorizeAttribute(AcpVariableContext.ACCESS_READ, AuthorizationPermissions.SUPER_ADMIN_RESOURCES, AuthorizationPermissions.ADMIN_RESOURCES, AuthorizationPermissions.GROUP_ADMIN_RESOURCES, AuthorizationPermissions.GENERAL_DATA_ADMIN_RESOURCES)]
        public UserScheduleDto Get(int userId)
        {
            return userScheduleService.findByKey(userId);
        }

        /// <summary>
        /// Add new userSchedule.
        /// URL - POST: api/userSchedules
        /// </summary>
        /// <param name="UserScheduleDto">New UserScheduleDto, represents a new userSchedule.</param>
        /// <returns>HttpResponseMessage indicates success or failure.</returns>
        [HttpPost]
        [Route()]
        [AcpResourceAuthorizeAttribute(AcpVariableContext.ACCESS_ADD, AuthorizationPermissions.SUPER_ADMIN_RESOURCES, AuthorizationPermissions.ADMIN_RESOURCES, AuthorizationPermissions.GROUP_ADMIN_RESOURCES, AuthorizationPermissions.GENERAL_DATA_ADMIN_RESOURCES)]
        public HttpResponseMessage Post(UserScheduleDto UserScheduleDto)
        {
            return HttpResponseCreator<UserScheduleDto>.CreatePostResponse(Request, UserScheduleDto, userScheduleService);
        }

        /// <summary>
        /// Updates an existing userSchedule.
        /// URL -  PUT: api/userSchedules/2
        /// </summary>
        /// <param name="userScheduleId">user schedule id to update</param>
        /// <param name="UserScheduleDto">acpDataTypeDto represents a updated acpDataType details.</param>
        /// <returns>HttpResponseMessage indicates success or failure.</returns>
        [HttpPut]
        [Route("{userScheduleId:int}")]
        [AcpResourceAuthorizeAttribute(AcpVariableContext.ACCESS_UPDATE, AuthorizationPermissions.SUPER_ADMIN_RESOURCES, AuthorizationPermissions.ADMIN_RESOURCES, AuthorizationPermissions.GROUP_ADMIN_RESOURCES, AuthorizationPermissions.GENERAL_DATA_ADMIN_RESOURCES)]
        public HttpResponseMessage Put(int userScheduleId, UserScheduleDto UserScheduleDto)
        {
            return HttpResponseCreator<UserScheduleDto>.CreateUpdateResponse(userScheduleId, UserScheduleDto, userScheduleService);
        }

        /// <summary>
        /// Delete a userSchedule.
        /// URL - DELETE: api/userSchedules/5
        /// </summary>
        /// <param name="acpDataTypeId">userSchedule Id to delete from the system.</param>
        [HttpDelete]
        [Route("{userScheduleId:int}")]
        [AcpResourceAuthorizeAttribute(AcpVariableContext.ACCESS_DELETE, AuthorizationPermissions.SUPER_ADMIN_RESOURCES, AuthorizationPermissions.ADMIN_RESOURCES, AuthorizationPermissions.GROUP_ADMIN_RESOURCES, AuthorizationPermissions.GENERAL_DATA_ADMIN_RESOURCES)]
        public HttpResponseMessage Delete(int userScheduleId)
        {
            return HttpResponseCreator<UserScheduleDto>.CreateDeleteResponse(userScheduleId, userScheduleService);

        }
    }
}
