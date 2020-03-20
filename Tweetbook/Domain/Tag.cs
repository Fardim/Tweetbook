using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Tweetbook.Domain
{
    public class Tag
    {
        [Key]
        public string Name { get; set; }

        public string CreatorId { get; set; }
        
        [ForeignKey(nameof(CreatorId))]
        public IdentityUser Creator { get; set; }

        public DateTime CreateOn { get; set; }
    }
}
