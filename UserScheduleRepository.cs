using acpCommon.Context;
using acpCommon.Repository;
using acpMetaDataStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acpMetaDataStorage.Repository
{
    /// <summary>
    /// Represents the UIControlRepository.
    /// </summary>
    public class UserScheduleRepository : BaseRepository<userSchedule>
    {
        /// <summary>
        /// Constructs the UserScheduleRepository.
        /// </summary>
        /// <param name="contextFactory">Type of IDbContextFactory.</param>
        public UserScheduleRepository(IDbContextFactory contextFactory)
            : base(contextFactory)
        {
        }
    }
}