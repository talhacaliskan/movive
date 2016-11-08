﻿using System.Collections.Generic;
using TMDbLib.Objects.General;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;

namespace TMDbLib.Objects.Find
{
    public class FindContainer
    {
        public List<MovieResult> MovieResults { get; set; }
        public List<Person> PersonResults { get; set; }     // Unconfirmed type
        public List<SearchTv> TvResults { get; set; }
    }
}