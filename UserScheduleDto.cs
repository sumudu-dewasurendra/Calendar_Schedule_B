using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace acpDtoModel.Models
{
    /// <summary>
    /// Represents the UserScheduleDto.
    /// </summary>
    public class UserScheduleDto : BaseDto
    {
        /// <summary>
        /// Gets or sets user schedule id.
        /// </summary> 
        public int userScheduleId { get; set; }

        /// <summary>
        /// Gets or sets user schedule user id.
        /// </summary>
        [Required]
        public int userId { get; set; }

        /// <summary>
        /// Gets or sets user schedule date.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime date { get; set; }

        /// <summary>
        /// Gets or sets user schedule note.
        /// </summary>
        public string note { get; set; }

        /// <summary>
        /// Gets or sets user schedule schedule.
        /// </summary>
        public string schedule { get; set; }

        /// <summary>
        /// Gets or sets user schedule preview.
        /// </summary>
        public string preview { get; set; }
    }
}
