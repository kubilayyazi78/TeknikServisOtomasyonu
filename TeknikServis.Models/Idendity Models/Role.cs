﻿using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeknikServis.Models.Idendity_Models
{
   public class Role : IdentityRole
    {
        public Role()
        {

        }
        public Role(string tanim)
        {
            Tanim = tanim;

        }


        [StringLength(100)]
        public string Tanim { get; set; }
    }
}
