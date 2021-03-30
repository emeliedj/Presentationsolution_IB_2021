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
   
    public class WeatherSorted : TableEntity
    {
        private List<WeatherType> _weatherTypes = new List<WeatherType>();
       
        public string Tid { get; set; }
        public List<WeatherType> WeatherTypes
        {
            get { return _weatherTypes; }
            set { _weatherTypes = value; }
        }

    }
}
