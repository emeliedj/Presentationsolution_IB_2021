using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Presentationsolution_IB_2021
{
    public class WeatherVindstyrka : TableEntity
    {

        public string Tid { get; set; }
        public string Vindstyrka { get; set; }
    }
}
