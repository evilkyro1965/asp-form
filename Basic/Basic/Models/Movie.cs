using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Basic.Models
{
    public class Movie
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Year { get; set; }
    }
}