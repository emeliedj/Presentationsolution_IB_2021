using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Presentationsolution_IB_2021
{
    public enum WeatherType 
    {
        Nederbörd, 
        Vindstyrka,
        Grad

    }


    public class WeatherNederbörd : TableEntity
    {
        //private List<WeatherType> _weatherTypes = new List<WeatherType>();

        public string Tid { get; set; }

        public string Nederbörd { get; set; }
  
    }
}
