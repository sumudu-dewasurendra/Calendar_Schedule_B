using acpCommon.Mappers;
using acpCommon.Repository;
using acpCommon.Services;
using acpDtoModel.Models;
using acpMetaDataStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acpMetaDataLogic.Services
{
    /// <summary>
    /// Inherits BaseService.
    /// </summary>
    public class UserScheduleService : BaseService<UserScheduleDto, userSchedule>
    {
        private DataEntityTransformer entityTransformer;
        private IRepository<userSchedule> userScheduleModelRepository;

        ///<summary>
        ///Constructs UIControlService.
        ///</summary>
        ///<param name="dataEntityTransformer">Instance of dataEntityTransformer.</param>
        ///<param name="companyRepository">Instance of IRepository<company>.</param>
        public UserScheduleService(DataEntityTransformer dataEntityTransformer, IRepository<userSchedule> userScheduleRepository)
            : base(dataEntityTransformer, userScheduleRepository)
        {
            entityTransformer = dataEntityTransformer;
            userScheduleModelRepository = userScheduleRepository;           
        }
    }
}
